using Crystal.Core;
using Crystal.Core.Authorization;
using Crystal.Core.Constants;
using Crystal.Core.Entities;
using Crystal.Core.Enums;
using Crystal.Infrastructure.Authorization;
using Crystal.Infrastructure.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace Crystal.Infrastructure.Data;

public static class DataSeeder
{
    private const string DefaultPassword = "ValidPass1!a";

    public static async Task SeedUsersAsync(IServiceProvider p_serviceProvider)
    {
        UserManager<ApplicationUser> userManager = p_serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        (string Email, string DynamicRoleId)[] testAccounts =
        [
            ("admin@crystal.local", ApplicationRoles.Admin),
            ("gerant@crystal.local", ApplicationRoles.Gerant),
            ("assistant@crystal.local", ApplicationRoles.Assistant),
            ("employee@crystal.local", ApplicationRoles.Employee),
        ];

        foreach ((string Email, string DynamicRoleId) account in testAccounts)
        {
            string email = account.Email;

            ApplicationUser? user = await userManager.FindByEmailAsync(email).ConfigureAwait(false)
                ?? await userManager.FindByNameAsync(email).ConfigureAwait(false);

            if (user is null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    DynamicRoleId = account.DynamicRoleId,
                };

                Console.WriteLine($"[DataSeeder] CreateAsync - before user creation: {email}");
                IdentityResult result = await userManager.CreateAsync(user, DefaultPassword).ConfigureAwait(false);
                Console.WriteLine($"[DataSeeder] CreateAsync - after user creation: {email}, Succeeded={result.Succeeded}");

                if (!result.Succeeded)
                {
                    foreach (IdentityError err in result.Errors)
                    {
                        Console.WriteLine($"[DataSeeder] Identity error: Code={err.Code}, Description={err.Description}");
                    }

                    continue;
                }
            }
            else if (user.DynamicRoleId != account.DynamicRoleId)
            {
                user.DynamicRoleId = account.DynamicRoleId;
                await userManager.UpdateAsync(user).ConfigureAwait(false);
            }
        }
    }

    public static async Task BackfillUserDynamicRolesAsync(IServiceProvider p_serviceProvider)
    {
        UserManager<ApplicationUser> userManager = p_serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        CrystalDbContext context = p_serviceProvider.GetRequiredService<CrystalDbContext>();

        List<ApplicationUser> usersWithoutDynamicRole = await context.Users
            .Where(p_user => p_user.IsActive)
            .Where(p_user => p_user.DynamicRoleId == null || p_user.DynamicRoleId == string.Empty)
            .ToListAsync()
            .ConfigureAwait(false);

        foreach (ApplicationUser user in usersWithoutDynamicRole)
        {
            user.DynamicRoleId = ApplicationRoles.Employee;
            await userManager.UpdateAsync(user).ConfigureAwait(false);
        }
    }

    public static async Task SeedLocationsAsync(IServiceProvider p_serviceProvider)
    {
        CrystalDbContext context = p_serviceProvider.GetRequiredService<CrystalDbContext>();

        if (await context.Locations.AnyAsync().ConfigureAwait(false))
        {
            return;
        }

        Location[] locations =
        [
            new Location
            {
                Title = SeedDataConstants.QuebecCityBranchTitle,
                Address = "123 rue Saint-Jean, Québec, QC",
                Description = "Succursale principale"
            },
            new Location
            {
                Title = SeedDataConstants.SainteFoyBranchTitle,
                Address = "2450 boulevard Laurier, Québec, QC",
                Description = "Succursale secondaire"
            }
        ];

        await context.Locations.AddRangeAsync(locations).ConfigureAwait(false);
        await context.SaveChangesAsync().ConfigureAwait(false);
    }

    public static async Task SeedItemsAndBooksAsync(IServiceProvider p_serviceProvider)
    {
        CrystalDbContext context = p_serviceProvider.GetRequiredService<CrystalDbContext>();

        if (await context.Items.AnyAsync().ConfigureAwait(false))
        {
            return;
        }

        Location? quebecLocation = await context.Locations
            .FirstOrDefaultAsync(p_l => p_l.Title == SeedDataConstants.QuebecCityBranchTitle)
            .ConfigureAwait(false);

        Location? sainteFoyLocation = await context.Locations
            .FirstOrDefaultAsync(p_l => p_l.Title == SeedDataConstants.SainteFoyBranchTitle)
            .ConfigureAwait(false);

        if (quebecLocation is null || sainteFoyLocation is null)
        {
            return;
        }

        Item cleanCodeItem = new()
        {
            Name = SeedDataConstants.CleanCodeItemName,
            Description = "Ouvrage sur les bonnes pratiques de programmation",
            Price = 49.99m,
            AlertQuantity = 5,
            IsActive = true,
            LastUpdate = DateTime.UtcNow
        };

        Item pragmaticProgrammerItem = new()
        {
            Name = SeedDataConstants.PragmaticProgrammerItemName,
            Description = "Ouvrage sur le développement logiciel professionnel",
            Price = 54.99m,
            AlertQuantity = 4,
            IsActive = true,
            LastUpdate = DateTime.UtcNow
        };

        Item keyboardItem = new()
        {
            Name = "Clavier mécanique",
            Description = "Clavier mécanique pour postes de travail",
            Price = 119.99m,
            AlertQuantity = 3,
            IsActive = true,
            LastUpdate = DateTime.UtcNow
        };

        Item mouseItem = new()
        {
            Name = "Souris sans fil",
            Description = "Souris sans fil ergonomique",
            Price = 39.99m,
            AlertQuantity = 6,
            IsActive = true,
            LastUpdate = DateTime.UtcNow
        };

        await context.Items.AddRangeAsync(
            cleanCodeItem,
            pragmaticProgrammerItem,
            keyboardItem,
            mouseItem
        ).ConfigureAwait(false);

        await context.SaveChangesAsync().ConfigureAwait(false);

        Book cleanCodeBook = new()
        {
            ItemId = cleanCodeItem.Id,
            PublicationDate = new DateOnly(2008, 8, 1)
        };

        Book pragmaticProgrammerBook = new()
        {
            ItemId = pragmaticProgrammerItem.Id,
            PublicationDate = new DateOnly(1999, 10, 20)
        };

        await context.Books.AddRangeAsync(
            cleanCodeBook,
            pragmaticProgrammerBook
        ).ConfigureAwait(false);

        await context.InventoryLines.AddRangeAsync(
            new InventoryLine
            {
                ItemId = cleanCodeItem.Id,
                LocationId = quebecLocation.Id,
                Quantity = 8
            },
            new InventoryLine
            {
                ItemId = cleanCodeItem.Id,
                LocationId = sainteFoyLocation.Id,
                Quantity = 2
            },
            new InventoryLine
            {
                ItemId = pragmaticProgrammerItem.Id,
                LocationId = quebecLocation.Id,
                Quantity = 3
            },
            new InventoryLine
            {
                ItemId = keyboardItem.Id,
                LocationId = quebecLocation.Id,
                Quantity = 10
            },
            new InventoryLine
            {
                ItemId = mouseItem.Id,
                LocationId = sainteFoyLocation.Id,
                Quantity = 12
            }
        ).ConfigureAwait(false);

        await context.SaveChangesAsync().ConfigureAwait(false);
    }

    public static async Task SeedAuthorsAsync(IServiceProvider p_serviceProvider)
    {
        CrystalDbContext context = p_serviceProvider.GetRequiredService<CrystalDbContext>();

        if (await context.Authors.AnyAsync().ConfigureAwait(false))
        {
            return;
        }

        Author[] authors =
        [
            new Author { Name = "Robert C. Martin" },
            new Author { Name = "Andrew Hunt" },
            new Author { Name = "David Thomas" },
        ];

        await context.Authors.AddRangeAsync(authors).ConfigureAwait(false);
        await context.SaveChangesAsync().ConfigureAwait(false);
    }

    public static async Task SeedHrReferenceDataAsync(IServiceProvider p_serviceProvider)
    {
        CrystalDbContext context = p_serviceProvider.GetRequiredService<CrystalDbContext>();
        UserManager<ApplicationUser> userManager = p_serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        if (await context.JobPositions.AnyAsync().ConfigureAwait(false))
        {
            return;
        }

        Location? quebecLocation = await context.Locations
            .FirstOrDefaultAsync(p_l => p_l.Title == SeedDataConstants.QuebecCityBranchTitle)
            .ConfigureAwait(false);

        Location? sainteFoyLocation = await context.Locations
            .FirstOrDefaultAsync(p_l => p_l.Title == SeedDataConstants.SainteFoyBranchTitle)
            .ConfigureAwait(false);

        if (quebecLocation is null || sainteFoyLocation is null)
        {
            return;
        }

        JobPosition commisPosition = new() { Name = SeedDataConstants.SalesAssociatePositionName, Description = "Commis en vente au détail" };
        JobPosition caissierPosition = new() { Name = SeedDataConstants.CashierPositionName, Description = "Caissier" };
        JobPosition gerantPosition = new() { Name = SeedDataConstants.BranchManagerPositionName, Description = "Gestion de succursale" };

        await context.JobPositions.AddRangeAsync(commisPosition, caissierPosition, gerantPosition).ConfigureAwait(false);
        await context.SaveChangesAsync().ConfigureAwait(false);

        (string Email, string FirstName, string LastName, int LocationId, string PositionName)[] profiles =
        [
            ("admin@crystal.local", "Alex", "Gagnon", quebecLocation.Id, gerantPosition.Name),
            ("gerant@crystal.local", "Gabriel", "Bergeron", quebecLocation.Id, gerantPosition.Name),
            ("assistant@crystal.local", "Aline", "Roy", sainteFoyLocation.Id, commisPosition.Name),
            ("employee@crystal.local", "Emily", "Tremblay", sainteFoyLocation.Id, caissierPosition.Name),
        ];

        foreach ((string Email, string FirstName, string LastName, int LocationId, string PositionName) profile in profiles)
        {
            ApplicationUser? user = await userManager.FindByEmailAsync(profile.Email).ConfigureAwait(false);
            if (user is null)
            {
                continue;
            }

            bool alreadyLinked = await context.EmployeeProfiles
                .AnyAsync(p_ep => p_ep.ApplicationUserId == user.Id)
                .ConfigureAwait(false);

            if (alreadyLinked)
            {
                continue;
            }

            JobPosition? jobPosition = await context.JobPositions
                .FirstOrDefaultAsync(p_jp => p_jp.Name == profile.PositionName)
                .ConfigureAwait(false);

            if (jobPosition is null)
            {
                continue;
            }

            EmployeeProfile employeeProfile = new()
            {
                FirstName = profile.FirstName,
                LastName = profile.LastName,
                Email = profile.Email,
                ApplicationUserId = user.Id,
                Salary = profile.PositionName == gerantPosition.Name ? 65000m : 43000m,
                Status = "Active",
                PositionId = jobPosition.Id,
                HiringDate = new DateOnly(2024, 1, 15),
                LocationId = profile.LocationId,
                IsDeleted = false,
            };

            await context.EmployeeProfiles.AddAsync(employeeProfile).ConfigureAwait(false);
        }

        await context.SaveChangesAsync().ConfigureAwait(false);
    }

    public static async Task SeedDynamicRolesAsync(IServiceProvider p_serviceProvider)
    {
        CrystalDbContext context = p_serviceProvider.GetRequiredService<CrystalDbContext>();

        if (!await context.DynamicRoles.AnyAsync().ConfigureAwait(false))
        {
            foreach ((string Id, string Name, IReadOnlyList<Crystal.Core.DTOs.Responses.PermissionRuleDto> Permissions) preset
                in PresetRolePermissions.AllPresets)
            {
                DynamicRole role = PresetRolePermissions.CreatePresetEntity(preset.Id, preset.Name, preset.Permissions);
                await context.DynamicRoles.AddAsync(role).ConfigureAwait(false);
            }

            await context.SaveChangesAsync().ConfigureAwait(false);
        }

        await SyncPresetRoleInventoryScopesAsync(p_serviceProvider).ConfigureAwait(false);
        await SyncPresetRolePermissionsAsync(p_serviceProvider).ConfigureAwait(false);
    }

    public static async Task SyncPresetRolePermissionsAsync(IServiceProvider p_serviceProvider)
    {
        CrystalDbContext context = p_serviceProvider.GetRequiredService<CrystalDbContext>();
        bool hasChanges = false;

        foreach ((string Id, string Name, IReadOnlyList<Crystal.Core.DTOs.Responses.PermissionRuleDto> Permissions) preset
            in PresetRolePermissions.AllPresets)
        {
            DynamicRole? role = await context.DynamicRoles
                .Include(p_dynamicRole => p_dynamicRole.Permissions)
                    .ThenInclude(p_permission => p_permission.ScopedLocations)
                .FirstOrDefaultAsync(p_dynamicRole => p_dynamicRole.Id == preset.Id && p_dynamicRole.IsPreset)
                .ConfigureAwait(false);

            if (role is null)
            {
                continue;
            }

            foreach (Crystal.Core.DTOs.Responses.PermissionRuleDto expectedRule in preset.Permissions)
            {
                bool exists = role.Permissions.Any(p_permission =>
                    p_permission.Action == expectedRule.Action
                    && p_permission.Subject == expectedRule.Subject
                    && (p_permission.LocationScope ?? string.Empty) == (expectedRule.LocationScope ?? string.Empty));

                if (exists)
                {
                    continue;
                }

                RolePermission newPermission = new()
                {
                    Action = expectedRule.Action,
                    Subject = expectedRule.Subject,
                    LocationScope = expectedRule.LocationScope,
                };

                if (expectedRule.LocationScope == LocationScopes.Specific)
                {
                    foreach (int locationId in expectedRule.LocationIds)
                    {
                        newPermission.ScopedLocations.Add(new RolePermissionLocation
                        {
                            LocationId = locationId,
                        });
                    }
                }

                role.Permissions.Add(newPermission);
                hasChanges = true;
            }
        }

        if (hasChanges)
        {
            await context.SaveChangesAsync().ConfigureAwait(false);
        }
    }

    public static async Task SyncPresetRoleInventoryScopesAsync(IServiceProvider p_serviceProvider)
    {
        CrystalDbContext context = p_serviceProvider.GetRequiredService<CrystalDbContext>();
        bool hasChanges = false;

        List<RolePermission> legacyInventoryPermissions = await context.RolePermissions
            .Include(p_permission => p_permission.ScopedLocations)
            .Where(p_permission => p_permission.Subject == PermissionSubjects.InventoryQuantity)
            .Where(p_permission => p_permission.LocationScope == null || p_permission.LocationScope == string.Empty)
            .ToListAsync()
            .ConfigureAwait(false);

        foreach (RolePermission permission in legacyInventoryPermissions)
        {
            permission.LocationScope = LocationScopes.All;
            permission.ScopedLocations.Clear();
            hasChanges = true;
        }

        foreach ((string Id, string Name, IReadOnlyList<Crystal.Core.DTOs.Responses.PermissionRuleDto> Permissions) preset
            in PresetRolePermissions.AllPresets)
        {
            DynamicRole? role = await context.DynamicRoles
                .Include(p_dynamicRole => p_dynamicRole.Permissions)
                    .ThenInclude(p_permission => p_permission.ScopedLocations)
                .FirstOrDefaultAsync(p_dynamicRole => p_dynamicRole.Id == preset.Id && p_dynamicRole.IsPreset)
                .ConfigureAwait(false);

            if (role is null)
            {
                continue;
            }

            IEnumerable<Crystal.Core.DTOs.Responses.PermissionRuleDto> expectedInventoryRules = preset.Permissions
                .Where(p_rule => p_rule.Subject == PermissionSubjects.InventoryQuantity);

            foreach (Crystal.Core.DTOs.Responses.PermissionRuleDto expectedRule in expectedInventoryRules)
            {
                RolePermission? existingPermission = role.Permissions.FirstOrDefault(
                    p_permission => p_permission.Action == expectedRule.Action
                        && p_permission.Subject == expectedRule.Subject);

                if (existingPermission is null)
                {
                    RolePermission newPermission = new()
                    {
                        Action = expectedRule.Action,
                        Subject = expectedRule.Subject,
                        LocationScope = expectedRule.LocationScope,
                    };

                    if (expectedRule.LocationScope == LocationScopes.Specific)
                    {
                        foreach (int locationId in expectedRule.LocationIds)
                        {
                            newPermission.ScopedLocations.Add(new RolePermissionLocation
                            {
                                LocationId = locationId,
                            });
                        }
                    }

                    role.Permissions.Add(newPermission);
                    hasChanges = true;
                    continue;
                }

                if (string.IsNullOrWhiteSpace(existingPermission.LocationScope)
                    && !string.IsNullOrWhiteSpace(expectedRule.LocationScope))
                {
                    existingPermission.LocationScope = expectedRule.LocationScope;
                    existingPermission.ScopedLocations.Clear();
                    hasChanges = true;
                }
            }
        }

        if (hasChanges)
        {
            await context.SaveChangesAsync().ConfigureAwait(false);
        }
    }

    public static async Task SeedForIntegrationTestsAsync(IServiceProvider p_serviceProvider)
    {
        await SeedDynamicRolesAsync(p_serviceProvider).ConfigureAwait(false);
        await SeedUsersAsync(p_serviceProvider).ConfigureAwait(false);
        await BackfillUserDynamicRolesAsync(p_serviceProvider).ConfigureAwait(false);
        await SeedLocationsAsync(p_serviceProvider).ConfigureAwait(false);
        await SeedItemsAndBooksAsync(p_serviceProvider).ConfigureAwait(false);
        await SeedAuthorsAsync(p_serviceProvider).ConfigureAwait(false);
    }

    public static async Task SeedDemoHrTransactionalDataAsync(IServiceProvider p_serviceProvider)
    {
        CrystalDbContext context = p_serviceProvider.GetRequiredService<CrystalDbContext>();

        if (await context.ScheduledShifts.AnyAsync().ConfigureAwait(false))
        {
            return;
        }

        List<EmployeeProfile> profiles = await context.EmployeeProfiles
            .Include(p_ep => p_ep.JobPosition)
            .Where(p_ep => !p_ep.IsDeleted)
            .ToListAsync()
            .ConfigureAwait(false);

        if (profiles.Count == 0)
        {
            return;
        }

        EmployeeProfile? emilieProfile = profiles.FirstOrDefault(p_ep => p_ep.Email == "employee@crystal.local");
        EmployeeProfile? alineProfile = profiles.FirstOrDefault(p_ep => p_ep.Email == "assistant@crystal.local");
        EmployeeProfile? gabrielProfile = profiles.FirstOrDefault(p_ep => p_ep.Email == "gerant@crystal.local");
        EmployeeProfile? alexProfile = profiles.FirstOrDefault(p_ep => p_ep.Email == "admin@crystal.local");

        DateOnly today = DateOnly.FromDateTime(DateTime.Today);
        DateOnly shiftStart = today.AddDays(-14);
        DateOnly shiftEnd = today.AddDays(28);

        List<ScheduledShift> shifts = [];
        shifts.AddRange(CreateShiftsForProfile(
            emilieProfile,
            shiftStart,
            shiftEnd,
            [DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday],
            new TimeOnly(13, 0),
            new TimeOnly(21, 0)));

        shifts.AddRange(CreateShiftsForProfile(
            alineProfile,
            shiftStart,
            shiftEnd,
            [DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday],
            new TimeOnly(9, 0),
            new TimeOnly(17, 0)));

        shifts.AddRange(CreateShiftsForProfile(
            gabrielProfile,
            shiftStart,
            shiftEnd,
            [DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday],
            new TimeOnly(9, 0),
            new TimeOnly(17, 0),
            p_saturdayEnd: new TimeOnly(18, 0)));

        shifts.AddRange(CreateShiftsForProfile(
            alexProfile,
            shiftStart,
            shiftEnd,
            [DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday],
            new TimeOnly(10, 0),
            new TimeOnly(18, 0)));

        await context.ScheduledShifts.AddRangeAsync(shifts).ConfigureAwait(false);
        await context.SaveChangesAsync().ConfigureAwait(false);

        LeaveRequest[] leaveRequests =
        [
            new LeaveRequest
            {
                EmployeeProfileId = emilieProfile?.Id ?? profiles[0].Id,
                LeaveType = LeaveType.Vacation,
                Status = LeaveRequestStatus.Pending,
                StartDate = today.AddDays(21),
                EndDate = today.AddDays(25),
                Reason = "Vacances familiales à Charlevoix",
            },
            new LeaveRequest
            {
                EmployeeProfileId = emilieProfile?.Id ?? profiles[0].Id,
                LeaveType = LeaveType.Sick,
                Status = LeaveRequestStatus.Approved,
                StartDate = today.AddDays(-10),
                EndDate = today.AddDays(-9),
                Reason = "Rendez-vous médical à l'hôpital St-Sacrement",
            },
            new LeaveRequest
            {
                EmployeeProfileId = alineProfile?.Id ?? profiles[0].Id,
                LeaveType = LeaveType.Vacation,
                Status = LeaveRequestStatus.Approved,
                StartDate = today.AddDays(-30),
                EndDate = today.AddDays(-27),
                Reason = "Semaine de relâche au Mont-Sainte-Anne",
            },
            new LeaveRequest
            {
                EmployeeProfileId = alineProfile?.Id ?? profiles[0].Id,
                LeaveType = LeaveType.Unpaid,
                Status = LeaveRequestStatus.Pending,
                StartDate = today.AddDays(14),
                EndDate = today.AddDays(15),
                Reason = "Déménagement à Limoilou",
            },
            new LeaveRequest
            {
                EmployeeProfileId = gabrielProfile?.Id ?? profiles[0].Id,
                LeaveType = LeaveType.Vacation,
                Status = LeaveRequestStatus.Rejected,
                StartDate = today.AddDays(7),
                EndDate = today.AddDays(10),
                Reason = "Congé refusé — période de pointe à la succursale Québec",
            },
            new LeaveRequest
            {
                EmployeeProfileId = gabrielProfile?.Id ?? profiles[0].Id,
                LeaveType = LeaveType.Other,
                Status = LeaveRequestStatus.Approved,
                StartDate = today.AddDays(-5),
                EndDate = today.AddDays(-5),
                Reason = "Formation en gestion d'inventaire à Montréal",
            },
            new LeaveRequest
            {
                EmployeeProfileId = alexProfile?.Id ?? profiles[0].Id,
                LeaveType = LeaveType.Vacation,
                Status = LeaveRequestStatus.Pending,
                StartDate = today.AddDays(35),
                EndDate = today.AddDays(42),
                Reason = "Vacances estivales sur la Côte-de-Beaupré",
            },
            new LeaveRequest
            {
                EmployeeProfileId = emilieProfile?.Id ?? profiles[0].Id,
                LeaveType = LeaveType.Vacation,
                Status = LeaveRequestStatus.Approved,
                StartDate = today.AddDays(-60),
                EndDate = today.AddDays(-56),
                Reason = "Congé approuvé — semaine de vacances de février",
            },
            new LeaveRequest
            {
                EmployeeProfileId = alineProfile?.Id ?? profiles[0].Id,
                LeaveType = LeaveType.Sick,
                Status = LeaveRequestStatus.Pending,
                StartDate = today.AddDays(3),
                EndDate = today.AddDays(4),
                Reason = "Grippe — en attente de confirmation médicale",
            },
            new LeaveRequest
            {
                EmployeeProfileId = gabrielProfile?.Id ?? profiles[0].Id,
                LeaveType = LeaveType.Vacation,
                Status = LeaveRequestStatus.Pending,
                StartDate = today.AddDays(18),
                EndDate = today.AddDays(22),
                Reason = "Voyage familial à Tadoussac",
            },
        ];

        await context.LeaveRequests.AddRangeAsync(leaveRequests).ConfigureAwait(false);
        await context.SaveChangesAsync().ConfigureAwait(false);

        List<TimeEntry> timeEntries = [];
        foreach (ScheduledShift shift in shifts.Where(
            p_shift => p_shift.Date < today && p_shift.EmployeeProfileId.HasValue))
        {
            timeEntries.Add(new TimeEntry
            {
                EmployeeProfileId = shift.EmployeeProfileId!.Value,
                ScheduledShiftId = shift.Id,
                Date = shift.Date,
                StartTime = shift.StartTime,
                EndTime = shift.EndTime,
            });
        }

        await context.TimeEntries.AddRangeAsync(timeEntries).ConfigureAwait(false);
        await context.SaveChangesAsync().ConfigureAwait(false);

        DateOnly periodOneStart = today.AddDays(-(int)today.DayOfWeek - 13);
        DateOnly periodOneEnd = periodOneStart.AddDays(13);
        DateOnly periodTwoStart = periodOneEnd.AddDays(1);
        DateOnly periodTwoEnd = periodTwoStart.AddDays(13);

        List<Timesheet> timesheets =
        [
            new Timesheet
            {
                EmployeeProfileId = emilieProfile?.Id ?? profiles[0].Id,
                PeriodStart = periodOneStart,
                PeriodEnd = periodOneEnd,
                Status = TimesheetStatus.Submitted,
            },
            new Timesheet
            {
                EmployeeProfileId = emilieProfile?.Id ?? profiles[0].Id,
                PeriodStart = periodTwoStart,
                PeriodEnd = periodTwoEnd,
                Status = TimesheetStatus.Draft,
            },
            new Timesheet
            {
                EmployeeProfileId = alineProfile?.Id ?? profiles[0].Id,
                PeriodStart = periodOneStart,
                PeriodEnd = periodOneEnd,
                Status = TimesheetStatus.Approved,
            },
            new Timesheet
            {
                EmployeeProfileId = alineProfile?.Id ?? profiles[0].Id,
                PeriodStart = periodTwoStart,
                PeriodEnd = periodTwoEnd,
                Status = TimesheetStatus.Submitted,
            },
            new Timesheet
            {
                EmployeeProfileId = gabrielProfile?.Id ?? profiles[0].Id,
                PeriodStart = periodOneStart,
                PeriodEnd = periodOneEnd,
                Status = TimesheetStatus.Approved,
            },
            new Timesheet
            {
                EmployeeProfileId = gabrielProfile?.Id ?? profiles[0].Id,
                PeriodStart = periodTwoStart,
                PeriodEnd = periodTwoEnd,
                Status = TimesheetStatus.Rejected,
            },
            new Timesheet
            {
                EmployeeProfileId = alexProfile?.Id ?? profiles[0].Id,
                PeriodStart = periodOneStart,
                PeriodEnd = periodOneEnd,
                Status = TimesheetStatus.Submitted,
            },
            new Timesheet
            {
                EmployeeProfileId = alexProfile?.Id ?? profiles[0].Id,
                PeriodStart = periodTwoStart,
                PeriodEnd = periodTwoEnd,
                Status = TimesheetStatus.Draft,
            },
        ];

        await context.Timesheets.AddRangeAsync(timesheets).ConfigureAwait(false);
        await context.SaveChangesAsync().ConfigureAwait(false);

        foreach (Timesheet timesheet in timesheets)
        {
            List<TimeEntry> matchingEntries = timeEntries
                .Where(p_entry =>
                    p_entry.EmployeeProfileId == timesheet.EmployeeProfileId
                    && p_entry.Date >= timesheet.PeriodStart
                    && p_entry.Date <= timesheet.PeriodEnd)
                .ToList();

            foreach (TimeEntry entry in matchingEntries)
            {
                entry.TimesheetId = timesheet.Id;
            }
        }

        await context.SaveChangesAsync().ConfigureAwait(false);
    }

    private static IEnumerable<ScheduledShift> CreateShiftsForProfile(
        EmployeeProfile? p_profile,
        DateOnly p_startDate,
        DateOnly p_endDate,
        DayOfWeek[] p_workDays,
        TimeOnly p_startTime,
        TimeOnly p_endTime,
        TimeOnly? p_saturdayEnd = null)
    {
        if (p_profile is null)
        {
            return [];
        }

        List<ScheduledShift> shifts = [];

        for (DateOnly date = p_startDate; date <= p_endDate; date = date.AddDays(1))
        {
            if (!p_workDays.Contains(date.DayOfWeek))
            {
                continue;
            }

            TimeOnly endTime = date.DayOfWeek == DayOfWeek.Saturday && p_saturdayEnd.HasValue
                ? p_saturdayEnd.Value
                : p_endTime;

            shifts.Add(new ScheduledShift
            {
                EmployeeProfileId = p_profile.Id,
                LocationId = p_profile.LocationId,
                JobPositionId = p_profile.PositionId,
                Date = date,
                StartTime = p_startTime,
                EndTime = endTime,
            });
        }

        return shifts;
    }

    public static async Task SeedCategoriesAsync(IServiceProvider p_serviceProvider)
    {
        CrystalDbContext context = p_serviceProvider.GetRequiredService<CrystalDbContext>();

        if (await context.Categories.AnyAsync().ConfigureAwait(false))
        {
            return;
        }

        Category[] categories =
        [
            new Category { Name = SeedDataConstants.QuebecLiteratureCategoryName },
            new Category { Name = SeedDataConstants.ScienceFictionCategoryName },
            new Category { Name = SeedDataConstants.SoftwareDevelopmentCategoryName },
            new Category { Name = SeedDataConstants.YoungAdultCategoryName },
            new Category { Name = SeedDataConstants.GraphicNovelsCategoryName },
            new Category { Name = SeedDataConstants.StationeryCategoryName },
            new Category { Name = SeedDataConstants.BoardGamesCategoryName },
            new Category { Name = SeedDataConstants.MagazinesCategoryName },
        ];

        await context.Categories.AddRangeAsync(categories).ConfigureAwait(false);
        await context.SaveChangesAsync().ConfigureAwait(false);
    }

    public static async Task SeedExtendedCatalogAsync(IServiceProvider p_serviceProvider)
    {
        CrystalDbContext context = p_serviceProvider.GetRequiredService<CrystalDbContext>();

        if (await context.Items.AnyAsync(p_item => p_item.Name == SeedDataConstants.HockeySweaterItemName).ConfigureAwait(false))
        {
            return;
        }

        Location? quebecLocation = await context.Locations
            .FirstOrDefaultAsync(p_l => p_l.Title == SeedDataConstants.QuebecCityBranchTitle)
            .ConfigureAwait(false);

        Location? sainteFoyLocation = await context.Locations
            .FirstOrDefaultAsync(p_l => p_l.Title == SeedDataConstants.SainteFoyBranchTitle)
            .ConfigureAwait(false);

        if (quebecLocation is null || sainteFoyLocation is null)
        {
            return;
        }

        Dictionary<string, Category> categoriesByName = await context.Categories
            .ToDictionaryAsync(p_c => p_c.Name)
            .ConfigureAwait(false);

        if (categoriesByName.Count == 0)
        {
            return;
        }

        await EnsureAuthorsAsync(context).ConfigureAwait(false);

        Dictionary<string, Author> authorsByName = await context.Authors
            .ToDictionaryAsync(p_a => p_a.Name)
            .ConfigureAwait(false);

        if (!await context.BookCategories.AnyAsync().ConfigureAwait(false))
        {
            await LinkExistingBooksToCategoriesAsync(context, categoriesByName, authorsByName).ConfigureAwait(false);
        }

        Item chandailItem = new()
        {
            Name = SeedDataConstants.HockeySweaterItemName,
            Description = "Le classique de Roch Carrier sur le hockey et l'identité québécoise.",
            Price = 16.95m,
            AlertQuantity = 4,
            IsActive = true,
            LastUpdate = DateTime.UtcNow,
        };

        Item stationElevenItem = new()
        {
            Name = "Station Eleven",
            Description = "Roman dystopique d'Emily St. John Mandel — lauréat du prix Locus.",
            Price = 21.99m,
            AlertQuantity = 3,
            IsActive = true,
            LastUpdate = DateTime.UtcNow,
        };

        Item designPatternsItem = new()
        {
            Name = "Design Patterns",
            Description = "L'ouvrage de référence du Gang of Four sur l'architecture logicielle.",
            Price = 59.99m,
            AlertQuantity = 2,
            IsActive = true,
            LastUpdate = DateTime.UtcNow,
        };

        Item cahierItem = new()
        {
            Name = "Cahier Moleskine édition Québec",
            Description = "Cahier 192 pages à couverture rigide, ligné.",
            Distributor = "Moleskine Canada",
            Price = 24.99m,
            AlertQuantity = 5,
            IsActive = true,
            LastUpdate = DateTime.UtcNow,
        };

        Item cafeItem = new()
        {
            Name = "Café torréfié Limoilou",
            Description = "Mélange maison de la Librairie Crystal — notes de noisette.",
            Distributor = "Crown Roasters",
            Price = 18.50m,
            AlertQuantity = 6,
            IsActive = true,
            LastUpdate = DateTime.UtcNow,
        };

        Item sacItem = new()
        {
            Name = "Sac réutilisable Librairie Crystal",
            Description = "Sac en coton avec logo — idéal pour les achats à Sainte-Foy.",
            Distributor = "Cap Printing",
            Price = 8.99m,
            AlertQuantity = 10,
            IsActive = true,
            LastUpdate = DateTime.UtcNow,
        };

        Item jeuItem = new()
        {
            Name = "Jeu de cartes Découvre le Québec",
            Description = "Jeu de cartes pour découvrir les régions du Québec en famille.",
            Distributor = "Scorpion Games",
            Price = 29.99m,
            AlertQuantity = 4,
            IsActive = true,
            LastUpdate = DateTime.UtcNow,
        };

        Item actualiteItem = new()
        {
            Name = "Magazine L'actualité",
            Description = "Numéro courant — actualité et culture québécoises.",
            Distributor = "Actualite Inc.",
            Price = 7.95m,
            AlertQuantity = 8,
            IsActive = true,
            LastUpdate = DateTime.UtcNow,
        };

        await context.Items.AddRangeAsync(
            chandailItem,
            stationElevenItem,
            designPatternsItem,
            cahierItem,
            cafeItem,
            sacItem,
            jeuItem,
            actualiteItem).ConfigureAwait(false);

        await context.SaveChangesAsync().ConfigureAwait(false);

        Book chandailBook = new()
        {
            ItemId = chandailItem.Id,
            Isbn = "9782890211186",
            PublicationDate = new DateOnly(1979, 1, 1),
        };

        Book stationElevenBook = new()
        {
            ItemId = stationElevenItem.Id,
            Isbn = "9782823612796",
            PublicationDate = new DateOnly(2014, 9, 1),
        };

        Book designPatternsBook = new()
        {
            ItemId = designPatternsItem.Id,
            Isbn = "9780201633610",
            PublicationDate = new DateOnly(1994, 10, 21),
        };

        await context.Books.AddRangeAsync(chandailBook, stationElevenBook, designPatternsBook).ConfigureAwait(false);

        if (authorsByName.TryGetValue("Roch Carrier", out Author? rochCarrier))
        {
            await context.AuthorBooks.AddAsync(new AuthorBook
            {
                AuthorId = rochCarrier.Id,
                BookId = chandailBook.ItemId,
            }).ConfigureAwait(false);
        }

        await context.BookCategories.AddRangeAsync(
            new BookCategory { BookId = chandailBook.ItemId, CategoryId = categoriesByName[SeedDataConstants.QuebecLiteratureCategoryName].Id },
            new BookCategory { BookId = stationElevenBook.ItemId, CategoryId = categoriesByName[SeedDataConstants.ScienceFictionCategoryName].Id },
            new BookCategory { BookId = designPatternsBook.ItemId, CategoryId = categoriesByName[SeedDataConstants.SoftwareDevelopmentCategoryName].Id }).ConfigureAwait(false);

        await context.InventoryLines.AddRangeAsync(
            new InventoryLine { ItemId = chandailItem.Id, LocationId = quebecLocation.Id, Quantity = 14 },
            new InventoryLine { ItemId = chandailItem.Id, LocationId = sainteFoyLocation.Id, Quantity = 9 },
            new InventoryLine { ItemId = stationElevenItem.Id, LocationId = quebecLocation.Id, Quantity = 6 },
            new InventoryLine { ItemId = stationElevenItem.Id, LocationId = sainteFoyLocation.Id, Quantity = 4 },
            new InventoryLine { ItemId = designPatternsItem.Id, LocationId = quebecLocation.Id, Quantity = 5 },
            new InventoryLine { ItemId = cahierItem.Id, LocationId = quebecLocation.Id, Quantity = 22 },
            new InventoryLine { ItemId = cahierItem.Id, LocationId = sainteFoyLocation.Id, Quantity = 15 },
            new InventoryLine { ItemId = cafeItem.Id, LocationId = sainteFoyLocation.Id, Quantity = 18 },
            new InventoryLine { ItemId = sacItem.Id, LocationId = quebecLocation.Id, Quantity = 40 },
            new InventoryLine { ItemId = sacItem.Id, LocationId = sainteFoyLocation.Id, Quantity = 35 },
            new InventoryLine { ItemId = jeuItem.Id, LocationId = quebecLocation.Id, Quantity = 7 },
            new InventoryLine { ItemId = actualiteItem.Id, LocationId = sainteFoyLocation.Id, Quantity = 20 }).ConfigureAwait(false);

        await context.SaveChangesAsync().ConfigureAwait(false);
    }

    private static async Task EnsureAuthorsAsync(CrystalDbContext p_context)
    {
        string[] authorNames =
        [
            "Roch Carrier",
            "Antonine Maillet",
            "Michel Tremblay",
            "Emily St. John Mandel",
        ];

        foreach (string authorName in authorNames)
        {
            bool exists = await p_context.Authors.AnyAsync(p_a => p_a.Name == authorName).ConfigureAwait(false);
            if (!exists)
            {
                await p_context.Authors.AddAsync(new Author { Name = authorName }).ConfigureAwait(false);
            }
        }

        await p_context.SaveChangesAsync().ConfigureAwait(false);
    }

    private static async Task LinkExistingBooksToCategoriesAsync(
        CrystalDbContext p_context,
        Dictionary<string, Category> p_categoriesByName,
        Dictionary<string, Author> p_authorsByName)
    {
        Item? cleanCodeItem = await p_context.Items
            .Include(p_i => p_i.Book)
            .FirstOrDefaultAsync(p_i => p_i.Name == SeedDataConstants.CleanCodeItemName)
            .ConfigureAwait(false);

        Item? pragmaticItem = await p_context.Items
            .Include(p_i => p_i.Book)
            .FirstOrDefaultAsync(p_i => p_i.Name == SeedDataConstants.PragmaticProgrammerItemName)
            .ConfigureAwait(false);

        if (cleanCodeItem?.Book is not null && p_categoriesByName.TryGetValue(SeedDataConstants.SoftwareDevelopmentCategoryName, out Category? devCategory))
        {
            await p_context.BookCategories.AddAsync(new BookCategory
            {
                BookId = cleanCodeItem.Book.ItemId,
                CategoryId = devCategory.Id,
            }).ConfigureAwait(false);

            if (p_authorsByName.TryGetValue("Robert C. Martin", out Author? martin))
            {
                bool alreadyLinked = await p_context.AuthorBooks
                    .AnyAsync(p_ab => p_ab.BookId == cleanCodeItem.Book.ItemId && p_ab.AuthorId == martin.Id)
                    .ConfigureAwait(false);

                if (!alreadyLinked)
                {
                    await p_context.AuthorBooks.AddAsync(new AuthorBook
                    {
                        BookId = cleanCodeItem.Book.ItemId,
                        AuthorId = martin.Id,
                    }).ConfigureAwait(false);
                }
            }
        }

        if (pragmaticItem?.Book is not null && p_categoriesByName.TryGetValue(SeedDataConstants.SoftwareDevelopmentCategoryName, out Category? devCategory2))
        {
            await p_context.BookCategories.AddAsync(new BookCategory
            {
                BookId = pragmaticItem.Book.ItemId,
                CategoryId = devCategory2.Id,
            }).ConfigureAwait(false);
        }

        await p_context.SaveChangesAsync().ConfigureAwait(false);
    }

    public static async Task CleanupDuplicateCatalogItemsAsync(IServiceProvider p_serviceProvider)
    {
        CrystalDbContext context = p_serviceProvider.GetRequiredService<CrystalDbContext>();

        List<Item> activeItems = await context.Items
            .Where(p_item => p_item.IsActive)
            .OrderBy(p_item => p_item.Id)
            .ToListAsync()
            .ConfigureAwait(false);

        IEnumerable<IGrouping<string, Item>> duplicateGroups = activeItems
            .GroupBy(p_item => p_item.Name.ToLowerInvariant())
            .Where(p_group => p_group.Count() > 1);

        bool hasChanges = false;

        foreach (IGrouping<string, Item> group in duplicateGroups)
        {
            List<Item> items = group.OrderBy(p_item => p_item.Id).ToList();
            Item canonicalItem = items[0];

            foreach (Item duplicateItem in items.Skip(1))
            {
                List<InventoryLine> duplicateLines = await context.InventoryLines
                    .Where(p_line => p_line.ItemId == duplicateItem.Id)
                    .ToListAsync()
                    .ConfigureAwait(false);

                foreach (InventoryLine duplicateLine in duplicateLines)
                {
                    InventoryLine? canonicalLine = await context.InventoryLines
                        .FirstOrDefaultAsync(p_line =>
                            p_line.ItemId == canonicalItem.Id &&
                            p_line.LocationId == duplicateLine.LocationId)
                        .ConfigureAwait(false);

                    if (canonicalLine is not null)
                    {
                        canonicalLine.Quantity += duplicateLine.Quantity;
                        context.InventoryLines.Remove(duplicateLine);
                    }
                    else
                    {
                        duplicateLine.ItemId = canonicalItem.Id;
                    }

                    hasChanges = true;
                }

                duplicateItem.IsActive = false;
                duplicateItem.LastUpdate = DateTime.UtcNow;
                hasChanges = true;
            }
        }

        if (hasChanges)
        {
            await context.SaveChangesAsync().ConfigureAwait(false);
        }
    }

    public static async Task SeedAllAsync(IServiceProvider p_serviceProvider)
    {
        await SeedForIntegrationTestsAsync(p_serviceProvider).ConfigureAwait(false);
        await SeedCategoriesAsync(p_serviceProvider).ConfigureAwait(false);
        await SeedExtendedCatalogAsync(p_serviceProvider).ConfigureAwait(false);
        await CleanupDuplicateCatalogItemsAsync(p_serviceProvider).ConfigureAwait(false);
        await SeedHrReferenceDataAsync(p_serviceProvider).ConfigureAwait(false);
        await SeedDemoHrTransactionalDataAsync(p_serviceProvider).ConfigureAwait(false);
    }
}
