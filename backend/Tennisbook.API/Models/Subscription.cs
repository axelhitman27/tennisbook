using System.ComponentModel.DataAnnotations;

namespace Tennisbook.API.Models;

public class Subscription
{
    public int Id { get; set; }

    public int PlayerId { get; set; }
    public Player Player { get; set; } = null!;

    [Required, MaxLength(100)]
    public string PlanName { get; set; } = string.Empty;

    public int TrainingsPerWeek { get; set; }
    public int TotalTrainingsPerMonth { get; set; }
    public int TrainingsUsedThisMonth { get; set; }

    public decimal MonthlyPrice { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal BalanceDue => MonthlyPrice - AmountPaid;

    public DateTime StartDate { get; set; }
    public DateTime RenewalDate { get; set; }
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int TrainingsRemaining => TotalTrainingsPerMonth - TrainingsUsedThisMonth;
}
