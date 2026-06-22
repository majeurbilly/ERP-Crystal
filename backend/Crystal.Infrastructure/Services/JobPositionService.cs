using Crystal.Core.Constants;
using Crystal.Core.DTOs.Requests;
using Crystal.Core.DTOs.Responses;
using Crystal.Core.Entities;
using Crystal.Core.Interfaces.Repositories;
using Crystal.Core.Interfaces.Services;
using Crystal.Infrastructure.Services.Validation;

namespace Crystal.Infrastructure.Services;

public class JobPositionService : IJobPositionService
{
    private readonly IJobPositionRepository m_jobPositionRepository;

    public JobPositionService(IJobPositionRepository p_jobPositionRepository)
    {
        m_jobPositionRepository = p_jobPositionRepository;
    }

    public async Task<IEnumerable<JobPositionResponseDto>> GetAllAsync()
    {
        IEnumerable<JobPosition> jobPositions = await m_jobPositionRepository.GetAllAsync();
        return jobPositions.Select(MapToDto);
    }

    public async Task<JobPositionResponseDto?> GetByIdAsync(int p_id)
    {
        EntityIdentifierValidator.EnsureValid(p_id);

        JobPosition? jobPosition = await m_jobPositionRepository.GetByIdAsync(p_id);

        if (jobPosition is null)
        {
            return null;
        }

        return MapToDto(jobPosition);
    }

    public async Task<JobPositionResponseDto> CreateAsync(CreateJobPositionRequest p_request)
    {
        string normalizedName = NormalizeName(p_request.Name);
        string normalizedDescription = NormalizeDescription(p_request.Description);
        ValidateName(normalizedName);
        ValidateDescription(normalizedDescription);
        string normalizedColor = NormalizeColor(p_request.Color);
        ValidateColor(normalizedColor);

        JobPosition? existingJobPosition = await m_jobPositionRepository.GetByNameAsync(normalizedName);
        if (existingJobPosition is not null)
        {
            throw new InvalidOperationException(ErrorMessages.JobPosition.NameAlreadyExists);
        }

        JobPosition jobPosition = new JobPosition
        {
            Name = normalizedName,
            Description = normalizedDescription,
            Color = normalizedColor,
            IsDeleted = false
        };

        await m_jobPositionRepository.AddAsync(jobPosition);
        await m_jobPositionRepository.SaveChangesAsync();

        return MapToDto(jobPosition);
    }

    public async Task<JobPositionResponseDto> UpdateAsync(int p_id, UpdateJobPositionRequest p_request)
    {
        EntityIdentifierValidator.EnsureValid(p_id);

        string normalizedName = NormalizeName(p_request.Name);
        string normalizedDescription = NormalizeDescription(p_request.Description);
        ValidateName(normalizedName);
        ValidateDescription(normalizedDescription);
        string normalizedColor = NormalizeColor(p_request.Color);
        ValidateColor(normalizedColor);

        JobPosition? existingJobPosition = await m_jobPositionRepository.GetByIdAsync(p_id);
        if (existingJobPosition is null)
        {
            throw new KeyNotFoundException(ErrorMessages.JobPosition.NotFound);
        }

        JobPosition? jobPositionWithSameName = await m_jobPositionRepository.GetByNameAsync(normalizedName);
        if (jobPositionWithSameName is not null && jobPositionWithSameName.Id != p_id)
        {
            throw new InvalidOperationException(ErrorMessages.JobPosition.NameAlreadyExists);
        }

        existingJobPosition.Name = normalizedName;
        existingJobPosition.Description = normalizedDescription;
        existingJobPosition.Color = normalizedColor;

        await m_jobPositionRepository.UpdateAsync(existingJobPosition);
        await m_jobPositionRepository.SaveChangesAsync();

        return MapToDto(existingJobPosition);
    }

    public async Task DeleteAsync(int p_id)
    {
        EntityIdentifierValidator.EnsureValid(p_id);

        JobPosition? existingJobPosition = await m_jobPositionRepository.GetByIdAsync(p_id);
        if (existingJobPosition is null)
        {
            throw new KeyNotFoundException(ErrorMessages.JobPosition.NotFound);
        }

        await m_jobPositionRepository.SoftDeleteAsync(existingJobPosition);
        await m_jobPositionRepository.SaveChangesAsync();
    }

    private static JobPositionResponseDto MapToDto(JobPosition p_jobPosition)
    {
        return new JobPositionResponseDto
        {
            Id = p_jobPosition.Id,
            Name = p_jobPosition.Name,
            Description = p_jobPosition.Description,
            Color = p_jobPosition.Color
        };
    }

    private static string NormalizeColor(string p_color)
    {
        return string.IsNullOrWhiteSpace(p_color) ? "#3B82F6" : p_color.Trim();
    }

    private static void ValidateColor(string p_color)
    {
        if (!System.Text.RegularExpressions.Regex.IsMatch(p_color, "^#[0-9A-Fa-f]{6}$"))
        {
            throw new ArgumentException(ErrorMessages.JobPosition.InvalidColorFormat);
        }
    }

    private static string NormalizeName(string p_name)
    {
        return p_name.Trim();
    }

    private static string NormalizeDescription(string p_description)
    {
        return p_description.Trim();
    }

    private static void ValidateName(string p_normalizedName)
    {
        if (string.IsNullOrWhiteSpace(p_normalizedName))
        {
            throw new ArgumentException(ErrorMessages.JobPosition.NameRequired);
        }

        if (p_normalizedName.Length > 100)
        {
            throw new ArgumentException(ErrorMessages.JobPosition.NameTooLong);
        }
    }

    private static void ValidateDescription(string p_normalizedDescription)
    {
        if (string.IsNullOrWhiteSpace(p_normalizedDescription))
        {
            throw new ArgumentException(ErrorMessages.JobPosition.DescriptionRequired);
        }

        if (p_normalizedDescription.Length > 500)
        {
            throw new ArgumentException(ErrorMessages.JobPosition.DescriptionTooLong);
        }
    }
}
