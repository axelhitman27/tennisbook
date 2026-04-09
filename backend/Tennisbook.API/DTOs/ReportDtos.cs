namespace Tennisbook.API.DTOs;

public record PlayerReportDto(
    int PlayerId,
    string PlayerName,
    string? Level,
    int TotalTrainings,
    int TrainingsThisMonth,
    int TotalTournaments,
    List<TrainingHistoryDto> RecentTrainings,
    List<TournamentHistoryDto> TournamentHistory,
    SubscriptionDto? ActiveSubscription
);

public record TrainingHistoryDto(
    int TrainingSessionId,
    string Title,
    DateTime ScheduledAt,
    string? CourtName,
    string? TrainerName,
    DateTime CheckedInAt
);

public record TournamentHistoryDto(
    int TournamentId,
    string TournamentName,
    DateTime StartDate,
    DateTime EndDate,
    string? Category,
    string? Result,
    int? Placement
);

public record QrScanResultDto(
    bool Success,
    string Message,
    int? PlayerId,
    string? PlayerName,
    int? TrainingsRemaining
);

public record DashboardDto(
    int TotalPlayers,
    int ActivePlayers,
    int TotalTrainingSessions,
    int UpcomingTrainingSessions,
    int TotalTournaments,
    int UpcomingTournaments,
    List<PlayerSummaryDto> RecentPlayers,
    List<TrainingSessionDto> UpcomingTrainings,
    List<TournamentDto> UpcomingTournamentsList
);
