using Crystal.Core.DTOs.Requests;
using Crystal.Core.DTOs.Responses;

namespace Crystal.Core.Interfaces.Services;

public interface ITimesheetService
{
    Task<IEnumerable<TimesheetResponseDto>> GetAllAsync(string p_userId);
    Task<TimesheetResponseDto?> GetByIdAsync(int p_id, string p_userId);
    Task<TimesheetResponseDto> CreateAsync(CreateTimesheetRequest p_request);
    Task<GenerateWeeklyTimesheetsResponseDto> GenerateWeeklyAsync(GenerateWeeklyTimesheetsRequest p_request, string p_userId);
    Task<TimesheetResponseDto> UpdateAsync(int p_id, CreateTimesheetRequest p_request);
    Task<TimesheetResponseDto> UpdateStatusAsync(int p_id, UpdateTimesheetStatusRequest p_request, string p_userId);
    Task<TimesheetResponseDto> UpdatePaidAsync(int p_id, UpdateTimesheetPaidRequest p_request);
    Task<TimesheetResponseDto> ReloadTimeEntriesAsync(int p_id);
    Task<TimesheetResponseDto> AddTimeEntryAsync(int p_id, CreateTimeEntryRequest p_request);
    Task<TimesheetResponseDto> UpdateTimeEntryAsync(int p_id, int p_timeEntryId, UpdateTimeEntryRequest p_request);
    Task<TimesheetResponseDto> RemoveTimeEntryAsync(int p_id, int p_timeEntryId);
    Task DeleteAsync(int p_id);
}
