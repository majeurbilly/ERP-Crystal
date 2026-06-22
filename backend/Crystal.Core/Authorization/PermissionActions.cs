namespace Crystal.Core.Authorization;

public static class PermissionActions
{
    public const string Create = "create";
    public const string Read = "read";
    public const string Update = "update";
    public const string Delete = "delete";
    public const string Submit = "submit";
    public const string Approve = "approve";
    public const string Manage = "manage";

    public static readonly IReadOnlyList<string> All =
        [Create, Read, Update, Delete, Submit, Approve, Manage];
}
