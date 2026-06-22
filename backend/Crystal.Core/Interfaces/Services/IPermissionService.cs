using Crystal.Core.DTOs.Responses;

namespace Crystal.Core.Interfaces.Services;

public interface IPermissionService
{
    Task<UserPermissionsResponseDto> GetUserPermissionsAsync(string p_userId);
    Task<bool> UserHasPermissionAsync(string p_userId, string p_action, string p_subject);
    Task<bool> UserHasPermissionForLocationAsync(string p_userId, string p_action, string p_subject, int? p_locationId);
    bool RulesGrantPermission(IEnumerable<PermissionRuleDto> p_rules, string p_action, string p_subject);
    bool RulesGrantPermissionForLocation(
        IEnumerable<PermissionRuleDto> p_rules,
        string p_action,
        string p_subject,
        int? p_locationId);
}
