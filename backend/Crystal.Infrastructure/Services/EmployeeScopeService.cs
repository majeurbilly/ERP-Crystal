using Crystal.Core.Authorization;
using Crystal.Core.Constants;
using Crystal.Core.Interfaces.Repositories;
using Crystal.Core.Interfaces.Services;

namespace Crystal.Infrastructure.Services;

public class EmployeeScopeService : IEmployeeScopeService
{
    private readonly IPermissionService m_permissionService;
    private readonly IEmployeeProfileRepository m_employeeProfileRepository;

    public EmployeeScopeService(
        IPermissionService p_permissionService,
        IEmployeeProfileRepository p_employeeProfileRepository)
    {
        m_permissionService = p_permissionService;
        m_employeeProfileRepository = p_employeeProfileRepository;
    }

    public async Task<bool> CanManageAsync(string p_userId, string p_subject)
    {
        return await m_permissionService.UserHasPermissionAsync(
            p_userId,
            PermissionActions.Manage,
            p_subject);
    }

    public async Task<bool> CanReadAsync(string p_userId, string p_subject)
    {
        return await m_permissionService.UserHasPermissionAsync(
            p_userId,
            PermissionActions.Read,
            p_subject);
    }

    public async Task<bool> CanSubmitAsync(string p_userId, string p_subject)
    {
        return await m_permissionService.UserHasPermissionAsync(
            p_userId,
            PermissionActions.Submit,
            p_subject);
    }

    public async Task<int?> GetEmployeeProfileIdAsync(string p_userId)
    {
        Crystal.Core.Entities.EmployeeProfile? profile =
            await m_employeeProfileRepository.GetByApplicationUserIdAsync(p_userId);

        return profile?.Id;
    }

    public async Task<int?> GetEmployeeLocationIdAsync(string p_userId)
    {
        Crystal.Core.Entities.EmployeeProfile? profile =
            await m_employeeProfileRepository.GetByApplicationUserIdAsync(p_userId);

        return profile?.LocationId;
    }

    public async Task EnsureCanManageAsync(string p_userId, string p_subject)
    {
        if (!await CanManageAsync(p_userId, p_subject))
        {
            throw new UnauthorizedAccessException(ErrorMessages.AccessDenied);
        }
    }
}
