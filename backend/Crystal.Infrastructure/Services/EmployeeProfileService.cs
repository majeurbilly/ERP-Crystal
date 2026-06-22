using Crystal.Core.Authorization;
using Crystal.Core.Constants;
using Crystal.Core.DTOs.Requests;
using Crystal.Core.DTOs.Responses;
using Crystal.Core.Entities;
using Crystal.Core.Interfaces.Repositories;
using Crystal.Core.Interfaces.Services;
using Crystal.Infrastructure.Services.Validation;
using Microsoft.AspNetCore.Identity;

namespace Crystal.Infrastructure.Services;

public class EmployeeProfileService : IEmployeeProfileService
{
    private readonly IEmployeeProfileRepository m_employeeProfileRepository;
    private readonly IJobPositionRepository m_jobPositionRepository;
    private readonly ILocationRepository m_locationRepository;
    private readonly UserManager<ApplicationUser> m_userManager;
    private readonly IEmployeeScopeService m_employeeScopeService;

    public EmployeeProfileService(
        IEmployeeProfileRepository p_employeeProfileRepository,
        IJobPositionRepository p_jobPositionRepository,
        ILocationRepository p_locationRepository,
        UserManager<ApplicationUser> p_userManager,
        IEmployeeScopeService p_employeeScopeService)
    {
        m_employeeProfileRepository = p_employeeProfileRepository;
        m_jobPositionRepository = p_jobPositionRepository;
        m_locationRepository = p_locationRepository;
        m_userManager = p_userManager;
        m_employeeScopeService = p_employeeScopeService;
    }

    public async Task<IEnumerable<EmployeeProfileResponseDto>> GetAllAsync(string p_userId)
    {
        await m_employeeScopeService.EnsureCanManageAsync(p_userId, PermissionSubjects.EmployeeProfile);

        IEnumerable<EmployeeProfile> employeeProfiles = await m_employeeProfileRepository.GetAllAsync();
        return employeeProfiles.Select(MapToDto);
    }

    public async Task<EmployeeProfileResponseDto?> GetByIdAsync(int p_id, string p_userId)
    {
        EntityIdentifierValidator.EnsureValid(p_id);

        EmployeeProfile? employeeProfile = await m_employeeProfileRepository.GetByIdAsync(p_id);

        if (employeeProfile is null)
        {
            return null;
        }

        if (!await m_employeeScopeService.CanManageAsync(p_userId, PermissionSubjects.EmployeeProfile))
        {
            int? ownProfileId = await m_employeeScopeService.GetEmployeeProfileIdAsync(p_userId);
            if (!ownProfileId.HasValue || employeeProfile.Id != ownProfileId.Value)
            {
                return null;
            }
        }

        return MapToDto(employeeProfile);
    }

    public async Task<EmployeeProfileResponseDto> CreateAsync(CreateEmployeeProfileRequest p_request)
    {
        string normalizedEmail = NormalizeEmail(p_request.Email);
        ValidateProfileFields(
            p_request.FirstName,
            p_request.LastName,
            normalizedEmail,
            p_request.Status,
            p_request.Salary);

        await EnsureEmailIsUniqueAsync(normalizedEmail, null);
        int jobPositionId = await ResolveJobPositionIdAsync(p_request.JobPositionId);
        await EnsureLocationExistsAsync(p_request.LocationId);

        string? applicationUserId = NormalizeApplicationUserId(p_request.ApplicationUserId);
        await EnsureApplicationUserIsValidAsync(applicationUserId, null);

        EmployeeProfile employeeProfile = new EmployeeProfile
        {
            FirstName = NormalizeName(p_request.FirstName),
            LastName = NormalizeName(p_request.LastName),
            Email = normalizedEmail,
            ApplicationUserId = applicationUserId,
            Salary = p_request.Salary,
            Status = NormalizeStatus(p_request.Status),
            PositionId = jobPositionId,
            HiringDate = p_request.HiringDate,
            LocationId = p_request.LocationId,
            IsDeleted = false
        };

        await m_employeeProfileRepository.AddAsync(employeeProfile);
        await m_employeeProfileRepository.SaveChangesAsync();

        EmployeeProfile? createdProfile = await m_employeeProfileRepository.GetByIdAsync(employeeProfile.Id);
        if (createdProfile is null)
        {
            throw new InvalidOperationException(ErrorMessages.EmployeeProfile.CreateRetrievalFailed);
        }

        return MapToDto(createdProfile);
    }

