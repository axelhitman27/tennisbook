using Tennisbook.API.Models;

namespace Tennisbook.API.DTOs;

public record CreateTournamentDto(
    string Name,
    string? Description,
    string? Location,
    DateTime StartDate,
    DateTime EndDate,
    int MaxParticipants,
    string? Category,
    string Format = "SingleElimination",
    string? Surface = null,
    int SetsToWin = 2
);

public record UpdateTournamentDto(
    string? Name,
    string? Description,
    string? Location,
    DateTime? StartDate,
    DateTime? EndDate,
    int? MaxParticipants,
    string? Category,
    TournamentStatus? Status,
    string? Format = null,
    string? Surface = null,
    int? SetsToWin = null
);

public record TournamentDto(
    int Id,
    string Name,
    string? Description,
    string? Location,
    DateTime StartDate,
    DateTime EndDate,
    int MaxParticipants,
    int CurrentParticipants,
    string? Category,
    string Format,
    string? Surface,
    int SetsToWin,
    bool DrawGenerated,
    string Status,
    DateTime CreatedAt
);

public record TournamentParticipationDto(
    int Id,
    int PlayerId,
    string PlayerName,
    int TournamentId,
    string TournamentName,
    DateTime RegisteredAt,
    string? Result,
    int? Placement
);

public record RegisterTournamentDto(
    int PlayerId,
    int TournamentId
);

public record UpdateParticipationResultDto(
    string? Result,
    int? Placement
);

public record TournamentMatchDto(
    int Id,
    int TournamentId,
    int Round,
    int MatchNumber,
    int? Player1Id,
    string? Player1Name,
    int? Player2Id,
    string? Player2Name,
    int? WinnerId,
    string? WinnerName,
    int? NextMatchId,
    string? Score,
    int Player1Set1, int Player2Set1,
    int Player1Set2, int Player2Set2,
    int Player1Set3, int Player2Set3,
    string Status,
    DateTime? ScheduledAt,
    string? CourtName
);

public record UpdateMatchScoreDto(
    int Player1Set1, int Player2Set1,
    int Player1Set2, int Player2Set2,
    int Player1Set3 = 0, int Player2Set3 = 0,
    int? WinnerId = null
);
