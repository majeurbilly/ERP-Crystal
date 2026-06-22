namespace Crystal.Core.Constants;

public static class BusinessClock
{
    private const string LinuxTimeZoneId = "America/Toronto";
    private const string WindowsTimeZoneId = "Eastern Standard Time";

    private static readonly TimeZoneInfo s_easternTimeZone = ResolveEasternTimeZone();

    public static DateTime NowInBusinessZone =>
        TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, s_easternTimeZone);

    public static DateOnly Today =>
        DateOnly.FromDateTime(NowInBusinessZone);

    public static TimeOnly CurrentTime =>
        TimeOnly.FromDateTime(NowInBusinessZone);

    private static TimeZoneInfo ResolveEasternTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(LinuxTimeZoneId);
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById(WindowsTimeZoneId);
        }
    }
}
