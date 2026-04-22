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

    [MaxLength(30)]
    public string Format { get; set; } = "SingleElimination"; // SingleElimination, RoundRobin

    [MaxLength(30)]
    public string? Surface { get; set; } // Clay, Hard, Grass, Indoor

    public int SetsToWin { get; set; } = 2; // Best of 3 (2 sets to win) or Best of 5 (3)

    public TournamentStatus Status { get; set; } = TournamentStatus.Upcoming;

    public bool DrawGenerated { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<TournamentParticipation> Participations { get; set; } = new List<TournamentParticipation>();
    public ICollection<TournamentMatch> Matches { get; set; } = new List<TournamentMatch>();
}

public enum TournamentStatus
{
    Upcoming,
    InProgress,
    Completed,
    Cancelled
}
