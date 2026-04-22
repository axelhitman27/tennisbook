namespace Tennisbook.API.Models;

public class TrainingAttendance
{
    public int Id { get; set; }

    public int PlayerId { get; set; }
    public Player Player { get; set; } = null!;

    public int TrainingSessionId { get; set; }
    public TrainingSession TrainingSession { get; set; } = null!;

    public DateTime CheckedInAt { get; set; } = DateTime.UtcNow;
    public bool WasQrScanned { get; set; }
}
