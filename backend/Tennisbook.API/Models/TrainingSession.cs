using System.ComponentModel.DataAnnotations;

namespace Tennisbook.API.Models;

public class TrainingSession
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime ScheduledAt { get; set; }
    public int DurationMinutes { get; set; } = 60;

    [MaxLength(100)]
    public string? CourtName { get; set; }

    [MaxLength(100)]
    public string? TrainerName { get; set; }

    public int MaxParticipants { get; set; } = 10;

    public TrainingStatus Status { get; set; } = TrainingStatus.Scheduled;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<TrainingAttendance> Attendances { get; set; } = new List<TrainingAttendance>();
}

public enum TrainingStatus
{
    Scheduled,
    InProgress,
    Completed,
    Cancelled
}
