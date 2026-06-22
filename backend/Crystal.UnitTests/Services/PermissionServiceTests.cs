using Crystal.Core.Authorization;
using Crystal.Core.DTOs.Responses;
using Crystal.Core.Entities;
using Crystal.Core.Interfaces.Repositories;
using Crystal.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace Crystal.UnitTests.Services;

public sealed class PermissionServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> m_userManagerMock;
    private readonly Mock<IDynamicRoleRepository> m_dynamicRoleRepositoryMock;
    private readonly PermissionService m_service;

    public PermissionServiceTests()
    {
        m_userManagerMock = new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(),
            null!, null!, null!, null!, null!, null!, null!, null!);

        m_dynamicRoleRepositoryMock = new Mock<IDynamicRoleRepository>();
        m_service = new PermissionService(m_userManagerMock.Object, m_dynamicRoleRepositoryMock.Object);
    }

    [Fact]
    public void RulesGrantPermission_ReturnsTrue_WhenManageAllPresent()
    {
        List<PermissionRuleDto> rules =
        [
            new() { Action = PermissionActions.Manage, Subject = PermissionSubjects.All },
        ];

        bool result = m_service.RulesGrantPermission(rules, PermissionActions.Create, PermissionSubjects.EmployeeProfile);

        Assert.True(result);
    }

    [Fact]
    public void RulesGrantPermission_ReturnsTrue_WhenManageSubjectPresent()
    {
        List<PermissionRuleDto> rules =
        [
            new() { Action = PermissionActions.Manage, Subject = PermissionSubjects.EmployeeProfile },
        ];

        bool result = m_service.RulesGrantPermission(rules, PermissionActions.Delete, PermissionSubjects.EmployeeProfile);

        Assert.True(result);
    }

    [Fact]
    public void RulesGrantPermission_ReturnsFalse_WhenExactRuleMissing()
    {
        List<PermissionRuleDto> rules =
        [
            new() { Action = PermissionActions.Read, Subject = PermissionSubjects.Item },
        ];

        bool result = m_service.RulesGrantPermission(rules, PermissionActions.Create, PermissionSubjects.EmployeeProfile);

        Assert.False(result);
    }

    [Fact]
    public void RulesGrantPermission_ReturnsTrue_WhenExactRuleMatches()
    {
        List<PermissionRuleDto> rules =
        [
            new() { Action = PermissionActions.Create, Subject = PermissionSubjects.LeaveRequest },
        ];

        bool result = m_service.RulesGrantPermission(rules, PermissionActions.Create, PermissionSubjects.LeaveRequest);

        Assert.True(result);
    }

    [Fact]
    public void RulesGrantPermissionForLocation_ReturnsTrue_WhenManageAllPresent()
    {
        List<PermissionRuleDto> rules =
        [
            new() { Action = PermissionActions.Manage, Subject = PermissionSubjects.All },
        ];

        bool result = m_service.RulesGrantPermissionForLocation(
            rules,
            PermissionActions.Update,
            PermissionSubjects.InventoryQuantity,
            99);

        Assert.True(result);
    }

    [Fact]
    public void RulesGrantPermissionForLocation_ReturnsTrue_WhenInventoryScopeIsAll()
    {
        List<PermissionRuleDto> rules =
        [
            new()
            {
                Action = PermissionActions.Update,
                Subject = PermissionSubjects.InventoryQuantity,
                LocationScope = LocationScopes.All,
            },
        ];

        bool resultLocationOne = m_service.RulesGrantPermissionForLocation(
            rules,
            PermissionActions.Update,
            PermissionSubjects.InventoryQuantity,
            1);

        bool resultLocationTwo = m_service.RulesGrantPermissionForLocation(
            rules,
            PermissionActions.Update,
            PermissionSubjects.InventoryQuantity,
            2);

        Assert.True(resultLocationOne);
        Assert.True(resultLocationTwo);
    }

    [Fact]
    public void RulesGrantPermissionForLocation_ReturnsTrue_OnlyForAllowedSpecificLocations()
    {
        List<PermissionRuleDto> rules =
        [
            new()
            {
                Action = PermissionActions.Update,
                Subject = PermissionSubjects.InventoryQuantity,
                LocationScope = LocationScopes.Specific,
                LocationIds = [1],
            },
        ];

        bool resultAllowedLocation = m_service.RulesGrantPermissionForLocation(
            rules,
            PermissionActions.Update,
            PermissionSubjects.InventoryQuantity,
            1);

        bool resultDeniedLocation = m_service.RulesGrantPermissionForLocation(
            rules,
            PermissionActions.Update,
            PermissionSubjects.InventoryQuantity,
            2);

        Assert.True(resultAllowedLocation);
        Assert.False(resultDeniedLocation);
    }

    [Fact]
    public void RulesGrantPermissionForLocation_RestrictsManageInventory_ToSpecificScope()
    {
        List<PermissionRuleDto> rules =
        [
            new()
            {
                Action = PermissionActions.Manage,
                Subject = PermissionSubjects.InventoryQuantity,
                LocationScope = LocationScopes.Specific,
                LocationIds = [2],
            },
        ];

        bool resultAllowedLocation = m_service.RulesGrantPermissionForLocation(
            rules,
            PermissionActions.Update,
            PermissionSubjects.InventoryQuantity,
            2);

        bool resultDeniedLocation = m_service.RulesGrantPermissionForLocation(
            rules,
            PermissionActions.Update,
            PermissionSubjects.InventoryQuantity,
            1);

        Assert.True(resultAllowedLocation);
        Assert.False(resultDeniedLocation);
    }

    [Fact]
    public void RulesGrantPermissionForLocation_ReturnsFalse_WhenInventoryScopeIsNull()
    {
        List<PermissionRuleDto> rules =
        [
            new()
            {
                Action = PermissionActions.Update,
                Subject = PermissionSubjects.InventoryQuantity,
                LocationScope = null,
            },
        ];

        bool result = m_service.RulesGrantPermissionForLocation(
            rules,
            PermissionActions.Update,
            PermissionSubjects.InventoryQuantity,
            1);

        Assert.False(result);
    }

    [Fact]
    public void RulesGrantPermissionForLocation_ReturnsFalse_WhenSpecificScopeWithoutMatchingLocationId()
    {
        List<PermissionRuleDto> rules =
        [
            new()
            {
                Action = PermissionActions.Update,
                Subject = PermissionSubjects.InventoryQuantity,
                LocationScope = LocationScopes.Specific,
                LocationIds = [1],
            },
        ];

        bool result = m_service.RulesGrantPermissionForLocation(
            rules,
            PermissionActions.Update,
            PermissionSubjects.InventoryQuantity,
            null);

        Assert.False(result);
    }

    [Fact]
    public void RulesGrantPermissionForLocation_DelegatesToGlobalRules_ForNonInventorySubjects()
    {
        List<PermissionRuleDto> rules =
        [
            new() { Action = PermissionActions.Read, Subject = PermissionSubjects.Item },
        ];

        bool result = m_service.RulesGrantPermissionForLocation(
            rules,
            PermissionActions.Read,
            PermissionSubjects.Item,
            1);

        Assert.True(result);
    }

    [Fact]
    public async Task UserHasPermissionForLocationAsync_ReturnsTrue_WhenRoleHasScopedInventoryAccess()
    {
        ApplicationUser user = new()
        {
            Id = "user-1",
            DynamicRoleId = "custom-role",
        };

        DynamicRole role = new()
        {
            Id = "custom-role",
            Name = "Employee (Saint-Foy)",
            Permissions =
            [
                new RolePermission
                {
                    Action = PermissionActions.Update,
                    Subject = PermissionSubjects.InventoryQuantity,
                    LocationScope = LocationScopes.Specific,
                    ScopedLocations =
                    [
                        new RolePermissionLocation { LocationId = 1 },
                    ],
                },
            ],
        };

        m_userManagerMock
            .Setup(p_manager => p_manager.FindByIdAsync(user.Id))
            .ReturnsAsync(user);

        m_dynamicRoleRepositoryMock
            .Setup(p_repository => p_repository.GetByIdWithPermissionsAsync(role.Id))
            .ReturnsAsync(role);

        bool result = await m_service.UserHasPermissionForLocationAsync(
            user.Id,
            PermissionActions.Update,
            PermissionSubjects.InventoryQuantity,
            1);

        Assert.True(result);
    }
}
