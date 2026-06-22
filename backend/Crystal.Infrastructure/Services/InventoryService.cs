using Crystal.Core.Constants;
using Crystal.Core.DTOs.Requests;
using Crystal.Core.DTOs.Responses;
using Crystal.Core.Entities;
using Crystal.Core.Interfaces.Repositories;
using Crystal.Core.Interfaces.Services;
using Crystal.Infrastructure.Excel;
using Crystal.Infrastructure.Services.Validation;
using MiniExcelLibs;

namespace Crystal.Infrastructure.Services;

public class InventoryService : IInventoryService
{
    private static readonly HashSet<string> AllowedExcelExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".xlsx"
    };

    private readonly IInventoryRepository m_inventoryRepository;

    public InventoryService(IInventoryRepository p_inventoryRepository)
    {
        m_inventoryRepository = p_inventoryRepository;
    }

    public async Task<IEnumerable<LocationInventoryLineResponseDto>> GetInventoryLinesAsync(
        int? p_locationId,
        int? p_itemId)
    {
        if (p_locationId.HasValue)
        {
            EntityIdentifierValidator.EnsureValid(p_locationId.Value);

            bool locationExists = await m_inventoryRepository.LocationExistsAsync(p_locationId.Value);

            if (!locationExists)
            {
                throw new KeyNotFoundException(ErrorMessages.Location.NotFound);
            }
        }

        if (p_itemId.HasValue)
        {
            EntityIdentifierValidator.EnsureValid(p_itemId.Value);

            bool itemExists = await m_inventoryRepository.IsActiveItemAsync(p_itemId.Value);

            if (!itemExists)
            {
                throw new KeyNotFoundException(ErrorMessages.Inventory.ItemNotFound);
            }
        }

        return await m_inventoryRepository.GetLinesAsync(p_locationId, p_itemId);
    }

    public async Task UpdateQuantityAsync(UpdateInventoryQuantityRequest p_request)
    {
        await UpsertQuantityAsync(p_request.ItemId, p_request.LocationId, p_request.Quantity);
    }

    public async Task AddQuantityAsync(UpdateInventoryQuantityRequest p_request)
    {
        if (p_request.Quantity < 0)
        {
            throw new ArgumentException(ErrorMessages.Inventory.NegativeQuantity);
        }

        bool itemIsActive = await m_inventoryRepository.IsActiveItemAsync(p_request.ItemId);

        if (!itemIsActive)
        {
            throw new ArgumentException(ErrorMessages.Inventory.ItemNotFoundInActiveCatalog);
        }

        bool locationExists = await m_inventoryRepository.LocationExistsAsync(p_request.LocationId);

        if (!locationExists)
        {
            throw new ArgumentException(ErrorMessages.Location.NotFound);
        }

        InventoryLine? line = await m_inventoryRepository.GetLineByItemAndLocationAsync(
            p_request.ItemId,
            p_request.LocationId);

        if (line is null)
        {
            line = new InventoryLine
            {
                ItemId = p_request.ItemId,
                LocationId = p_request.LocationId,
                Quantity = p_request.Quantity
            };

            m_inventoryRepository.AddLine(line);
        }
        else
        {
            line.Quantity += p_request.Quantity;
        }

        await m_inventoryRepository.SaveChangesAsync();
    }

    public async Task<InventoryStockResponseDto> GetStockAsync(int p_locationId, int p_itemId)
    {
        EntityIdentifierValidator.EnsureValid(p_locationId);
        EntityIdentifierValidator.EnsureValid(p_itemId);

        bool itemIsActive = await m_inventoryRepository.IsActiveItemAsync(p_itemId);

        if (!itemIsActive)
        {
            throw new KeyNotFoundException(ErrorMessages.Inventory.ItemNotFound);
        }

        bool locationExists = await m_inventoryRepository.LocationExistsAsync(p_locationId);

        if (!locationExists)
        {
            throw new KeyNotFoundException(ErrorMessages.Location.NotFound);
        }

        InventoryLine? line = await m_inventoryRepository.GetLineByItemAndLocationReadOnlyAsync(
            p_itemId,
            p_locationId);

        int quantity = line?.Quantity ?? 0;

        return new InventoryStockResponseDto
        {
            LocationId = p_locationId,
            ItemId = p_itemId,
            Quantity = quantity
        };
    }

    public async Task SetStockAsync(int p_locationId, int p_itemId, UpdateStockRequest p_request)
    {
        await UpsertQuantityAsync(p_itemId, p_locationId, p_request.Quantity);
    }

    public async Task ImportFromExcelAsync(Stream p_fileStream, string p_fileName)
    {
        if (p_fileStream == null || p_fileStream.Length == 0)
        {
            throw new ArgumentException(ErrorMessages.Inventory.ExcelFileRequired);
        }

        if (string.IsNullOrWhiteSpace(p_fileName))
        {
            throw new ArgumentException(ErrorMessages.Item.FileNameRequired);
        }

        string extension = Path.GetExtension(p_fileName);

        if (!AllowedExcelExtensions.Contains(extension))
        {
            throw new ArgumentException(ErrorMessages.Inventory.ExcelOnly);
        }

        List<InventoryExcelRow> rows;

        try
        {
            IEnumerable<InventoryExcelRow> queryResult = p_fileStream.Query<InventoryExcelRow>(excelType: ExcelType.XLSX);
            rows = queryResult.ToList();
        }
        catch (Exception p_ex)
        {
            throw new InvalidOperationException(ErrorMessages.Inventory.InvalidExcelFormat, p_ex);
        }

        if (rows.Count == 0)
        {
            throw new ArgumentException(ErrorMessages.Inventory.EmptyExcelFile);
        }

        int rowNumber = 1;

        foreach (InventoryExcelRow row in rows)
        {
            if (row.LocationId <= 0 || row.ItemId <= 0)
            {
                throw new ArgumentException(string.Format(ErrorMessages.Inventory.ExcelRowInvalidIds, rowNumber));
            }

            if (row.Quantity < 0)
            {
                throw new ArgumentException(string.Format(ErrorMessages.Inventory.ExcelRowNegativeQuantity, rowNumber));
            }

            bool itemExists = await m_inventoryRepository.ItemExistsAsync(row.ItemId);

            if (!itemExists)
            {
                throw new ArgumentException(string.Format(ErrorMessages.Inventory.ExcelRowItemNotFound, rowNumber, row.ItemId));
            }

            bool locationExists = await m_inventoryRepository.LocationExistsAsync(row.LocationId);

            if (!locationExists)
            {
                throw new ArgumentException(string.Format(ErrorMessages.Inventory.ExcelRowLocationNotFound, rowNumber, row.LocationId));
            }

            InventoryLine? line = await m_inventoryRepository.GetLineByItemAndLocationAsync(
                row.ItemId,
                row.LocationId);

            if (line is null)
            {
                line = new InventoryLine
                {
                    ItemId = row.ItemId,
                    LocationId = row.LocationId,
                    Quantity = row.Quantity
                };

                m_inventoryRepository.AddLine(line);
            }
            else
            {
                line.Quantity = row.Quantity;
            }

            rowNumber++;
        }

        await m_inventoryRepository.SaveChangesAsync();
    }

    private async Task UpsertQuantityAsync(int p_itemId, int p_locationId, int p_quantity)
    {
        if (p_quantity < 0)
        {
            throw new ArgumentException(ErrorMessages.Inventory.NegativeQuantity);
        }

        bool itemIsActive = await m_inventoryRepository.IsActiveItemAsync(p_itemId);

        if (!itemIsActive)
        {
            throw new ArgumentException(ErrorMessages.Inventory.ItemNotFoundInActiveCatalog);
        }

        bool locationExists = await m_inventoryRepository.LocationExistsAsync(p_locationId);

        if (!locationExists)
        {
            throw new ArgumentException(ErrorMessages.Location.NotFound);
        }

        InventoryLine? line = await m_inventoryRepository.GetLineByItemAndLocationAsync(
            p_itemId,
            p_locationId);

        if (line is null)
        {
            line = new InventoryLine
            {
                ItemId = p_itemId,
                LocationId = p_locationId,
                Quantity = p_quantity
            };

            m_inventoryRepository.AddLine(line);
        }
        else
        {
            line.Quantity = p_quantity;
        }

        await m_inventoryRepository.SaveChangesAsync();
    }
}
