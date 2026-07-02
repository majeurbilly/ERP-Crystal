using Crystal.API.Middleware;
using Crystal.Core.Entities;
using Crystal.Core.Interfaces.Repositories;
using Crystal.Core.Interfaces.Services;
using Crystal.Infrastructure.Context;
using Crystal.Infrastructure.Data;
using Crystal.Infrastructure.Repositories;
using Crystal.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(p_options =>
{
    p_options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    p_options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(p_options =>
{
    p_options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Crystal.API",
        Version = "v1"
    });

    p_options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {your JWT token}"
    });

    p_options.AddSecurityRequirement(p_document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", p_document)] = []
    });
});

builder.Services.AddCors(p_options =>
{
    p_options.AddPolicy("FrontendPolicy", p_policy =>
    {
        p_policy.AllowAnyHeader()
              .AllowAnyMethod();

        if (builder.Environment.IsProduction())
        {
            string[] allowedOrigins = builder.Configuration
                .GetSection("Cors:AllowedOrigins")
                .Get<string[]>() ?? ["https://librairiecrystal.com"];

            p_policy.WithOrigins(allowedOrigins);
        }
        else
        {
            p_policy.AllowAnyOrigin();
        }
    });
});

Console.WriteLine($"Current environment: {builder.Environment.EnvironmentName}");

if (builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddSingleton(p_ =>
    {
        SqliteConnection connection = new("DataSource=:memory:;Cache=Shared");
        connection.Open();
        return connection;
    });
    builder.Services.AddDbContext<CrystalDbContext>((p_serviceProvider, p_options) =>
        p_options.UseSqlite(p_serviceProvider.GetRequiredService<SqliteConnection>()));
}
else
{
    string defaultConnection = DatabaseConnectionResolver.Resolve(builder.Configuration);
    builder.Services.AddDbContext<CrystalDbContext>(p_options =>
        p_options.UseNpgsql(defaultConnection));
}

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(p_options =>
    {
        p_options.User.RequireUniqueEmail = true;
        p_options.Password.RequiredLength = 8;
        p_options.Password.RequireDigit = true;
        p_options.Password.RequireLowercase = true;
        p_options.Password.RequireUppercase = true;
        p_options.Password.RequireNonAlphanumeric = true;
        p_options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        p_options.Lockout.MaxFailedAccessAttempts = 5;
    })
    .AddEntityFrameworkStores<CrystalDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<IItemRepository, ItemRepository>();
builder.Services.AddScoped<IItemService, ItemService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ILocationRepository, LocationRepository>();
builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IJobPositionRepository, JobPositionRepository>();
builder.Services.AddScoped<IJobPositionService, JobPositionService>();
builder.Services.AddScoped<IEmployeeProfileRepository, EmployeeProfileRepository>();
builder.Services.AddScoped<IEmployeeProfileService, EmployeeProfileService>();
builder.Services.AddScoped<IScheduledShiftRepository, ScheduledShiftRepository>();
builder.Services.AddScoped<IScheduledShiftService, ScheduledShiftService>();
builder.Services.AddScoped<ITimeEntryRepository, TimeEntryRepository>();
builder.Services.AddScoped<IPunchEligibilityService, PunchEligibilityService>();
builder.Services.AddScoped<ITimeEntryService, TimeEntryService>();
builder.Services.AddScoped<ITimesheetRepository, TimesheetRepository>();
builder.Services.AddScoped<ITimesheetService, TimesheetService>();
builder.Services.AddScoped<IEmploymentContractRepository, EmploymentContractRepository>();
builder.Services.AddScoped<IEmploymentContractService, EmploymentContractService>();
builder.Services.AddScoped<ILeaveRequestRepository, LeaveRequestRepository>();
builder.Services.AddScoped<ILeaveRequestService, LeaveRequestService>();
builder.Services.AddScoped<IPayPeriodRepository, PayPeriodRepository>();
builder.Services.AddScoped<IPayStubRepository, PayStubRepository>();
builder.Services.AddScoped<IPayrollService, PayrollService>();
builder.Services.AddScoped<IHrMetricsService, HrMetricsService>();
builder.Services.AddScoped<IAuthorRepository, AuthorRepository>();
builder.Services.AddScoped<IAuthorService, AuthorService>();
builder.Services.AddScoped<IDynamicRoleRepository, DynamicRoleRepository>();
builder.Services.AddScoped<IDynamicRoleService, DynamicRoleService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IEmployeeScopeService, EmployeeScopeService>();

IConfigurationSection jwtSettings = builder.Configuration.GetRequiredSection("Jwt");
byte[] key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);
bool requireHttpsMetadata = builder.Environment.IsProduction();

builder.Services.AddAuthentication(p_options =>
{
    p_options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    p_options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(p_options =>
{
    p_options.RequireHttpsMetadata = requireHttpsMetadata;
    p_options.SaveToken = true;

    p_options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),
        NameClaimType = System.Security.Claims.ClaimTypes.NameIdentifier,
    };
});

WebApplication app = builder.Build();

using (IServiceScope scope = app.Services.CreateScope())
{
    CrystalDbContext dbContext = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

    if (app.Environment.IsEnvironment("Testing"))
    {
        await dbContext.Database.EnsureCreatedAsync().ConfigureAwait(false);
        await DataSeeder.SeedForIntegrationTestsAsync(scope.ServiceProvider).ConfigureAwait(false);
    }
    else
    {
        await dbContext.Database.MigrateAsync().ConfigureAwait(false);

        if (app.Environment.IsDevelopment())
        {
            await DataSeeder.SeedAllAsync(scope.ServiceProvider);
        }
    }
}

app.UseMiddleware<ErrorHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("FrontendPolicy");

app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => "Crystal API is online")
    .AllowAnonymous();

app.MapControllers();

app.Run();

public partial class Program
{
}
