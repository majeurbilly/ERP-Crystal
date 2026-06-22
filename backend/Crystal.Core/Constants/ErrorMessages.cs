namespace Crystal.Core.Constants;

public static class ErrorMessages
{
    public const string InvalidIdentifier = "The identifier is invalid.";
    public const string AccessDenied = "Access to this resource is denied.";
    public const string InternalServerError = "An internal error occurred.";

    public static class Category
    {
        public const string NotFound = "Category not found.";
        public const string NameAlreadyExists = "A category with this name already exists.";
        public const string NameRequired = "Category name is required.";
        public const string NameTooLong = "Category name is too long.";
    }

    public static class JobPosition
    {
        public const string NotFound = "Job position not found.";
        public const string NameAlreadyExists = "A job position with this name already exists.";
        public const string NameRequired = "Job position name is required.";
        public const string NameTooLong = "Job position name is too long.";
        public const string DescriptionRequired = "Job position description is required.";
        public const string DescriptionTooLong = "Job position description is too long.";
        public const string InvalidColorFormat = "Color must be a hexadecimal code in #RRGGBB format.";
    }

    public static class Location
    {
        public const string NotFound = "Location not found.";
        public const string TitleAlreadyExists = "A location with this title already exists.";
        public const string TitleRequired = "Location title is required.";
        public const string AddressRequired = "Location address is required.";
        public const string TitleTooLong = "Location title is too long.";
        public const string AddressTooLong = "Location address is too long.";
        public const string DescriptionTooLong = "Location description is too long.";
        public const string HasInventoryCannotDelete = "Cannot delete this location because it still contains inventory items.";
    }

    public static class EmployeeProfile
    {
        public const string NotFound = "Employee profile not found.";
        public const string NotLinkedToAccount = "No employee profile is linked to this account.";
        public const string InvalidIdentifier = "Employee profile identifier is invalid.";
        public const string InvalidUserAccountIdentifier = "User account identifier is invalid.";
        public const string SpecifiedNotFound = "The specified employee profile was not found.";
        public const string NoProfileLinkedToUserAccount = "No employee profile is linked to this user account.";
        public const string CreateRetrievalFailed = "The employee profile could not be retrieved after creation.";
        public const string UpdateRetrievalFailed = "The employee profile could not be retrieved after update.";
        public const string EmailAlreadyExists = "An employee profile with this email already exists.";
        public const string NoJobPositionAvailable = "No job position is available to associate with the employee profile.";
        public const string JobPositionNotFound = "The specified job position was not found.";
        public const string LocationNotFound = "The specified location was not found.";
        public const string UserAccountNotFound = "The specified user account was not found.";
        public const string UserAlreadyLinked = "This user account is already linked to another employee profile.";
        public const string FirstNameRequired = "First name is required.";
        public const string FirstNameTooLong = "First name is too long.";
        public const string LastNameRequired = "Last name is required.";
        public const string LastNameTooLong = "Last name is too long.";
        public const string EmailRequired = "Email is required.";
        public const string EmailTooLong = "Email is too long.";
        public const string StatusRequired = "Status is required.";
        public const string StatusTooLong = "Status is too long.";
        public const string NegativeSalary = "Salary cannot be negative.";
    }

    public static class LeaveRequest
    {
        public const string NotFound = "Leave request not found.";
        public const string CreateRetrievalFailed = "The leave request could not be retrieved after creation.";
        public const string StatusUpdateRetrievalFailed = "The leave request could not be retrieved after status update.";
        public const string EndDateBeforeStartDate = "End date must be on or after the start date.";
        public const string OverlappingPeriod = "A leave request already exists for this period.";
        public const string OnlyPendingCanBeApprovedOrRejected = "Only pending leave requests can be approved or rejected.";
        public const string InvalidPendingStatusTransition = "The requested status is not valid for a pending leave request.";
    }

    public static class EmploymentContract
    {
        public const string NotFound = "Employment contract not found.";
        public const string CreateRetrievalFailed = "The employment contract could not be retrieved after creation.";
        public const string UpdateRetrievalFailed = "The employment contract could not be retrieved after update.";
        public const string EndDateBeforeStartDate = "End date must be on or after the start date.";
        public const string ActiveContractAlreadyExists = "An active contract already exists for this period.";
    }

    public static class ScheduledShift
    {
        public const string NotFound = "Scheduled shift not found.";
        public const string CreateRetrievalFailed = "The scheduled shift could not be retrieved after creation.";
        public const string UpdateRetrievalFailed = "The scheduled shift could not be retrieved after update.";
        public const string JobPositionNotFound = "The specified job position was not found.";
        public const string LocationNotFound = "The specified location was not found.";
        public const string EndTimeBeforeStartTime = "End time must be after start time.";
        public const string EmployeeNotInShiftLocation = "The selected employee does not belong to the shift location.";
    }

