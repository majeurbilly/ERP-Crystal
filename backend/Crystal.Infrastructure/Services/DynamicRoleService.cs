using Crystal.Core.Authorization;
using Crystal.Core.Constants;
using Crystal.Core.DTOs.Requests;
using Crystal.Core.DTOs.Responses;
using Crystal.Core.Entities;
using Crystal.Core.Interfaces.Repositories;
using Crystal.Core.Interfaces.Services;
using Crystal.Infrastructure.Authorization;
using Crystal.Infrastructure.Services.Validation;

namespace Crystal.Infrastructure.Services;

public class DynamicRoleService : IDynamicRoleService
{
    private readonly IDynamicRoleRepository m_dynamicRoleRepository;
    private readonly ILocationRepository m_locationRepository;

    public DynamicRoleService(
        IDynamicRoleRepository p_dynamicRoleRepository,
        ILocationRepository p_locationRepository)
    {
        m_dynamicRoleRepository = p_dynamicRoleRepository;
        m_locationRepository = p_locationRepository;
    }

    public async Task<IEnumerable<DynamicRoleResponseDto>> GetAllAsync()
    {
        IEnumerable<DynamicRole> roles = await m_dynamicRoleRepository.GetAllAsync();
        return roles.Select(MapToDto);
    }

    public async Task<DynamicRoleResponseDto?> GetByIdAsync(string p_id)
    {
        DynamicRole? role = await m_dynamicRoleRepository.GetByIdWithPermissionsAsync(p_id);
        return role is null ? null : MapToDto(role);
    }

    public async Task<DynamicRoleResponseDto> CreateAsync(CreateDynamicRoleRequest p_request)
    {
        string normalizedName = NormalizeName(p_request.Name);
        ValidateName(normalizedName);

        IList<PermissionRuleRequest> permissions = ResolvePermissionsForCreate(p_request);

        string roleId = Guid.NewGuid().ToString("N");

        DynamicRole role = new()
        {
            Id = roleId,
            Name = normalizedName,
            IsPreset = false,
        };

        await ApplyPermissionsAsync(role, permissions);

        await m_dynamicRoleRepository.AddAsync(role);
        await m_dynamicRoleRepository.SaveChangesAsync();

        DynamicRole? created = await m_dynamicRoleRepository.GetByIdWithPermissionsAsync(roleId);
        if (created is null)
        {
            throw new InvalidOperationException(ErrorMessages.DynamicRole.CreateRetrievalFailed);
        }

        return MapToDto(created);
    }

    public async Task<DynamicRoleResponseDto> UpdateAsync(string p_id, UpdateDynamicRoleRequest p_request)
    {
        DynamicRole? existingRole = await m_dynamicRoleRepository.GetByIdWithPermissionsAsync(p_id);
        if (existingRole is null)
        {
            throw new KeyNotFoundException(ErrorMessages.DynamicRole.NotFound);
        }

        if (existingRole.IsPreset)
        {
            throw new InvalidOperationException(ErrorMessages.DynamicRole.PresetRolesCannotBeModified);
        }

        string normalizedName = NormalizeName(p_request.Name);
        ValidateName(normalizedName);

        existingRole.Name = normalizedName;
        existingRole.Permissions.Clear();
        await ApplyPermissionsAsync(existingRole, p_request.Permissions);

        await m_dynamicRoleRepository.UpdateAsync(existingRole);
        await m_dynamicRoleRepository.SaveChangesAsync();

        DynamicRole? updated = await m_dynamicRoleRepository.GetByIdWithPermissionsAsync(p_id);
        if (updated is null)
        {
            throw new InvalidOperationException(ErrorMessages.DynamicRole.UpdateRetrievalFailed);
        }

        return MapToDto(updated);
    }

    public async Task DeleteAsync(string p_id)
    {
        DynamicRole? existingRole = await m_dynamicRoleRepository.GetByIdWithPermissionsAsync(p_id);
        if (existingRole is null)
        {
            throw new KeyNotFoundException(ErrorMessages.DynamicRole.NotFound);
        }

        if (existingRole.IsPreset)
        {
            throw new InvalidOperationException(ErrorMessages.DynamicRole.PresetRolesCannotBeDeleted);
        }

        int assignedUsers = await m_dynamicRoleRepository.CountUsersAssignedAsync(p_id);
        if (assignedUsers > 0)
        {
            throw new InvalidOperationException(ErrorMessages.DynamicRole.RoleAssignedToUsers);
        }

        await m_dynamicRoleRepository.DeleteAsync(existingRole);
        await m_dynamicRoleRepository.SaveChangesAsync();
    }

    public IEnumerable<PermissionEntityResponseDto> GetPermissionEntities()
    {
        return PermissionSubjects.AllEntities
            .Select(p_subject => new PermissionEntityResponseDto { Id = p_subject });
    }

    private static IList<PermissionRuleRequest> ResolvePermissionsForCreate(CreateDynamicRoleRequest p_request)
    {
        if (!string.IsNullOrWhiteSpace(p_request.PresetId))
        {
            (string Id, string Name, IReadOnlyList<PermissionRuleDto> Permissions)? preset =
                PresetRolePermissions.AllPresets.FirstOrDefault(p_item => p_item.Id == p_request.PresetId);

            if (preset is null || string.IsNullOrWhiteSpace(preset.Value.Id))
            {
                throw new ArgumentException(ErrorMessages.DynamicRole.InvalidPreset);
            }

            return preset.Value.Permissions
                .Select(p_rule => new PermissionRuleRequest
                {
                    Action = p_rule.Action,
                    Subject = p_rule.Subject,
                    LocationScope = p_rule.LocationScope,
                    LocationIds = p_rule.LocationIds.ToList(),
                })
                .ToList();
        }

        if (p_request.Permissions.Count == 0)
        {
            throw new ArgumentException(ErrorMessages.DynamicRole.AtLeastOnePermissionRequired);
        }

        return p_request.Permissions;
    }