    public async Task<EmployeeProfileResponseDto> UpdateAsync(int p_id, UpdateEmployeeProfileRequest p_request)
    {
        EntityIdentifierValidator.EnsureValid(p_id);

        string normalizedEmail = NormalizeEmail(p_request.Email);
        ValidateProfileFields(
            p_request.FirstName,
            p_request.LastName,
            normalizedEmail,
            p_request.Status,
            p_request.Salary);

        EmployeeProfile? existingProfile = await m_employeeProfileRepository.GetTrackedByIdAsync(p_id);
        if (existingProfile is null)
        {
            throw new KeyNotFoundException(ErrorMessages.EmployeeProfile.NotFound);
        }

        await EnsureEmailIsUniqueAsync(normalizedEmail, p_id);
        int jobPositionId = p_request.JobPositionId.HasValue && p_request.JobPositionId.Value > 0
            ? p_request.JobPositionId.Value
            : existingProfile.PositionId;
        await EnsureJobPositionExistsAsync(jobPositionId);
        await EnsureLocationExistsAsync(p_request.LocationId);

        string? applicationUserId = NormalizeApplicationUserId(p_request.ApplicationUserId);
        await EnsureApplicationUserIsValidAsync(applicationUserId, p_id);

        existingProfile.FirstName = NormalizeName(p_request.FirstName);
        existingProfile.LastName = NormalizeName(p_request.LastName);
        existingProfile.Email = normalizedEmail;
        existingProfile.ApplicationUserId = applicationUserId;
        existingProfile.Salary = p_request.Salary;
        existingProfile.Status = NormalizeStatus(p_request.Status);
        existingProfile.PositionId = jobPositionId;
        existingProfile.HiringDate = p_request.HiringDate;
        existingProfile.LocationId = p_request.LocationId;

        await m_employeeProfileRepository.UpdateAsync(existingProfile);
        await m_employeeProfileRepository.SaveChangesAsync();

        EmployeeProfile? updatedProfile = await m_employeeProfileRepository.GetByIdAsync(p_id);
        if (updatedProfile is null)
        {
            throw new InvalidOperationException(ErrorMessages.EmployeeProfile.UpdateRetrievalFailed);
        }

        return MapToDto(updatedProfile);
    }

    public async Task DeleteAsync(int p_id)
    {
        EntityIdentifierValidator.EnsureValid(p_id);

        EmployeeProfile? existingProfile = await m_employeeProfileRepository.GetTrackedByIdAsync(p_id);
        if (existingProfile is null)
        {
            throw new KeyNotFoundException(ErrorMessages.EmployeeProfile.NotFound);
        }

        await m_employeeProfileRepository.SoftDeleteAsync(existingProfile);
        await m_employeeProfileRepository.SaveChangesAsync();
    }

    public async Task<EmployeeProfileResponseDto> GetMyProfileAsync(string p_applicationUserId)
    {
        if (string.IsNullOrWhiteSpace(p_applicationUserId))
        {
            throw new ArgumentException(ErrorMessages.EmployeeProfile.InvalidUserAccountIdentifier);
        }

        EmployeeProfile? employeeProfile = await m_employeeProfileRepository
            .GetByApplicationUserIdAsync(p_applicationUserId);

        if (employeeProfile is null)
        {
            throw new KeyNotFoundException(ErrorMessages.EmployeeProfile.NoProfileLinkedToUserAccount);
        }

        return MapToDto(employeeProfile);
    }

    private async Task EnsureEmailIsUniqueAsync(string p_email, int? p_excludeId)
    {
        bool isUnique = await m_employeeProfileRepository.IsEmailUniqueAsync(p_email, p_excludeId);
        if (!isUnique)
        {
            throw new InvalidOperationException(ErrorMessages.EmployeeProfile.EmailAlreadyExists);
        }
    }

    private async Task<int> ResolveJobPositionIdAsync(int? p_jobPositionId)
    {
        if (p_jobPositionId.HasValue && p_jobPositionId.Value > 0)
        {
            await EnsureJobPositionExistsAsync(p_jobPositionId.Value);
            return p_jobPositionId.Value;
        }

        JobPosition? defaultPosition = (await m_jobPositionRepository.GetAllAsync()).FirstOrDefault();
        if (defaultPosition is null)
        {
            throw new InvalidOperationException(ErrorMessages.EmployeeProfile.NoJobPositionAvailable);
        }

        return defaultPosition.Id;
    }

