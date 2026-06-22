using Crystal.Core.Entities;
using Crystal.Infrastructure.Data.Configurations;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace Crystal.Infrastructure.Context;

public class CrystalDbContext : IdentityDbContext<ApplicationUser>
{
    public CrystalDbContext(DbContextOptions<CrystalDbContext> p_options)
        : base(p_options)
    {
    }

    public DbSet<EmployeeProfile> EmployeeProfiles => Set<EmployeeProfile>();
    public DbSet<JobPosition> JobPositions => Set<JobPosition>();
    public DbSet<ScheduledShift> ScheduledShifts => Set<ScheduledShift>();
    public DbSet<TimeEntry> TimeEntries => Set<TimeEntry>();
    public DbSet<Timesheet> Timesheets => Set<Timesheet>();
    public DbSet<EmploymentContract> EmploymentContracts => Set<EmploymentContract>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
    public DbSet<PayPeriod> PayPeriods => Set<PayPeriod>();
    public DbSet<PayStub> PayStubs => Set<PayStub>();
    public DbSet<Availability> Availabilities => Set<Availability>();

    public DbSet<Item> Items => Set<Item>();
    public DbSet<InventoryLine> InventoryLines => Set<InventoryLine>();
    public DbSet<Location> Locations => Set<Location>();

    public DbSet<Book> Books => Set<Book>();
    public DbSet<Author> Authors => Set<Author>();
    public DbSet<AuthorBook> AuthorBooks => Set<AuthorBook>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<BookCategory> BookCategories => Set<BookCategory>();
    public DbSet<Publisher> Publishers => Set<Publisher>();
    public DbSet<BookPublisher> BookPublishers => Set<BookPublisher>();

    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Receipt> Receipts => Set<Receipt>();

    public DbSet<DynamicRole> DynamicRoles => Set<DynamicRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<RolePermissionLocation> RolePermissionLocations => Set<RolePermissionLocation>();

    protected override void OnModelCreating(ModelBuilder p_modelBuilder)
    {
        base.OnModelCreating(p_modelBuilder);

        p_modelBuilder.ApplyConfiguration(new EmployeeProfileConfiguration());
        p_modelBuilder.ApplyConfiguration(new JobPositionConfiguration());
        p_modelBuilder.ApplyConfiguration(new ScheduledShiftConfiguration());
        p_modelBuilder.ApplyConfiguration(new TimeEntryConfiguration());
        p_modelBuilder.ApplyConfiguration(new TimesheetConfiguration());
        p_modelBuilder.ApplyConfiguration(new EmploymentContractConfiguration());
        p_modelBuilder.ApplyConfiguration(new LeaveRequestConfiguration());
        p_modelBuilder.ApplyConfiguration(new PayPeriodConfiguration());
        p_modelBuilder.ApplyConfiguration(new PayStubConfiguration());

        p_modelBuilder.ApplyConfigurationsFromAssembly(typeof(CrystalDbContext).Assembly);

        p_modelBuilder.Entity<AuthorBook>()
            .HasKey(p_x => new { p_x.AuthorId, p_x.BookId });

        p_modelBuilder.Entity<BookCategory>()
            .HasKey(p_x => new { p_x.BookId, p_x.CategoryId });

        p_modelBuilder.Entity<BookPublisher>()
            .HasKey(p_x => new { p_x.BookId, p_x.PublisherId });

        p_modelBuilder.Entity<Book>()
            .HasOne(p_x => p_x.Item)
            .WithOne(p_x => p_x.Book)
            .HasForeignKey<Book>(p_x => p_x.ItemId);

        p_modelBuilder.Entity<InventoryLine>()
            .HasOne(p_x => p_x.Item)
            .WithMany(p_x => p_x.InventoryLines)
            .HasForeignKey(p_x => p_x.ItemId);

        p_modelBuilder.Entity<InventoryLine>()
            .HasOne(p_line => p_line.Location)
            .WithMany(p_location => p_location.InventoryLines)
            .HasForeignKey(p_line => p_line.LocationId)
            .OnDelete(DeleteBehavior.Restrict);

        p_modelBuilder.Entity<InventoryLine>()
            .HasIndex(p_x => new { p_x.ItemId, p_x.LocationId })
            .IsUnique();

        p_modelBuilder.Entity<Receipt>()
            .HasOne(p_x => p_x.Client)
            .WithMany(p_x => p_x.Receipts)
            .HasForeignKey(p_x => p_x.ClientId);

        p_modelBuilder.Entity<Receipt>()
            .HasOne(p_x => p_x.Item)
            .WithMany(p_x => p_x.Receipts)
            .HasForeignKey(p_x => p_x.ItemId);
    }
}
