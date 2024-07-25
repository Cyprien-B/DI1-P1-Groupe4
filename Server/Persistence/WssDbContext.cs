using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

using Server.Models;

namespace Server.Persistence;

public class WssDbContext(DbContextOptions options, IConfiguration configuration) : DbContext(options)
{
    public DbSet<Company> Companies { get; set; } = null!;
    public DbSet<Employee> Employees { get; set; } = null!;
    public DbSet<Game> Games { get; set; } = null!;
    public DbSet<Player> Players { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var dbOptions = configuration.GetSection("Database");

        var dbHost = dbOptions.GetValue<string>("Host");
        var dbPort = dbOptions.GetValue<string>("Port");
        var dbName = dbOptions.GetValue<string>("Name");
        var dbUser = dbOptions.GetValue<string>("User");
        var dbPass = dbOptions.GetValue<string>("Pass");

        var dbConnectionString = $"Host={dbHost};Port={dbPort};Db={dbName};Username={dbUser};Password={dbPass}";

        optionsBuilder.UseNpgsql(dbConnectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Company>(e =>
        {
            e.ToTable("companies");
            e.HasKey(e => e.Id);
            e.Property(e => e.Name).HasColumnType("varchar(255)");
            e.HasOne(e => e.Player)
                .WithOne(e => e.Company)
                .HasForeignKey<Company>(e => e.PlayerId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
            e.HasMany(e => e.Employees)
                .WithOne(e => e.Company)
                .HasForeignKey(e => e.CompanyId);
        });

        modelBuilder.Entity<Employee>(e =>
        {
            e.ToTable("employees");
            e.HasKey(e => e.Id);
            e.Property(e => e.Name).HasColumnType("varchar(255)");
            e.HasOne(e => e.Company)
                .WithMany(e => e.Employees)
                .HasForeignKey(e => e.CompanyId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Game>(e =>
        {
            e.ToTable("games");
            e.HasKey(e => e.Id);
            e.Property(e => e.Name).HasColumnType("varchar(255)");
            e.Property(e => e.Rounds).HasColumnType("integer");
            e.Property(e => e.Status)
                .HasColumnType("varchar(255)")
                .HasDefaultValue(GameStatus.Waiting)
                .HasConversion(new EnumToStringConverter<GameStatus>());
            e.HasMany(e => e.Players)
                .WithOne(e => e.Game)
                .HasForeignKey(e => e.GameId);
        });

        modelBuilder.Entity<Player>(e =>
        {
            e.ToTable("players");
            e.HasKey(e => e.Id);
            e.Property(e => e.Name).HasColumnType("varchar(255)");
            e.HasOne(e => e.Game)
                .WithMany(e => e.Players)
                .HasForeignKey(e => e.GameId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(e => e.Company)
                .WithOne(e => e.Player)
                .HasForeignKey<Company>(e => e.PlayerId);
        });
    }
}
