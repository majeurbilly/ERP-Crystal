namespace Crystal.Core;

public static class ApplicationRoles
{
    public const string Admin = "Admin";
    public const string Gerant = "Gerant";
    public const string Assistant = "Assistant";
    public const string Employee = "Employee";

    public static readonly IReadOnlyList<string> All = new[] { Admin, Gerant, Assistant, Employee };
}