    private async Task EnsureJobPositionExistsAsync(int p_jobPositionId)
    {
        EntityIdentifierValidator.EnsureValid(p_jobPositionId);

        JobPosition? jobPosition = await m_jobPositionRepository.GetByIdAsync(p_jobPositionId);
        if (jobPosition is null)
        {
            throw new InvalidOperationException(ErrorMessages.EmployeeProfile.JobPositionNotFound);
        }
    }

    private async Task EnsureLocationExistsAsync(int? p_locationId)
    {
        if (!p_locationId.HasValue)
        {
            return;
        }

        EntityIdentifierValidator.EnsureValid(p_locationId.Value);

        Location? location = await m_locationRepository.GetByIdAsync(p_locationId.Value);
        if (location is null)
        {
            throw new InvalidOperationException(ErrorMessages.EmployeeProfile.LocationNotFound);
        }
    }

    private async Task EnsureApplicationUserIsValidAsync(string? p_applicationUserId, int? p_excludeProfileId)
    {
        if (p_applicationUserId is null)
        {
            return;
        }

        ApplicationUser? applicationUser = await m_userManager.FindByIdAsync(p_applicationUserId);
        if (applicationUser is null)
        {
            throw new KeyNotFoundException(ErrorMessages.EmployeeProfile.UserAccountNotFound);
        }

        bool isAvailable = await m_employeeProfileRepository
            .IsApplicationUserIdAvailableAsync(p_applicationUserId, p_excludeProfileId);

        if (!isAvailable)
        {
            throw new InvalidOperationException(ErrorMessages.EmployeeProfile.UserAlreadyLinked);
        }
    }

    private static EmployeeProfileResponseDto MapToDto(EmployeeProfile p_employeeProfile)
    {
        return new EmployeeProfileResponseDto
        {
            Id = p_employeeProfile.Id,
            FirstName = p_employeeProfile.FirstName,
            LastName = p_employeeProfile.LastName,
            Email = p_employeeProfile.Email,
            ApplicationUserId = p_employeeProfile.ApplicationUserId,
            HiringDate = p_employeeProfile.HiringDate,
            Salary = p_employeeProfile.Salary,
            Status = p_employeeProfile.Status,
            JobPositionId = p_employeeProfile.PositionId,
            JobPositionName = p_employeeProfile.JobPosition.Name,
            LocationId = p_employeeProfile.LocationId,
            LocationTitle = p_employeeProfile.Location?.Title
        };
    }

    private static string NormalizeName(string p_name)
    {
        return p_name.Trim();
    }

    private static string NormalizeEmail(string p_email)
    {
        return p_email.Trim();
    }

    private static string NormalizeStatus(string p_status)
    {
        return p_status.Trim();
    }

    private static string? NormalizeApplicationUserId(string? p_applicationUserId)
    {
        if (string.IsNullOrWhiteSpace(p_applicationUserId))
        {
            return null;
        }

        return p_applicationUserId.Trim();
    }

    private static void ValidateProfileFields(
        string p_firstName,
        string p_lastName,
        string p_email,
        string p_status,
        decimal p_salary)
    {
        string normalizedFirstName = NormalizeName(p_firstName);
        string normalizedLastName = NormalizeName(p_lastName);

        if (string.IsNullOrWhiteSpace(normalizedFirstName))
        {
            throw new ArgumentException(ErrorMessages.EmployeeProfile.FirstNameRequired);
        }

        if (normalizedFirstName.Length > 100)
        {
            throw new ArgumentException(ErrorMessages.EmployeeProfile.FirstNameTooLong);
        }

        if (string.IsNullOrWhiteSpace(normalizedLastName))
        {
            throw new ArgumentException(ErrorMessages.EmployeeProfile.LastNameRequired);
        }

        if (normalizedLastName.Length > 100)
        {
            throw new ArgumentException(ErrorMessages.EmployeeProfile.LastNameTooLong);
        }

        if (string.IsNullOrWhiteSpace(p_email))
        {
            throw new ArgumentException(ErrorMessages.EmployeeProfile.EmailRequired);
        }

        if (p_email.Length > 256)
        {
            throw new ArgumentException(ErrorMessages.EmployeeProfile.EmailTooLong);
        }

        string normalizedStatus = NormalizeStatus(p_status);
        if (string.IsNullOrWhiteSpace(normalizedStatus))
        {
            throw new ArgumentException(ErrorMessages.EmployeeProfile.StatusRequired);
        }

        if (normalizedStatus.Length > 50)
        {
            throw new ArgumentException(ErrorMessages.EmployeeProfile.StatusTooLong);
        }

        if (p_salary < 0)
        {
            throw new ArgumentException(ErrorMessages.EmployeeProfile.NegativeSalary);
        }
    }
}
