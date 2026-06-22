namespace Crystal.Core.Interfaces.Services;

public interface IEmployeeScopeService
{
    Task<bool> CanManageAsync(string p_userId, string p_subject);

    Task<bool> CanReadAsync(string p_userId, string p_subject);

    Task<bool> CanSubmitAsync(string p_userId, string p_subject);

    Task<int?> GetEmployeeProfileIdAsync(string p_userId);

    Task<int?> GetEmployeeLocationIdAsync(string p_userId);

    Task EnsureCanManageAsync(string p_userId, string p_subject);
}
