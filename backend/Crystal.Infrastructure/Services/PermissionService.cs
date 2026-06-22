using Crystal.Core.Authorization;
using Crystal.Core.Constants;
using Crystal.Core.DTOs.Responses;
using Crystal.Core.Entities;
using Crystal.Core.Interfaces.Repositories;
using Crystal.Core.Interfaces.Services;
using Microsoft.AspNetCore.Identity;

namespace Crystal.Infrastructure.Services;

public class PermissionService : IPermissionService
{
    private readonly UserManager<ApplicationUser> m_userManager;
    private readonly IDynamicRoleRepository m_dynamicRoleRepository;

    public PermissionService(
        UserManager<ApplicationUser> p_userManager,
        IDynamicRoleRepository p_dynamicRoleRepository)
    {
        m_userManager = p_userManager;
        m_dynamicRoleRepository = p_dynamicRoleRepository;
    }

    public async Task<UserPermissionsResponseDto> GetUserPermissionsAsync(string p_userId)
    {
        ApplicationUser? user = await m_userManager.FindByIdAsync(p_userId);
        if (user is null)
        {
            throw new KeyNotFoundException(ErrorMessages.Permission.UserNotFound);
        }

        string roleId = await ResolveEffectiveRoleIdAsync(user);
        DynamicRole? role = await m_dynamicRoleRepository.GetByIdWithPermissionsAsync(roleId);

        if (role is null)
        {
            throw new KeyNotFoundException(ErrorMessages.Permission.DynamicRoleNotFound);
        }

        return new UserPermissionsResponseDto
        {
            RoleId = role.Id,
            RoleName = role.Name,
            Permissions = role.Permissions
                .Select(p_permission => new PermissionRuleDto
                {
                    Action = p_permission.Action,
                    Subject = p_permission.Subject,
                    LocationScope = p_permission.LocationScope,
                    LocationIds = p_permission.ScopedLocations
                        .Select(p_scopedLocation => p_scopedLocation.LocationId)
                        .OrderBy(p_locationId => p_locationId)
                        .ToList(),
                })
                .ToList(),
        };
    }

    public async Task<bool> UserHasPermissionAsync(string p_userId, string p_action, string p_subject)
    {
        UserPermissionsResponseDto permissions = await GetUserPermissionsAsync(p_userId);
        return RulesGrantPermission(permissions.Permissions, p_action, p_subject);
    }

    public async Task<bool> UserHasPermissionForLocationAsync(
        string p_userId,
        string p_action,
        string p_subject,
        int? p_locationId)
    {
        UserPermissionsResponseDto permissions = await GetUserPermissionsAsync(p_userId);
        return RulesGrantPermissionForLocation(permissions.Permissions, p_action, p_subject, p_locationId);
    }

    public bool RulesGrantPermission(IEnumerable<PermissionRuleDto> p_rules, string p_action, string p_subject)
    {
        List<PermissionRuleDto> ruleList = p_rules.ToList();

        if (ruleList.Any(p_rule =>
                p_rule.Action == PermissionActions.Manage && p_rule.Subject == PermissionSubjects.All))
        {
            return true;
        }

        if (ruleList.Any(p_rule =>
                p_rule.Action == PermissionActions.Manage && p_rule.Subject == p_subject))
        {
            return true;
        }

        return ruleList.Any(p_rule =>
            p_rule.Action == p_action && p_rule.Subject == p_subject);
    }

    public bool RulesGrantPermissionForLocation(
        IEnumerable<PermissionRuleDto> p_rules,
        string p_action,
        string p_subject,
        int? p_locationId)
    {
        List<PermissionRuleDto> ruleList = p_rules.ToList();

        if (ruleList.Any(p_rule =>
                p_rule.Action == PermissionActions.Manage && p_rule.Subject == PermissionSubjects.All))
        {
            return true;
        }

        if (p_subject != PermissionSubjects.InventoryQuantity)
        {
            return RulesGrantPermission(p_rules, p_action, p_subject);
        }

        return ruleList.Any(p_rule => InventoryRuleGrantsActionAtLocation(p_rule, p_action, p_locationId));
    }

    private static bool InventoryRuleGrantsActionAtLocation(
        PermissionRuleDto p_rule,
        string p_action,
        int? p_locationId)
    {
        if (p_rule.Subject != PermissionSubjects.InventoryQuantity)
        {
            return false;
        }

        if (p_rule.Action != p_action && p_rule.Action != PermissionActions.Manage)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(p_rule.LocationScope))
        {
            return false;
        }

        if (p_rule.LocationScope == LocationScopes.All)
        {
            return true;
        }

        return p_rule.LocationScope == LocationScopes.Specific
            && p_locationId.HasValue
            && p_rule.LocationIds.Contains(p_locationId.Value);
    }

    private Task<string> ResolveEffectiveRoleIdAsync(ApplicationUser p_user)
    {
        if (string.IsNullOrWhiteSpace(p_user.DynamicRoleId))
        {
            throw new InvalidOperationException(ErrorMessages.Permission.UserHasNoDynamicRole);
        }

        return Task.FromResult(p_user.DynamicRoleId);
    }
}
