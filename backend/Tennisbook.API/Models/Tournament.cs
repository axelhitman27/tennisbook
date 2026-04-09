using System.ComponentModel.DataAnnotations;

namespace Tennisbook.API.Models;

public class Tournament
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [MaxLength(100)]
    public string? Location { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public int MaxParticipants { get; set; } = 32;

    [MaxLength(50)]
    public string? Category { get; set; } // Singles, Doubles, Mixed

    public TournamentStatus Status { get; set; } = TournamentStatus.Upcoming;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<TournamentParticipation> Participations { get; set; } = new List<TournamentParticipation>();
}

public enum TournamentStatus
{
    Upcoming,
    InProgress,
    Completed,
    Cancelled
}
