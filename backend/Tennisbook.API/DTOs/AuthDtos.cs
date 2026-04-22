namespace Tennisbook.API.DTOs;

public record LoginDto(string Email, string Password);

public record RegisterDto(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? Phone,
    DateTime DateOfBirth,
    string? Level
);

public record AuthResponseDto(
    string Token,
    string Role,
    int UserId,
    int? PlayerId,
    string Email
);

public record ChangePasswordDto(string CurrentPassword, string NewPassword);
