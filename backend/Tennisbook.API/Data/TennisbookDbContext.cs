using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Tennisbook.API.Models;

namespace Tennisbook.API.Data;

public class TennisbookDbContext : DbContext
{
    public TennisbookDbContext(DbContextOptions<TennisbookDbContext> options) : base(options) { }

    public DbSet<Player> Players => Set<Player>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<TrainingSession> TrainingSessions => Set<TrainingSession>();
    public DbSet<TrainingAttendance> TrainingAttendances => Set<TrainingAttendance>();
    public DbSet<Tournament> Tournaments => Set<Tournament>();
    public DbSet<TournamentParticipation> TournamentParticipations => Set<TournamentParticipation>();

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ConvertDateTimesToUtc();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        ConvertDateTimesToUtc();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void ConvertDateTimesToUtc()
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State is EntityState.Added or EntityState.Modified)
            {
                foreach (var prop in entry.Properties)
                {
                    if (prop.CurrentValue is DateTime dt && dt.Kind != DateTimeKind.Utc)
                    {
                        prop.CurrentValue = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
                    }
                }
            }
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Player>(e =>
        {
            e.HasIndex(p => p.Email).IsUnique();
            e.HasIndex(p => p.QrCode).IsUnique();
        });

        modelBuilder.Entity<Subscription>(e =>
        {
            e.HasOne(s => s.Player)
                .WithMany(p => p.Subscriptions)
                .HasForeignKey(s => s.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);

            e.Property(s => s.MonthlyPrice).HasPrecision(10, 2);
            e.Property(s => s.AmountPaid).HasPrecision(10, 2);
            e.Ignore(s => s.BalanceDue);
            e.Ignore(s => s.TrainingsRemaining);
        });

        modelBuilder.Entity<TrainingAttendance>(e =>
        {
            e.HasOne(ta => ta.Player)
                .WithMany(p => p.TrainingAttendances)
                .HasForeignKey(ta => ta.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(ta => ta.TrainingSession)
                .WithMany(ts => ts.Attendances)
                .HasForeignKey(ta => ta.TrainingSessionId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(ta => new { ta.PlayerId, ta.TrainingSessionId }).IsUnique();
        });

        modelBuilder.Entity<TournamentParticipation>(e =>
        {
            e.HasOne(tp => tp.Player)
                .WithMany(p => p.TournamentParticipations)
                .HasForeignKey(tp => tp.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(tp => tp.Tournament)
                .WithMany(t => t.Participations)
                .HasForeignKey(tp => tp.TournamentId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(tp => new { tp.PlayerId, tp.TournamentId }).IsUnique();
        });
    }
}
