using Tennisbook.API.Models;

namespace Tennisbook.API.DTOs;

public record CreateTrainingSessionDto(
    string Title,
    string? Description,
    DateTime ScheduledAt,
    int DurationMinutes,
    string? CourtName,
    string? TrainerName,
    int MaxParticipants,
    bool IsRecurring = false,
    DayOfWeek? RecurrenceDay = null,
    string? RecurrenceTime = null
);

public record UpdateTrainingSessionDto(
    string? Title,
    string? Description,
    DateTime? ScheduledAt,
    int? DurationMinutes,
    string? CourtName,
    string? TrainerName,
    int? MaxParticipants,
    TrainingStatus? Status,
    bool? IsRecurring = null,
    DayOfWeek? RecurrenceDay = null,
    string? RecurrenceTime = null
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
    bool IsRecurring,
    string? RecurrenceDay,
    string? RecurrenceTime,
    int EnrolledCount,
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

public record TrainingEnrollmentDto(
    int Id,
    int PlayerId,
    string PlayerName,
    int TrainingSessionId,
    string TrainingTitle,
    string? RecurrenceDay,
    string? RecurrenceTime,
    string? CourtName,
    bool IsActive,
    DateTime EnrolledAt
);

public record EnrollPlayerDto(
    int PlayerId,
    int TrainingSessionId
);
