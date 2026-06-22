using Crystal.Core.DTOs.Requests;
using Crystal.Core.DTOs.Responses;
using Crystal.Core.Enums;
using Crystal.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Crystal.IntegrationTests.EmploymentContract;

public sealed class EmploymentContractIntegrationTests : IClassFixture<CrystalWebApplicationFactory>, IDisposable
{
    private readonly HttpClient m_client;
    private readonly CrystalWebApplicationFactory m_factory;

    private static readonly DateOnly s_existingStart = new(2024, 1, 1);
    private static readonly DateOnly s_existingEnd = new(2024, 12, 31);

    public EmploymentContractIntegrationTests(CrystalWebApplicationFactory p_factory)
    {
        m_factory = p_factory;
        m_client = p_factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_Returns200OK_WithAdminToken()
    {
        await AuthenticateAsAdminAsync();

        HttpResponseMessage response = await m_client.GetAsync("/api/contracts");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_Returns200OK_WithEmployeeToken()
    {
        await AuthenticateAsEmployeeAsync();

        HttpResponseMessage response = await m_client.GetAsync("/api/contracts");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Create_Returns201Created_WithValidContract()
    {
        await AuthenticateAsAdminAsync();

        HrSeedResult seedResult = await SeedHrReferenceDataAsync();
        CreateEmploymentContractRequest request = BuildCreateRequest(
            seedResult.EmployeeProfileId,
            s_existingStart,
            s_existingEnd);

        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/contracts", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        EmploymentContractResponseDto? body = await response.Content.ReadFromJsonAsync<EmploymentContractResponseDto>();
        Assert.NotNull(body);
        Assert.Equal("FullTime", body.ContractType);
        Assert.Equal("Fixed", body.WageType);
        Assert.Equal(seedResult.EmployeeFirstName, body.EmployeeFirstName);
        Assert.Equal(seedResult.EmployeeLastName, body.EmployeeLastName);
        Assert.Equal(55000m, body.BaseRate);
    }

    [Fact]
    public async Task Create_Returns409Conflict_WhenOverlappingStart()
    {
        await AuthenticateAsAdminAsync();

        HrSeedResult seedResult = await SeedHrReferenceDataAsync();
        await CreateContractAsync(seedResult.EmployeeProfileId, s_existingStart, s_existingEnd);

        CreateEmploymentContractRequest request = BuildCreateRequest(
            seedResult.EmployeeProfileId,
            new DateOnly(2024, 6, 1),
            new DateOnly(2025, 6, 1));

        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/contracts", request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Create_Returns409Conflict_WhenOverlappingEnd()
    {
        await AuthenticateAsAdminAsync();

        HrSeedResult seedResult = await SeedHrReferenceDataAsync();
        await CreateContractAsync(seedResult.EmployeeProfileId, s_existingStart, s_existingEnd);

        CreateEmploymentContractRequest request = BuildCreateRequest(
            seedResult.EmployeeProfileId,
            new DateOnly(2023, 6, 1),
            new DateOnly(2024, 6, 1));

        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/contracts", request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Create_Returns409Conflict_WhenCompletelyEnclosed()
    {
        await AuthenticateAsAdminAsync();

        HrSeedResult seedResult = await SeedHrReferenceDataAsync();
        await CreateContractAsync(seedResult.EmployeeProfileId, s_existingStart, s_existingEnd);

        CreateEmploymentContractRequest request = BuildCreateRequest(
            seedResult.EmployeeProfileId,
            new DateOnly(2024, 3, 1),
            new DateOnly(2024, 9, 1));

        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/contracts", request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Create_Returns409Conflict_WhenCompletelyEnclosesExisting()
    {
        await AuthenticateAsAdminAsync();

        HrSeedResult seedResult = await SeedHrReferenceDataAsync();
        await CreateContractAsync(seedResult.EmployeeProfileId, s_existingStart, s_existingEnd);

        CreateEmploymentContractRequest request = BuildCreateRequest(
            seedResult.EmployeeProfileId,
            new DateOnly(2023, 1, 1),
            new DateOnly(2025, 12, 31));

        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/contracts", request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Create_Returns409Conflict_WhenOpenEndedExistingOverlaps()
    {
        await AuthenticateAsAdminAsync();

        HrSeedResult seedResult = await SeedHrReferenceDataAsync();
        await CreateContractAsync(seedResult.EmployeeProfileId, new DateOnly(2026, 1, 1), null);

        CreateEmploymentContractRequest request = BuildCreateRequest(
            seedResult.EmployeeProfileId,
            new DateOnly(2026, 6, 1),
            new DateOnly(2026, 12, 31));

        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/contracts", request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Create_Returns201Created_WhenNonOverlappingBefore()
    {
        await AuthenticateAsAdminAsync();

        HrSeedResult seedResult = await SeedHrReferenceDataAsync();
        await CreateContractAsync(seedResult.EmployeeProfileId, s_existingStart, s_existingEnd);

        CreateEmploymentContractRequest request = BuildCreateRequest(
            seedResult.EmployeeProfileId,
            new DateOnly(2022, 1, 1),
            new DateOnly(2023, 12, 31));

        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/contracts", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Create_Returns201Created_WhenNonOverlappingAfter()
    {
        await AuthenticateAsAdminAsync();

        HrSeedResult seedResult = await SeedHrReferenceDataAsync();
        await CreateContractAsync(seedResult.EmployeeProfileId, s_existingStart, s_existingEnd);

        CreateEmploymentContractRequest request = BuildCreateRequest(
            seedResult.EmployeeProfileId,
            new DateOnly(2025, 1, 1),
            new DateOnly(2025, 12, 31));

        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/contracts", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Create_Returns400BadRequest_WhenEndDateIsBeforeStartDate()
    {
        await AuthenticateAsAdminAsync();

        HrSeedResult seedResult = await SeedHrReferenceDataAsync();

        CreateEmploymentContractRequest request = BuildCreateRequest(
            seedResult.EmployeeProfileId,
            new DateOnly(2024, 12, 31),
            new DateOnly(2024, 1, 1));

        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/contracts", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Update_Returns200OK_WhenAdjustingSameContractWithoutOverlap()
    {
        await AuthenticateAsAdminAsync();

        HrSeedResult seedResult = await SeedHrReferenceDataAsync();
        int contractId = await CreateContractAsync(seedResult.EmployeeProfileId, s_existingStart, s_existingEnd);

        UpdateEmploymentContractRequest updateRequest = new()
        {
            EmployeeProfileId = seedResult.EmployeeProfileId,
            ContractType = ContractType.PartTime,
            WageType = WageType.Monthly,
            BaseRate = 28.50m,
            StartDate = new DateOnly(2024, 2, 1),
            EndDate = new DateOnly(2024, 11, 30)
        };

        HttpResponseMessage response = await m_client.PutAsJsonAsync($"/api/contracts/{contractId}", updateRequest);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        EmploymentContractResponseDto? body = await response.Content.ReadFromJsonAsync<EmploymentContractResponseDto>();
        Assert.NotNull(body);
        Assert.Equal("PartTime", body.ContractType);
        Assert.Equal("Monthly", body.WageType);
        Assert.Equal(28.50m, body.BaseRate);
    }

    [Fact]
    public async Task Create_Returns403Forbidden_WithEmployeeToken()
    {
        HrSeedResult seedResult = await SeedHrReferenceDataAsync();
        await AuthenticateAsEmployeeAsync();

        CreateEmploymentContractRequest request = BuildCreateRequest(
            seedResult.EmployeeProfileId,
            s_existingStart,
            s_existingEnd);

        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/contracts", request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Delete_Returns204NoContent_WithAdminToken()
    {
        await AuthenticateAsAdminAsync();

        HrSeedResult seedResult = await SeedHrReferenceDataAsync();
        int contractId = await CreateContractAsync(seedResult.EmployeeProfileId, s_existingStart, s_existingEnd);

        HttpResponseMessage response = await m_client.DeleteAsync($"/api/contracts/{contractId}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

        Crystal.Core.Entities.EmploymentContract? deleted = await context.EmploymentContracts
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p_contract => p_contract.Id == contractId);

        Assert.NotNull(deleted);
        Assert.True(deleted.IsDeleted);
    }

    private async Task<int> CreateContractAsync(int p_employeeProfileId, DateOnly p_startDate, DateOnly? p_endDate)
    {
        CreateEmploymentContractRequest request = BuildCreateRequest(p_employeeProfileId, p_startDate, p_endDate);

        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/contracts", request);
        response.EnsureSuccessStatusCode();

        EmploymentContractResponseDto? created = await response.Content.ReadFromJsonAsync<EmploymentContractResponseDto>();
        Assert.NotNull(created);
        return created.Id;
    }

    private static CreateEmploymentContractRequest BuildCreateRequest(
        int p_employeeProfileId,
        DateOnly p_startDate,
        DateOnly? p_endDate)
    {
        return new CreateEmploymentContractRequest
        {
            EmployeeProfileId = p_employeeProfileId,
            ContractType = ContractType.FullTime,
            WageType = WageType.Fixed,
            BaseRate = 55000m,
            StartDate = p_startDate,
            EndDate = p_endDate
        };
    }

    private async Task<HrSeedResult> SeedHrReferenceDataAsync()
    {
        string uniqueSuffix = Guid.NewGuid().ToString();
        string jobPositionName = $"JobPosition-{uniqueSuffix}";
        string employeeEmail = $"employee-{uniqueSuffix}@test.local";
        string employeeFirstName = "Contract";
        string employeeLastName = $"Test-{uniqueSuffix}";

        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

        Crystal.Core.Entities.JobPosition jobPosition = new()
        {
            Name = jobPositionName,
            Description = "Poste pour tests de contrat",
            IsDeleted = false
        };

        await context.JobPositions.AddAsync(jobPosition);
        await context.SaveChangesAsync();

        Crystal.Core.Entities.EmployeeProfile employeeProfile = new()
        {
            FirstName = employeeFirstName,
            LastName = employeeLastName,
            Email = employeeEmail,
            Salary = 50000m,
            Status = "Active",
            PositionId = jobPosition.Id,
            HiringDate = new DateOnly(2024, 1, 1),
            IsDeleted = false
        };

        await context.EmployeeProfiles.AddAsync(employeeProfile);
        await context.SaveChangesAsync();

        return new HrSeedResult(
            employeeProfile.Id,
            employeeFirstName,
            employeeLastName);
    }

    private async Task AuthenticateAsAdminAsync()
    {
        await AuthenticateAsync("admin@crystal.local");
    }

    private async Task AuthenticateAsEmployeeAsync()
    {
        await AuthenticateAsync("employee@crystal.local");
    }

    private async Task AuthenticateAsync(string p_email)
    {
        LoginRequest request = new()
        {
            Email = p_email,
            Password = "ValidPass1!a"
        };

        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/auth/login", request);
        response.EnsureSuccessStatusCode();

        LoginResponse? login = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(login);
        Assert.False(string.IsNullOrWhiteSpace(login.Token));

        m_client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", login.Token);
    }

    public void Dispose()
    {
        m_client.Dispose();
    }

    private sealed class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    private sealed class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
    }

    private sealed record HrSeedResult(
        int EmployeeProfileId,
        string EmployeeFirstName,
        string EmployeeLastName);
}
