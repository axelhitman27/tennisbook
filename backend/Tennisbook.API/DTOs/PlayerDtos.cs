namespace Tennisbook.API.DTOs;

public record CreatePlayerDto(
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    DateTime DateOfBirth,
    string? Level
);

public record UpdatePlayerDto(
    string? FirstName,
    string? LastName,
    string? Email,
    string? Phone,
    DateTime? DateOfBirth,
    string? Level,
    bool? IsActive
);

public record PlayerDto(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    DateTime DateOfBirth,
    string? Level,
    string QrCode,
    bool IsActive,
    DateTime CreatedAt,
    SubscriptionDto? ActiveSubscription
);

public record PlayerSummaryDto(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    string? Level,
    bool IsActive,
    int? TrainingsRemaining
);
