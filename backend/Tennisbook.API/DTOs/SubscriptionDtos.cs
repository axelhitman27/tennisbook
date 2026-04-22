namespace Tennisbook.API.DTOs;

public record CreateSubscriptionDto(
    int PlayerId,
    string PlanName,
    int TrainingsPerWeek,
    int TotalTrainingsPerMonth,
    decimal MonthlyPrice,
    DateTime StartDate
);

public record UpdateSubscriptionDto(
    string? PlanName,
    int? TrainingsPerWeek,
    int? TotalTrainingsPerMonth,
    decimal? MonthlyPrice,
    decimal? AmountPaid,
    bool? IsActive
);

public record SubscriptionDto(
    int Id,
    int PlayerId,
    string PlayerName,
    string PlanName,
    int TrainingsPerWeek,
    int TotalTrainingsPerMonth,
    int TrainingsUsedThisMonth,
    int TrainingsRemaining,
    decimal MonthlyPrice,
    decimal AmountPaid,
    decimal BalanceDue,
    DateTime StartDate,
    DateTime RenewalDate,
    bool IsActive
);
