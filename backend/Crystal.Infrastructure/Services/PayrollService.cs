using Crystal.Core.Authorization;
using Crystal.Core.Constants;
using Crystal.Core.DTOs.Requests;
using Crystal.Core.DTOs.Responses;
using Crystal.Core.Entities;
using Crystal.Core.Enums;
using Crystal.Core.Interfaces.Repositories;
using Crystal.Core.Interfaces.Services;
using Crystal.Infrastructure.Services.Validation;

namespace Crystal.Infrastructure.Services;

public class PayrollService : IPayrollService
{
    private readonly IPayPeriodRepository m_payPeriodRepository;
    private readonly IPayStubRepository m_payStubRepository;
    private readonly ITimesheetRepository m_timesheetRepository;
    private readonly IEmploymentContractRepository m_employmentContractRepository;
    private readonly IEmployeeScopeService m_employeeScopeService;

    public PayrollService(
        IPayPeriodRepository p_payPeriodRepository,
        IPayStubRepository p_payStubRepository,
        ITimesheetRepository p_timesheetRepository,
        IEmploymentContractRepository p_employmentContractRepository,
        IEmployeeScopeService p_employeeScopeService)
    {
        m_payPeriodRepository = p_payPeriodRepository;
        m_payStubRepository = p_payStubRepository;
        m_timesheetRepository = p_timesheetRepository;
        m_employmentContractRepository = p_employmentContractRepository;
        m_employeeScopeService = p_employeeScopeService;
    }

    public async Task<IEnumerable<PayStubResponseDto>> GetAllPayStubsAsync(string p_userId)
    {
        IEnumerable<PayStub> payStubs;

        if (await m_employeeScopeService.CanManageAsync(p_userId, PermissionSubjects.Payroll))
        {
            payStubs = await m_payStubRepository.GetAllAsync();
        }
        else
        {
            int? profileId = await m_employeeScopeService.GetEmployeeProfileIdAsync(p_userId);
            if (!profileId.HasValue)
            {
                return [];
            }

            payStubs = await m_payStubRepository.GetPublishedByEmployeeProfileIdAsync(profileId.Value);
        }

        return payStubs.Select(MapToDto);
    }

    public async Task<IEnumerable<PayPeriodResponseDto>> GetAllPayPeriodsAsync()
    {
        IEnumerable<PayPeriod> payPeriods = await m_payPeriodRepository.GetAllAsync();
        return payPeriods.Select(MapPayPeriodToDto);
    }

    public async Task<PayPeriodResponseDto> CreatePayPeriodAsync(CreatePayPeriodRequest p_request)
    {
        if (p_request.EndDate < p_request.StartDate)
        {
            throw new ArgumentException(ErrorMessages.Payroll.EndDateBeforeStartDate);
        }

        ValidatePayPeriodIsCompletePast(p_request.EndDate);

        PayPeriod payPeriod = new PayPeriod
        {
            StartDate = p_request.StartDate,
            EndDate = p_request.EndDate,
            IsProcessed = false,
        };

        await m_payPeriodRepository.AddAsync(payPeriod);
        await m_payPeriodRepository.SaveChangesAsync();

        PayPeriod? createdPayPeriod = await m_payPeriodRepository.GetByIdAsync(payPeriod.Id);
        if (createdPayPeriod is null)
        {
            throw new InvalidOperationException(ErrorMessages.Payroll.PayPeriodCreateRetrievalFailed);
        }

        return MapPayPeriodToDto(createdPayPeriod);
    }

    public async Task<PayStubResponseDto> GeneratePayStubAsync(int p_payPeriodId, int p_employeeProfileId)
    {
        EntityIdentifierValidator.EnsureValid(p_payPeriodId);
        EntityIdentifierValidator.EnsureValidEmployeeProfileId(p_employeeProfileId);

        PayPeriod? payPeriod = await m_payPeriodRepository.GetByIdAsync(p_payPeriodId);
        if (payPeriod is null)
        {
            throw new KeyNotFoundException(ErrorMessages.Payroll.PayPeriodNotFound);
        }

        ValidatePayPeriodIsCompletePast(payPeriod.EndDate);

        Timesheet? approvedTimesheet = await m_timesheetRepository.GetApprovedByEmployeeAndPeriodAsync(
            p_employeeProfileId,
            payPeriod.StartDate,
            payPeriod.EndDate);

        if (approvedTimesheet is null)
        {
            throw new InvalidOperationException(ErrorMessages.Payroll.NoApprovedTimesheetForExactPeriod);
        }

        EmploymentContract? activeContract = await m_employmentContractRepository.GetActiveForEmployeeAndPeriodAsync(
            p_employeeProfileId,
            payPeriod.StartDate,
            payPeriod.EndDate);

        if (activeContract is null)
        {
            throw new InvalidOperationException(ErrorMessages.Payroll.NoActiveContractForPeriod);
        }

        PayStub payStub = CreatePayStub(p_payPeriodId, p_employeeProfileId, approvedTimesheet, activeContract);

        await m_payStubRepository.AddAsync(payStub);
        await m_payStubRepository.SaveChangesAsync();

        PayStub? createdPayStub = await m_payStubRepository.GetByIdAsync(payStub.Id);
        if (createdPayStub is null)
        {
            throw new InvalidOperationException(ErrorMessages.Payroll.PayStubGenerateRetrievalFailed);
        }

        return MapToDto(createdPayStub);
    }

