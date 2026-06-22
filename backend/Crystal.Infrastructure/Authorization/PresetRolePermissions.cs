using Crystal.Core;
using Crystal.Core.Authorization;
using Crystal.Core.DTOs.Responses;
using Crystal.Core.Entities;

namespace Crystal.Infrastructure.Authorization;

public static class PresetRolePermissions
{
    public static IReadOnlyList<(string Id, string Name, IReadOnlyList<PermissionRuleDto> Permissions)> AllPresets =>
    [
        (ApplicationRoles.Admin, "Administrator", AdminPermissions),
        (ApplicationRoles.Gerant, "Branch Manager", GerantPermissions),
        (ApplicationRoles.Assistant, "Assistant", AssistantPermissions),
        (ApplicationRoles.Employee, "Employee", EmployeePermissions),
    ];

    private static readonly IReadOnlyList<PermissionRuleDto> AdminPermissions =
    [
        new() { Action = PermissionActions.Manage, Subject = PermissionSubjects.All }
    ];

    private static readonly IReadOnlyList<PermissionRuleDto> SelfAccountPermissions =
    [
        Rule(PermissionActions.Read, PermissionSubjects.Me),
        Rule(PermissionActions.Update, PermissionSubjects.Me),
    ];

    private static readonly IReadOnlyList<PermissionRuleDto> GerantPermissions =
    [
        .. SelfAccountPermissions,
        Rule(PermissionActions.Manage, PermissionSubjects.User),
        Rule(PermissionActions.Read, PermissionSubjects.HrDashboard),
        Rule(PermissionActions.Manage, PermissionSubjects.EmployeeProfile),
        Rule(PermissionActions.Manage, PermissionSubjects.LeaveRequest),
        Rule(PermissionActions.Manage, PermissionSubjects.ScheduledShift),
        Rule(PermissionActions.Manage, PermissionSubjects.TimeEntry),
        Rule(PermissionActions.Manage, PermissionSubjects.Timesheet),
        Rule(PermissionActions.Manage, PermissionSubjects.Payroll),
        Rule(PermissionActions.Manage, PermissionSubjects.EmploymentContract),
        Rule(PermissionActions.Read, PermissionSubjects.Location),
        Rule(PermissionActions.Update, PermissionSubjects.Location),
        Rule(PermissionActions.Read, PermissionSubjects.Item),
        Rule(PermissionActions.Manage, PermissionSubjects.Item),
        InventoryRule(PermissionActions.Read, LocationScopes.All),
        InventoryRule(PermissionActions.Update, LocationScopes.All),
        Rule(PermissionActions.Read, PermissionSubjects.Category),
        Rule(PermissionActions.Create, PermissionSubjects.Category),
        Rule(PermissionActions.Update, PermissionSubjects.Category),
        Rule(PermissionActions.Manage, PermissionSubjects.JobPosition),
        Rule(PermissionActions.Read, PermissionSubjects.Author),
        Rule(PermissionActions.Create, PermissionSubjects.Author),
        Rule(PermissionActions.Update, PermissionSubjects.Author),
    ];

    private static readonly IReadOnlyList<PermissionRuleDto> AssistantPermissions =
    [
        .. SelfAccountPermissions,
        Rule(PermissionActions.Read, PermissionSubjects.HrDashboard),
        Rule(PermissionActions.Read, PermissionSubjects.EmployeeProfile),
        Rule(PermissionActions.Create, PermissionSubjects.LeaveRequest),
        Rule(PermissionActions.Read, PermissionSubjects.LeaveRequest),
        Rule(PermissionActions.Read, PermissionSubjects.ScheduledShift),
        Rule(PermissionActions.Read, PermissionSubjects.TimeEntry),
        Rule(PermissionActions.Create, PermissionSubjects.TimeEntry),
        Rule(PermissionActions.Read, PermissionSubjects.Timesheet),
        Rule(PermissionActions.Read, PermissionSubjects.Payroll),
        Rule(PermissionActions.Read, PermissionSubjects.EmploymentContract),
        Rule(PermissionActions.Submit, PermissionSubjects.Timesheet),
        Rule(PermissionActions.Create, PermissionSubjects.Timesheet),
        Rule(PermissionActions.Read, PermissionSubjects.Item),
        Rule(PermissionActions.Create, PermissionSubjects.Item),
        InventoryRule(PermissionActions.Read, LocationScopes.All),
        Rule(PermissionActions.Read, PermissionSubjects.Location),
        Rule(PermissionActions.Read, PermissionSubjects.Category),
        Rule(PermissionActions.Read, PermissionSubjects.JobPosition),
        Rule(PermissionActions.Read, PermissionSubjects.Author),
    ];

    private static readonly IReadOnlyList<PermissionRuleDto> EmployeePermissions =
    [
        .. SelfAccountPermissions,
        Rule(PermissionActions.Read, PermissionSubjects.EmployeeProfile),
        Rule(PermissionActions.Create, PermissionSubjects.LeaveRequest),
        Rule(PermissionActions.Read, PermissionSubjects.LeaveRequest),
        Rule(PermissionActions.Read, PermissionSubjects.ScheduledShift),
        Rule(PermissionActions.Read, PermissionSubjects.TimeEntry),
        Rule(PermissionActions.Create, PermissionSubjects.TimeEntry),
        Rule(PermissionActions.Read, PermissionSubjects.Timesheet),
        Rule(PermissionActions.Read, PermissionSubjects.Payroll),
        Rule(PermissionActions.Read, PermissionSubjects.EmploymentContract),
        Rule(PermissionActions.Read, PermissionSubjects.Item),
        Rule(PermissionActions.Read, PermissionSubjects.Location),
        Rule(PermissionActions.Read, PermissionSubjects.Category),
        Rule(PermissionActions.Read, PermissionSubjects.JobPosition),
        Rule(PermissionActions.Read, PermissionSubjects.Author),
    ];

    public static DynamicRole CreatePresetEntity(string p_id, string p_name, IEnumerable<PermissionRuleDto> p_permissions)
    {
        DynamicRole role = new()
        {
            Id = p_id,
            Name = p_name,
            IsPreset = true,
        };

        foreach (PermissionRuleDto permission in p_permissions)
        {
            RolePermission rolePermission = new()
            {
                Action = permission.Action,
                Subject = permission.Subject,
                LocationScope = permission.LocationScope,
            };

            if (permission.LocationScope == LocationScopes.Specific)
            {
                foreach (int locationId in permission.LocationIds)
                {
                    rolePermission.ScopedLocations.Add(new RolePermissionLocation
                    {
                        LocationId = locationId,
                    });
                }
            }

            role.Permissions.Add(rolePermission);
        }

        return role;
    }

    private static PermissionRuleDto Rule(string p_action, string p_subject) =>
        new() { Action = p_action, Subject = p_subject };

    private static PermissionRuleDto InventoryRule(string p_action, string p_locationScope, IReadOnlyList<int>? p_locationIds = null) =>
        new()
        {
            Action = p_action,
            Subject = PermissionSubjects.InventoryQuantity,
            LocationScope = p_locationScope,
            LocationIds = p_locationIds?.ToList() ?? new List<int>(),
        };
}
