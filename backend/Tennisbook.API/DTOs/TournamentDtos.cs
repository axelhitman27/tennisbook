using Tennisbook.API.Models;

namespace Tennisbook.API.DTOs;

public record CreateTournamentDto(
    string Name,
    string? Description,
    string? Location,
    DateTime StartDate,
    DateTime EndDate,
    int MaxParticipants,
    string? Category
);

public record UpdateTournamentDto(
    string? Name,
    string? Description,
    string? Location,
    DateTime? StartDate,
    DateTime? EndDate,
    int? MaxParticipants,
    string? Category,
    TournamentStatus? Status
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