    public async Task<GeneratePayrollForPeriodResponseDto> GenerateForPeriodAsync(
        int p_payPeriodId,
        string p_userId,
        int? p_locationId)
    {
        EntityIdentifierValidator.EnsureValid(p_payPeriodId);

        int? scopedLocationId = await ResolveGenerationLocationIdAsync(p_userId, p_locationId);

        PayPeriod? payPeriod = await m_payPeriodRepository.GetByIdAsync(p_payPeriodId);
        if (payPeriod is null)
        {
            throw new KeyNotFoundException(ErrorMessages.Payroll.PayPeriodNotFound);
        }

        ValidatePayPeriodIsCompletePast(payPeriod.EndDate);

        IList<Timesheet> approvedTimesheets = (await m_timesheetRepository.GetByPeriodAsync(
                payPeriod.StartDate,
                payPeriod.EndDate))
            .Where(p_timesheet => p_timesheet.Status == TimesheetStatus.Approved)
            .Where(p_timesheet =>
                !scopedLocationId.HasValue
                || p_timesheet.EmployeeProfile.LocationId == scopedLocationId.Value)
            .ToList();

        if (approvedTimesheets.Count == 0)
        {
            throw new InvalidOperationException(ErrorMessages.Payroll.NoApprovedTimesheetForPeriod);
        }

        IList<PayStub> existingPayStubs = await m_payStubRepository.GetByPayPeriodIdAsync(payPeriod.Id);
        HashSet<int> existingEmployeeProfileIds = existingPayStubs
            .Select(p_stub => p_stub.EmployeeProfileId)
            .ToHashSet();

        List<PayStub> newPayStubs = new();
        int skippedCount = 0;

        foreach (Timesheet timesheet in approvedTimesheets)
        {
            if (existingEmployeeProfileIds.Contains(timesheet.EmployeeProfileId))
            {
                continue;
            }

            EmploymentContract? activeContract = await m_employmentContractRepository.GetActiveForEmployeeAndPeriodAsync(
                timesheet.EmployeeProfileId,
                payPeriod.StartDate,
                payPeriod.EndDate);

            if (activeContract is null)
            {
                skippedCount++;
                continue;
            }

            newPayStubs.Add(CreatePayStub(payPeriod.Id, timesheet.EmployeeProfileId, timesheet, activeContract));
        }

        if (newPayStubs.Count > 0)
        {
            await m_payStubRepository.AddRangeAsync(newPayStubs);
            await m_payStubRepository.SaveChangesAsync();
        }

        IList<PayStub> payStubsForPeriod = await m_payStubRepository.GetByPayPeriodIdAsync(payPeriod.Id);
        HashSet<int> approvedEmployeeProfileIds = approvedTimesheets
            .Select(p_timesheet => p_timesheet.EmployeeProfileId)
            .ToHashSet();

        return new GeneratePayrollForPeriodResponseDto
        {
            PayPeriodId = payPeriod.Id,
            PeriodStartDate = payPeriod.StartDate,
            PeriodEndDate = payPeriod.EndDate,
            LocationId = scopedLocationId,
            CreatedCount = newPayStubs.Count,
            ExistingCount = approvedTimesheets.Count(p_timesheet =>
                existingEmployeeProfileIds.Contains(p_timesheet.EmployeeProfileId)),
            SkippedCount = skippedCount,
            PayStubs = payStubsForPeriod
                .Where(p_stub => approvedEmployeeProfileIds.Contains(p_stub.EmployeeProfileId))
                .Select(MapToDto)
                .ToList(),
        };
    }

