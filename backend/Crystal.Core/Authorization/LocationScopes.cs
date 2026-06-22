namespace Crystal.Core.Authorization;

public static class LocationScopes
{
    public const string All = "all";
    public const string Specific = "specific";

    public static readonly IReadOnlyList<string> AllValues =
        [All, Specific];
}
