using System.ComponentModel.DataAnnotations;

namespace Tennisbook.API.Models;

public class TournamentParticipation
{
    public int Id { get; set; }

    public int PlayerId { get; set; }
    public Player Player { get; set; } = null!;

    public int TournamentId { get; set; }
    public Tournament Tournament { get; set; } = null!;

    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

    [MaxLength(50)]
    public string? Result { get; set; } // Winner, Runner-up, Semi-finalist, etc.

    public int? Placement { get; set; }
}
