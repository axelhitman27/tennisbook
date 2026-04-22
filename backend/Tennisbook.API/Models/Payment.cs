using System.ComponentModel.DataAnnotations;

namespace Tennisbook.API.Models;

public class Payment
{
    public int Id { get; set; }

    public int PlayerId { get; set; }
    public Player Player { get; set; } = null!;

    public int? TrainingAttendanceId { get; set; }
    public TrainingAttendance? TrainingAttendance { get; set; }

    public decimal Amount { get; set; }

    [MaxLength(50)]
    public string PaymentType { get; set; } = "PerSession"; // PerSession, Subscription

    [MaxLength(200)]
    public string? Description { get; set; }

    public bool IsPaid { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PaidAt { get; set; }
}
