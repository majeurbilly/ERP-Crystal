namespace Crystal.Core.DTOs.Responses;

/// <summary>
/// État d'éligibilité au pointage pour l'employé connecté.
/// </summary>
public class PunchEligibilityDto
{
    public bool CanPunchIn { get; set; }

    public bool CanPunchOut { get; set; }

    public string? BlockedReason { get; set; }

    public string? BlockCode { get; set; }

    public int? ActiveEntryId { get; set; }

    public TimeOnly? ActiveEntryStartTime { get; set; }

    public int? ScheduledShiftId { get; set; }

    public DateOnly? ShiftDate { get; set; }

    public TimeOnly? ShiftStartTime { get; set; }

    public TimeOnly? ShiftEndTime { get; set; }

    public TimeOnly? EarliestPunchInTime { get; set; }
}
