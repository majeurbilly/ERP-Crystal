namespace Crystal.Core.Authorization;

public static class PermissionSubjects
{
    public const string Me = "me";
    public const string All = "all";
    public const string Location = "location";
    public const string Item = "item";
    public const string User = "user";
    public const string Category = "category";
    public const string InventoryQuantity = "inventory_quantity";
    public const string HrDashboard = "hr_dashboard";
    public const string JobPosition = "job_position";
    public const string EmployeeProfile = "employee_profile";
    public const string EmploymentContract = "employment_contract";
    public const string LeaveRequest = "leave_request";
    public const string ScheduledShift = "scheduled_shift";
    public const string TimeEntry = "time_entry";
    public const string Timesheet = "timesheet";
    public const string Payroll = "payroll";
    public const string Author = "author";
    public const string UserRole = "user_role";

    public static readonly IReadOnlyList<string> AllEntities =
    [
        Location, Item, User, Category, InventoryQuantity, HrDashboard,
        JobPosition, EmployeeProfile, EmploymentContract,
        LeaveRequest, ScheduledShift, TimeEntry, Timesheet, Payroll,
        Author, UserRole
    ];
}
