using System.ComponentModel.DataAnnotations;

namespace Tennisbook.API.Models;

public class TournamentMatch
{
    public int Id { get; set; }

    public int TournamentId { get; set; }
    public Tournament Tournament { get; set; } = null!;

    public int Round { get; set; }
    public int MatchNumber { get; set; }

    public int? Player1Id { get; set; }
    public Player? Player1 { get; set; }

    public int? Player2Id { get; set; }
    public Player? Player2 { get; set; }

    public int? WinnerId { get; set; }
    public Player? Winner { get; set; }

    public int? NextMatchId { get; set; }
    public TournamentMatch? NextMatch { get; set; }

    [MaxLength(50)]
    public string? Score { get; set; } // e.g. "6-4, 3-6, 7-5"

    public int Player1Set1 { get; set; }
    public int Player2Set1 { get; set; }
    public int Player1Set2 { get; set; }
    public int Player2Set2 { get; set; }
    public int Player1Set3 { get; set; }
    public int Player2Set3 { get; set; }

    public MatchStatus Status { get; set; } = MatchStatus.Pending;

    public DateTime? ScheduledAt { get; set; }

    [MaxLength(100)]
    public string? CourtName { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum MatchStatus
{
    Pending,
    InProgress,
    Completed,
    Walkover
}
