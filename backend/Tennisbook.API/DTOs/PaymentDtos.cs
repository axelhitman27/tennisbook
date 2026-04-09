namespace Tennisbook.API.DTOs;

public record CreatePaymentDto(
    int PlayerId,
    int? TrainingAttendanceId,
    decimal Amount,
    string? Description
);

public record PaymentDto(
    int Id,
    int PlayerId,
    string PlayerName,
    int? TrainingAttendanceId,
    string? TrainingTitle,
    decimal Amount,
    string PaymentType,
    string? Description,
    bool IsPaid,
    DateTime CreatedAt,
    DateTime? PaidAt
);
