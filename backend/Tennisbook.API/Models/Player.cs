using System.ComponentModel.DataAnnotations;

namespace Tennisbook.API.Models;

public class Player
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Phone { get; set; }

    public DateTime DateOfBirth { get; set; }

    [MaxLength(20)]
    public string? Level { get; set; } // Beginner, Intermediate, Advanced, Pro

    public string? ProfileImageUrl { get; set; }

    public string QrCode { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    public ICollection<TrainingAttendance> TrainingAttendances { get; set; } = new List<TrainingAttendance>();
    public ICollection<TournamentParticipation> TournamentParticipations { get; set; } = new List<TournamentParticipation>();
}
