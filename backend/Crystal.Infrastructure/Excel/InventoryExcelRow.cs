namespace Crystal.Infrastructure.Excel;

/// <summary>
/// Ligne attendue dans le fichier d'import inventaire (.xlsx).
/// </summary>
public class InventoryExcelRow
{
    public int LocationId { get; set; }
    public int ItemId { get; set; }
    public int Quantity { get; set; }
}
