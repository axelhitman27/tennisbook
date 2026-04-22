namespace Tennisbook.API.Models;

public class TrainingEnrollment
{
    public int Id { get; set; }

    public int PlayerId { get; set; }
    public Player Player { get; set; } = null!;

    public int TrainingSessionId { get; set; }
    public TrainingSession TrainingSession { get; set; } = null!;

    public bool IsActive { get; set; } = true;

    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
}
