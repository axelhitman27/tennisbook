using Microsoft.EntityFrameworkCore;
using Tennisbook.API.Data;
using Tennisbook.API.DTOs;

namespace Tennisbook.API.Services;

public class ReportService
{
    private readonly TennisbookDbContext _db;

    public ReportService(TennisbookDbContext db) => _db = db;

    public async Task<PlayerReportDto?> GetPlayerReportAsync(int playerId)
    {
        var player = await _db.Players
            .Include(p => p.Subscriptions)
            .Include(p => p.TrainingAttendances)
                .ThenInclude(a => a.TrainingSession)
            .Include(p => p.TournamentParticipations)
                .ThenInclude(tp => tp.Tournament)
            .FirstOrDefaultAsync(p => p.Id == playerId);

        if (player == null) return null;

        var now = DateTime.UtcNow;
        var thisMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var activeSub = player.Subscriptions.FirstOrDefault(s => s.IsActive);

        return new PlayerReportDto(
            player.Id,
            $"{player.FirstName} {player.LastName}",
            player.Level,
            player.TrainingAttendances.Count,
            player.TrainingAttendances.Count(a => a.CheckedInAt >= thisMonth),
            player.TournamentParticipations.Count,
            player.TrainingAttendances
                .OrderByDescending(a => a.CheckedInAt)
                .Take(20)
                .Select(a => new TrainingHistoryDto(
                    a.TrainingSessionId,
                    a.TrainingSession.Title,
                    a.TrainingSession.ScheduledAt,
                    a.TrainingSession.CourtName,
                    a.TrainingSession.TrainerName,
                    a.CheckedInAt
                ))
                .ToList(),
            player.TournamentParticipations
                .OrderByDescending(tp => tp.Tournament.StartDate)
                .Select(tp => new TournamentHistoryDto(
                    tp.TournamentId,
                    tp.Tournament.Name,
                    tp.Tournament.StartDate,
                    tp.Tournament.EndDate,
                    tp.Tournament.Category,
                    tp.Result,
                    tp.Placement
                ))
                .ToList(),
            activeSub != null ? new SubscriptionDto(
                activeSub.Id,
                activeSub.PlayerId,
                $"{player.FirstName} {player.LastName}",
                activeSub.PlanName,
                activeSub.TrainingsPerWeek,
                activeSub.TotalTrainingsPerMonth,
                activeSub.TrainingsUsedThisMonth,
                activeSub.TrainingsRemaining,
                activeSub.MonthlyPrice,
                activeSub.AmountPaid,
                activeSub.BalanceDue,
                activeSub.StartDate,
                activeSub.RenewalDate,
                activeSub.IsActive
            ) : null
        );
    }

    public async Task<DashboardDto> GetDashboardAsync()
    {
        var now = DateTime.UtcNow;

        var totalPlayers = await _db.Players.CountAsync();
        var activePlayers = await _db.Players.CountAsync(p => p.IsActive);
        var totalTrainings = await _db.TrainingSessions.CountAsync();
        var upcomingTrainings = await _db.TrainingSessions.CountAsync(t => t.ScheduledAt > now);
        var totalTournaments = await _db.Tournaments.CountAsync();
        var upcomingTournaments = await _db.Tournaments.CountAsync(t => t.StartDate > now);

        var recentPlayers = await _db.Players
            .Include(p => p.Subscriptions)
            .OrderByDescending(p => p.CreatedAt)
            .Take(5)
            .Select(p => new PlayerSummaryDto(
                p.Id,
                p.FirstName,
                p.LastName,
                p.Email,
                p.Level,
                p.IsActive,
                p.Subscriptions
                    .Where(s => s.IsActive)
                    .Select(s => s.TotalTrainingsPerMonth - s.TrainingsUsedThisMonth)
                    .FirstOrDefault()
            ))
            .ToListAsync();

        var upcomingTrainingsList = await _db.TrainingSessions
            .Include(t => t.Attendances)
            .Where(t => t.ScheduledAt > now)
            .OrderBy(t => t.ScheduledAt)
            .Take(5)
            .Select(t => new TrainingSessionDto(
                t.Id, t.Title, t.Description, t.ScheduledAt, t.DurationMinutes,
                t.CourtName, t.TrainerName, t.MaxParticipants, t.Attendances.Count,
                t.Status.ToString(), t.CreatedAt
            ))
            .ToListAsync();

        var upcomingTournamentsList = await _db.Tournaments
            .Include(t => t.Participations)
            .Where(t => t.StartDate > now)
            .OrderBy(t => t.StartDate)
            .Take(5)
            .Select(t => new TournamentDto(
                t.Id, t.Name, t.Description, t.Location, t.StartDate, t.EndDate,
                t.MaxParticipants, t.Participations.Count, t.Category,
                t.Status.ToString(), t.CreatedAt
            ))
            .ToListAsync();

        return new DashboardDto(
            totalPlayers, activePlayers, totalTrainings, upcomingTrainings,
            totalTournaments, upcomingTournaments,
            recentPlayers, upcomingTrainingsList, upcomingTournamentsList
        );
    }
}