    private async Task ApplyPermissionsAsync(DynamicRole p_role, IEnumerable<PermissionRuleRequest> p_permissions)
    {
        HashSet<string> seen = new();

        foreach (PermissionRuleRequest permission in p_permissions)
        {
            string action = permission.Action.Trim().ToLowerInvariant();
            string subject = permission.Subject.Trim().ToLowerInvariant();
            ValidatePermission(action, subject);
            ValidatePermissionScope(action, subject, permission.LocationScope, permission.LocationIds);

            string key = $"{action}:{subject}";
            if (!seen.Add(key))
            {
                continue;
            }

            RolePermission rolePermission = new()
            {
                Action = action,
                Subject = subject,
            };

            if (subject == PermissionSubjects.InventoryQuantity)
            {
                rolePermission.LocationScope = permission.LocationScope!.Trim().ToLowerInvariant();

                if (rolePermission.LocationScope == LocationScopes.Specific)
                {
                    await AddScopedLocationsAsync(rolePermission, permission.LocationIds);
                }
            }

            p_role.Permissions.Add(rolePermission);
        }
    }

    private async Task AddScopedLocationsAsync(RolePermission p_rolePermission, IEnumerable<int> p_locationIds)
    {
        HashSet<int> uniqueLocationIds = new();

        foreach (int locationId in p_locationIds)
        {
            if (!uniqueLocationIds.Add(locationId))
            {
                continue;
            }

            EntityIdentifierValidator.EnsureValid(locationId);

            Location? location = await m_locationRepository.GetByIdAsync(locationId);
            if (location is null)
            {
                throw new ArgumentException(string.Format(ErrorMessages.DynamicRole.LocationNotFoundWithId, locationId));
            }

            p_rolePermission.ScopedLocations.Add(new RolePermissionLocation
            {
                LocationId = locationId,
            });
        }
    }

    private static void ValidatePermission(string p_action, string p_subject)
    {
        if (!PermissionActions.All.Contains(p_action))
        {
            throw new ArgumentException(string.Format(ErrorMessages.DynamicRole.InvalidPermissionAction, p_action));
        }

        if (p_subject != PermissionSubjects.All
            && p_subject != PermissionSubjects.Me
            && !PermissionSubjects.AllEntities.Contains(p_subject))
        {
            throw new ArgumentException(string.Format(ErrorMessages.DynamicRole.InvalidPermissionSubject, p_subject));
        }
    }

    private static void ValidatePermissionScope(
        string p_action,
        string p_subject,
        string? p_locationScope,
        IEnumerable<int> p_locationIds)
    {
        bool isInventorySubject = p_subject == PermissionSubjects.InventoryQuantity;
        List<int> locationIds = p_locationIds.ToList();

        if (!isInventorySubject)
        {
            if (!string.IsNullOrWhiteSpace(p_locationScope))
            {
                throw new ArgumentException(ErrorMessages.DynamicRole.LocationScopeInventoryOnly);
            }

            if (locationIds.Count > 0)
            {
                throw new ArgumentException(ErrorMessages.DynamicRole.SpecificLocationsInventoryOnly);
            }

            return;
        }

        if (string.IsNullOrWhiteSpace(p_locationScope))
        {
            throw new ArgumentException(ErrorMessages.DynamicRole.LocationScopeRequiredForInventory);
        }

        string normalizedScope = p_locationScope.Trim().ToLowerInvariant();
        if (!LocationScopes.AllValues.Contains(normalizedScope))
        {
            throw new ArgumentException(string.Format(ErrorMessages.DynamicRole.InvalidLocationScope, p_locationScope));
        }

        if (normalizedScope == LocationScopes.All)
        {
            if (locationIds.Count > 0)
            {
                throw new ArgumentException(ErrorMessages.DynamicRole.SpecificLocationsNotAllowedWhenScopeIsAll);
            }

            return;
        }

        if (locationIds.Count == 0)
        {
            throw new ArgumentException(ErrorMessages.DynamicRole.AtLeastOneLocationRequired);
        }

        foreach (int locationId in locationIds)
        {
            EntityIdentifierValidator.EnsureValid(locationId);
        }
    }

    private static string NormalizeName(string p_name) => p_name.Trim();

    private static void ValidateName(string p_name)
    {
        if (string.IsNullOrWhiteSpace(p_name))
        {
            throw new ArgumentException(ErrorMessages.DynamicRole.NameRequired);
        }

        if (p_name.Length > 128)
        {
            throw new ArgumentException(ErrorMessages.DynamicRole.NameTooLong);
        }
    }

    private static DynamicRoleResponseDto MapToDto(DynamicRole p_role)
    {
        return new DynamicRoleResponseDto
        {
            Id = p_role.Id,
            Name = p_role.Name,
            IsPreset = p_role.IsPreset,
            Permissions = p_role.Permissions
                .Select(MapPermissionToDto)
                .ToList(),
        };
    }

    private static PermissionRuleDto MapPermissionToDto(RolePermission p_permission)
    {
        return new PermissionRuleDto
        {
            Action = p_permission.Action,
            Subject = p_permission.Subject,
            LocationScope = p_permission.LocationScope,
            LocationIds = p_permission.ScopedLocations
                .Select(p_scopedLocation => p_scopedLocation.LocationId)
                .OrderBy(p_locationId => p_locationId)
                .ToList(),
        };
    }
}