    public async Task<PayStubResponseDto> PublishPayStubAsync(int p_payStubId)
    {
        EntityIdentifierValidator.EnsureValid(p_payStubId);

        PayStub? payStub = await m_payStubRepository.GetTrackedByIdAsync(p_payStubId);
        if (payStub is null)
        {
            throw new KeyNotFoundException(ErrorMessages.Payroll.PayStubNotFound);
        }

        Timesheet? paidTimesheet = payStub.Timesheet;
        if (paidTimesheet is null)
        {
            paidTimesheet = await m_timesheetRepository.GetApprovedByEmployeeAndPeriodAsync(
                payStub.EmployeeProfileId,
                payStub.PayPeriod.StartDate,
                payStub.PayPeriod.EndDate);
        }

        if (paidTimesheet is null)
        {
            throw new InvalidOperationException(ErrorMessages.Payroll.TimesheetForPayStubNotFound);
        }

        payStub.IsPublished = true;
        payStub.TimesheetId = paidTimesheet.Id;
        paidTimesheet.IsPaid = true;
        await m_timesheetRepository.UpdateAsync(paidTimesheet);
        await m_payStubRepository.SaveChangesAsync();

        PayStub? publishedPayStub = await m_payStubRepository.GetByIdAsync(p_payStubId);
        if (publishedPayStub is null)
        {
            throw new InvalidOperationException(ErrorMessages.Payroll.PayStubPublishRetrievalFailed);
        }

        return MapToDto(publishedPayStub);
    }

    private async Task<int?> ResolveGenerationLocationIdAsync(string p_userId, int? p_requestedLocationId)
    {
        if (await m_employeeScopeService.CanManageAsync(p_userId, PermissionSubjects.All))
        {
            return p_requestedLocationId;
        }

        int? userLocationId = await m_employeeScopeService.GetEmployeeLocationIdAsync(p_userId);
        if (!userLocationId.HasValue)
        {
            throw new UnauthorizedAccessException(ErrorMessages.Payroll.NoLocationLinkedToUser);
        }

        if (p_requestedLocationId.HasValue && p_requestedLocationId.Value != userLocationId.Value)
        {
            throw new UnauthorizedAccessException(ErrorMessages.Payroll.GeneratePayrollLimitedToOwnLocation);
        }

        return userLocationId.Value;
    }

    private static void ValidatePayPeriodIsCompletePast(DateOnly p_periodEnd)
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.Today);
        if (p_periodEnd >= today)
        {
            throw new ArgumentException(ErrorMessages.Payroll.PayPeriodMustBeCompletePast);
        }
    }

    private static decimal CalculateTotalHours(Timesheet p_timesheet)
    {
        decimal totalHours = 0m;

        foreach (TimeEntry entry in p_timesheet.TimeEntries)
        {
            if (!entry.EndTime.HasValue)
            {
                throw new InvalidOperationException(ErrorMessages.Payroll.OpenTimeEntryCannotBeIncludedInPayroll);
            }

            TimeSpan duration = entry.EndTime.Value.ToTimeSpan() - entry.StartTime.ToTimeSpan();
            totalHours += (decimal)duration.TotalHours;
        }

        return totalHours;
    }

    private static PayPeriodResponseDto MapPayPeriodToDto(PayPeriod p_payPeriod)
    {
        return new PayPeriodResponseDto
        {
            Id = p_payPeriod.Id,
            StartDate = p_payPeriod.StartDate,
            EndDate = p_payPeriod.EndDate,
            IsProcessed = p_payPeriod.IsProcessed,
        };
    }

    private static PayStubResponseDto MapToDto(PayStub p_payStub)
    {
        return new PayStubResponseDto
        {
            Id = p_payStub.Id,
            PayPeriodId = p_payStub.PayPeriodId,
            EmployeeProfileId = p_payStub.EmployeeProfileId,
            EmployeeFirstName = p_payStub.EmployeeProfile.FirstName,
            EmployeeLastName = p_payStub.EmployeeProfile.LastName,
            PeriodStartDate = p_payStub.PayPeriod.StartDate,
            PeriodEndDate = p_payStub.PayPeriod.EndDate,
            TotalHours = p_payStub.TotalHours,
            GrossPay = p_payStub.GrossPay,
            IsPublished = p_payStub.IsPublished
        };
    }

    private static PayStub CreatePayStub(
        int p_payPeriodId,
        int p_employeeProfileId,
        Timesheet p_timesheet,
        EmploymentContract p_contract)
    {
        decimal totalHours = CalculateTotalHours(p_timesheet);
        decimal grossPay = p_contract.WageType == WageType.Fixed
            ? p_contract.BaseRate
            : totalHours * p_contract.BaseRate;

        return new PayStub
        {
            PayPeriodId = p_payPeriodId,
            EmployeeProfileId = p_employeeProfileId,
            TimesheetId = p_timesheet.Id,
            TotalHours = totalHours,
            GrossPay = grossPay,
            IsPublished = false,
            IsDeleted = false
        };
    }
}