    public static class Timesheet
    {
        public const string NotFound = "Timesheet not found.";
        public const string CreateRetrievalFailed = "The timesheet could not be retrieved after creation.";
        public const string UpdateRetrievalFailed = "The timesheet could not be retrieved after update.";
        public const string StatusUpdateRetrievalFailed = "The timesheet could not be retrieved after status update.";
        public const string PaidUpdateRetrievalFailed = "The timesheet could not be retrieved after paid status update.";
        public const string ReloadRetrievalFailed = "The timesheet could not be retrieved after reloading time entries.";
        public const string AddTimeEntryRetrievalFailed = "The timesheet could not be retrieved after adding the time entry.";
        public const string UpdateTimeEntryRetrievalFailed = "The timesheet could not be retrieved after updating the time entry.";
        public const string RemoveTimeEntryRetrievalFailed = "The timesheet could not be retrieved after removing the time entry.";
        public const string OnlyDraftCanBeModified = "Only draft timesheets can be modified.";
        public const string EmployeeCannotBeChanged = "The employee associated with the timesheet cannot be changed.";
        public const string OnlyDraftCanBeReloaded = "Only draft timesheets can be reloaded.";
        public const string PeriodEndBeforeStart = "Period end date must be on or after the start date.";
        public const string PeriodStartRequired = "Period start date is required.";
        public const string GeneratedWeekMustStartOnMonday = "The generated week must start on a Monday.";
        public const string GeneratedWeekMustBeComplete = "The generated week must be fully completed.";
        public const string NoLocationLinkedToUser = "No location is linked to this user.";
        public const string GenerateOnlyOwnLocation = "You can generate timesheets only for your own location.";
        public const string TimeEntryNotFoundOnTimesheet = "Time entry not found on this timesheet.";
        public const string TimeEntriesNotFound = "One or more specified time entries were not found.";
        public const string TimeEntriesMustBelongToEmployee = "All time entries must belong to the timesheet employee.";
        public const string TimeEntriesAlreadyLinked = "One or more time entries are already linked to another timesheet.";
        public const string TimeEntriesOutsidePeriod = "All time entries must fall within the timesheet period.";
        public const string TimeEntryMustBelongToTimesheetEmployee = "The time entry must belong to the timesheet employee.";
        public const string TimeEntryMustBeWithinTimesheetPeriod = "The time entry must fall within the timesheet period.";
        public const string EndTimeBeforeStartTime = "End time must be after start time.";
        public const string ScheduledShiftNotFound = "The specified scheduled shift was not found.";
        public const string ScheduledShiftEmployeeMismatch = "The scheduled shift does not match the timesheet employee.";
        public const string CannotApproveDraftTimesheet = "Cannot approve a draft timesheet. Submit it first.";
        public const string CannotRejectDraftTimesheet = "Cannot reject a draft timesheet. Submit it first.";
        public const string ApprovedTimesheetCannotChangeStatus = "An approved timesheet cannot change status.";
        public const string InvalidStatusTransition = "Status transition from {0} to {1} is not allowed.";
    }

    public static class TimeEntry
    {
        public const string NotFound = "Time entry not found.";
        public const string CreateRetrievalFailed = "The time entry could not be retrieved after creation.";
        public const string UpdateRetrievalFailed = "The time entry could not be retrieved after update.";
        public const string PunchOutRetrievalFailed = "The time entry could not be retrieved after punch out.";
        public const string ScheduledShiftNotFound = "The specified scheduled shift was not found.";
        public const string ScheduledShiftEmployeeMismatch = "The scheduled shift does not match the time entry employee.";
        public const string PunchAlreadyInProgress = "A punch is already in progress.";
        public const string NoOpenPunchToClose = "No open punch to close.";
        public const string EndTimeBeforeStartTime = "End time must be after start time.";
    }

    public static class PunchEligibility
    {
        public const string AccountNotLinkedToProfile = "Your account is not linked to an employee profile. Contact an administrator.";
        public const string NoShiftScheduledToday = "No shift is scheduled for you today. Contact your manager if you need to work.";
        public const string PunchInNotAllowedNow = "Punch-in is not allowed at this time.";
        public const string PunchInOpensAt = "Punch-in opens at {0}. Your shift starts at {1}.";
        public const string PunchInClosedAt = "Punch-in closed at {0}. Your shift ended at {1}.";
    }

    public static class Payroll
    {
        public const string PayPeriodNotFound = "Pay period not found.";
        public const string PayPeriodCreateRetrievalFailed = "The pay period could not be retrieved after creation.";
        public const string EndDateBeforeStartDate = "End date must be on or after the start date.";
        public const string PayPeriodMustBeCompletePast = "The pay period must be fully completed.";
        public const string NoActiveContractForPeriod = "No active employment contract covers this pay period.";
        public const string PayStubNotFound = "Pay stub not found.";
        public const string PayStubGenerateRetrievalFailed = "The pay stub could not be retrieved after generation.";
        public const string PayStubPublishRetrievalFailed = "The pay stub could not be retrieved after publication.";
        public const string NoApprovedTimesheetForExactPeriod = "No approved timesheet matches this pay period exactly.";
        public const string NoApprovedTimesheetForPeriod = "No approved timesheet matches this pay period.";
        public const string TimesheetForPayStubNotFound = "The timesheet used to generate this pay stub was not found.";
        public const string OpenTimeEntryCannotBeIncludedInPayroll = "A time entry without an end time cannot be included in payroll calculation.";
        public const string NoLocationLinkedToUser = "No location is linked to this user.";
        public const string GeneratePayrollLimitedToOwnLocation = "Payroll generation is limited to your own location.";
    }

