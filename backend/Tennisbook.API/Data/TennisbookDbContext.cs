using Microsoft.EntityFrameworkCore;
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
    public DbSet<User> Users => Set<User>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<TrainingEnrollment> TrainingEnrollments => Set<TrainingEnrollment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Player>(e =>
        {
            e.HasIndex(p => p.Email).IsUnique();
            e.HasIndex(p => p.QrCode).IsUnique();
        });

        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
            e.HasOne(u => u.Player)
                .WithOne(p => p.User)
                .HasForeignKey<User>(u => u.PlayerId)
                .OnDelete(DeleteBehavior.SetNull);
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

        modelBuilder.Entity<TrainingEnrollment>(e =>
        {
            e.HasOne(te => te.Player)
                .WithMany(p => p.TrainingEnrollments)
                .HasForeignKey(te => te.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(te => te.TrainingSession)
                .WithMany(ts => ts.Enrollments)
                .HasForeignKey(te => te.TrainingSessionId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(te => new { te.PlayerId, te.TrainingSessionId }).IsUnique();
        });

        modelBuilder.Entity<Payment>(e =>
        {
            e.HasOne(p => p.Player)
                .WithMany(pl => pl.Payments)
                .HasForeignKey(p => p.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(p => p.TrainingAttendance)
                .WithMany()
                .HasForeignKey(p => p.TrainingAttendanceId)
                .OnDelete(DeleteBehavior.SetNull);

            e.Property(p => p.Amount).HasPrecision(10, 2);
        });
    }
}
