using Crystal.Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Crystal.Infrastructure.Context;

public class CrystalDbContext : IdentityDbContext<ApplicationUser>
{
    public CrystalDbContext(DbContextOptions<CrystalDbContext> p_options)
        : base(p_options)
    {
    }

    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<EmployeeRole> EmployeeRoles => Set<EmployeeRole>();
    public DbSet<Position> Positions => Set<Position>();
    public DbSet<WorkShift> WorkShifts => Set<WorkShift>();
    public DbSet<Punch> Punches => Set<Punch>();
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

    protected override void OnModelCreating(ModelBuilder p_modelBuilder)
    {
        base.OnModelCreating(p_modelBuilder);

        p_modelBuilder.Entity<AuthorBook>()
            .HasKey(x => new { x.AuthorId, x.BookId });

        p_modelBuilder.Entity<BookCategory>()
            .HasKey(x => new { x.BookId, x.CategoryId });

        p_modelBuilder.Entity<BookPublisher>()
            .HasKey(x => new { x.BookId, x.PublisherId });

        p_modelBuilder.Entity<Book>()
            .HasOne(x => x.Item)
            .WithOne(x => x.Book)
            .HasForeignKey<Book>(x => x.ItemId);

        p_modelBuilder.Entity<InventoryLine>()
            .HasOne(x => x.Item)
            .WithMany(x => x.InventoryLines)
            .HasForeignKey(x => x.ItemId);

        p_modelBuilder.Entity<InventoryLine>()
            .HasOne(x => x.Location)
            .WithMany(x => x.InventoryLines)
            .HasForeignKey(x => x.LocationId);

        p_modelBuilder.Entity<Receipt>()
            .HasOne(x => x.Client)
            .WithMany(x => x.Receipts)
            .HasForeignKey(x => x.ClientId);

        p_modelBuilder.Entity<Receipt>()
            .HasOne(x => x.Item)
            .WithMany(x => x.Receipts)
            .HasForeignKey(x => x.ItemId);

        p_modelBuilder.Entity<Employee>()
            .HasIndex(x => x.Email)
            .IsUnique();
    }
}