    public static class DynamicRole
    {
        public const string NotFound = "Role not found.";
        public const string CreateRetrievalFailed = "The role could not be retrieved after creation.";
        public const string UpdateRetrievalFailed = "The role could not be retrieved after update.";
        public const string PresetRolesCannotBeModified = "Preset roles cannot be modified.";
        public const string PresetRolesCannotBeDeleted = "Preset roles cannot be deleted.";
        public const string RoleAssignedToUsers = "This role is assigned to one or more users.";
        public const string InvalidPreset = "Invalid role preset.";
        public const string NameRequired = "Role name is required.";
        public const string NameTooLong = "Role name is too long.";
        public const string AtLeastOnePermissionRequired = "At least one permission is required.";
        public const string LocationScopeInventoryOnly = "Location scope is reserved for inventory permissions.";
        public const string SpecificLocationsInventoryOnly = "Specific locations are reserved for inventory permissions.";
        public const string LocationScopeRequiredForInventory = "Location scope is required for inventory permissions.";
        public const string InvalidLocationScope = "Invalid location scope: {0}.";
        public const string AtLeastOneLocationRequired = "At least one location is required for a specific scope.";
        public const string SpecificLocationsNotAllowedWhenScopeIsAll = "Specific locations must not be provided when the scope is all.";
        public const string LocationNotFoundWithId = "Location not found: {0}.";
        public const string InvalidPermissionAction = "Invalid permission action: {0}.";
        public const string InvalidPermissionSubject = "Invalid permission subject: {0}.";
    }

    public static class Permission
    {
        public const string UserNotFound = "User not found.";
        public const string DynamicRoleNotFound = "Dynamic role not found.";
        public const string UserHasNoDynamicRole = "The user has no dynamic role assigned.";
    }

    public static class User
    {
        public const string RoleRequired = "A role is required for each user.";
        public const string RoleNotFound = "The specified role was not found.";
        public const string UnableToCreateUser = "Unable to create user.";
        public const string UnableToUpdateUser = "Unable to update user.";
        public const string UnableToUpdateProfile = "Unable to update profile.";
        public const string UnableToUpdatePassword = "Unable to update password.";
        public const string UnableToDeleteUser = "Unable to delete user.";
    }

    public static class Auth
    {
        public const string JwtKeyMissing = "Jwt:Key configuration is missing.";
    }

    public static class Item
    {
        public const string NotFound = "Item not found.";
        public const string NegativePrice = "Price cannot be negative.";
        public const string NegativeAlertQuantity = "Alert quantity cannot be negative.";
        public const string NameAlreadyExists = "An item with this name already exists.";
        public const string CreateLoadFailed = "Unable to load the created item.";
        public const string InvalidImageFormat = "Only JPG, JPEG, and PNG files are accepted.";
        public const string NameRequired = "The item must have a name.";
        public const string BookNameRequired = "The book must have a name.";
        public const string IsbnRequired = "The book must have an ISBN.";
        public const string CategoriesNotFound = "One or more categories were not found: {0}.";
        public const string ImageFileRequired = "Image file is required.";
        public const string FileNameRequired = "File name is required.";
    }

    public static class Inventory
    {
        public const string NegativeQuantity = "Quantity cannot be negative.";
        public const string ExcelOnly = "Only Excel (.xlsx) files are accepted.";
        public const string EmptyExcelFile = "The Excel file contains no data rows.";
        public const string ItemNotFound = "Item not found.";
        public const string ItemNotFoundInActiveCatalog = "Item was not found in the active catalog.";
        public const string ExcelFileRequired = "Excel file is required.";
        public const string InvalidExcelFormat = "Invalid Excel file format. Expected columns: LocationId, ItemId, Quantity.";
        public const string ExcelRowInvalidIds = "Row {0}: LocationId and ItemId must be positive integers.";
        public const string ExcelRowNegativeQuantity = "Row {0}: quantity cannot be negative.";
        public const string ExcelRowItemNotFound = "Row {0}: item (ItemId {1}) was not found.";
        public const string ExcelRowLocationNotFound = "Row {0}: location (LocationId {1}) was not found.";
    }

    public static class Book
    {
        public const string AuthorsNotFound = "One or more authors were not found: {0}.";
        public const string CategoriesNotFound = "One or more categories were not found: {0}.";
        public const string PublishersNotFound = "One or more publishers were not found: {0}.";
    }

    public static class Author
    {
        public const string NotFound = "Author not found.";
        public const string NameRequired = "Author name is required.";
        public const string NameTooLong = "Author name is too long.";
    }
}
