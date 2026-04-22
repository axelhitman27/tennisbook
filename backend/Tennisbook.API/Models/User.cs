using System.ComponentModel.DataAnnotations;

namespace Tennisbook.API.Models;

public class User
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string Role { get; set; } = "Player"; // Admin or Player

    public int? PlayerId { get; set; }
    public Player? Player { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
