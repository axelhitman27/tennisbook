using Tennisbook.API.Models;

namespace Tennisbook.API.DTOs;

public record CreateTrainingSessionDto(
    string Title,
    string? Description,
    DateTime ScheduledAt,
    int DurationMinutes,
    string? CourtName,
    string? TrainerName,
    int MaxParticipants
);

public record UpdateTrainingSessionDto(
    string? Title,
    string? Description,
    DateTime? ScheduledAt,
    int? DurationMinutes,
    string? CourtName,
    string? TrainerName,
    int? MaxParticipants,
    TrainingStatus? Status
);

public record TrainingSessionDto(
    int Id,
    string Title,
    string? Description,
    DateTime ScheduledAt,
    int DurationMinutes,
    string? CourtName,
    string? TrainerName,
    int MaxParticipants,
    int CurrentParticipants,
    string Status,
    DateTime CreatedAt
);

public record TrainingAttendanceDto(
    int Id,
    int PlayerId,
    string PlayerName,
    int TrainingSessionId,
    string TrainingTitle,
    DateTime CheckedInAt,
    bool WasQrScanned
);
