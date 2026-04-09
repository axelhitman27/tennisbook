using Microsoft.EntityFrameworkCore;
using Tennisbook.API.Data;
using Tennisbook.API.DTOs;
using Tennisbook.API.Models;

namespace Tennisbook.API.Services;

public class SubscriptionService
{
    private readonly TennisbookDbContext _db;

    public SubscriptionService(TennisbookDbContext db) => _db = db;

    public async Task<List<SubscriptionDto>> GetAllAsync()
    {
        return await _db.Subscriptions
            .Include(s => s.Player)
            .Select(s => MapToDto(s))
            .ToListAsync();
    }

    public async Task<SubscriptionDto?> GetByIdAsync(int id)
    {
        var sub = await _db.Subscriptions
            .Include(s => s.Player)
            .FirstOrDefaultAsync(s => s.Id == id);
        return sub == null ? null : MapToDto(sub);
    }

    public async Task<List<SubscriptionDto>> GetByPlayerIdAsync(int playerId)
    {
        return await _db.Subscriptions
            .Include(s => s.Player)
            .Where(s => s.PlayerId == playerId)
            .Select(s => MapToDto(s))
            .ToListAsync();
    }

    public async Task<SubscriptionDto> CreateAsync(CreateSubscriptionDto dto)
    {
        // Deactivate any existing active subscriptions for this player
        var existingActive = await _db.Subscriptions
            .Where(s => s.PlayerId == dto.PlayerId && s.IsActive)
            .ToListAsync();
        foreach (var s in existingActive) s.IsActive = false;

        var sub = new Subscription
        {
            PlayerId = dto.PlayerId,
            PlanName = dto.PlanName,
            TrainingsPerWeek = dto.TrainingsPerWeek,
            TotalTrainingsPerMonth = dto.TotalTrainingsPerMonth,
            MonthlyPrice = dto.MonthlyPrice,
            StartDate = dto.StartDate,
            RenewalDate = dto.StartDate.AddMonths(1),
            IsActive = true
        };

        _db.Subscriptions.Add(sub);
        await _db.SaveChangesAsync();

        await _db.Entry(sub).Reference(s => s.Player).LoadAsync();
        return MapToDto(sub);
    }

    public async Task<SubscriptionDto?> UpdateAsync(int id, UpdateSubscriptionDto dto)
    {
        var sub = await _db.Subscriptions
            .Include(s => s.Player)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (sub == null) return null;

        if (dto.PlanName != null) sub.PlanName = dto.PlanName;
        if (dto.TrainingsPerWeek.HasValue) sub.TrainingsPerWeek = dto.TrainingsPerWeek.Value;
        if (dto.TotalTrainingsPerMonth.HasValue) sub.TotalTrainingsPerMonth = dto.TotalTrainingsPerMonth.Value;
        if (dto.MonthlyPrice.HasValue) sub.MonthlyPrice = dto.MonthlyPrice.Value;
        if (dto.AmountPaid.HasValue) sub.AmountPaid = dto.AmountPaid.Value;
        if (dto.IsActive.HasValue) sub.IsActive = dto.IsActive.Value;

        await _db.SaveChangesAsync();
        return MapToDto(sub);
    }

    private static SubscriptionDto MapToDto(Subscription s) => new(
        s.Id,
        s.PlayerId,
        s.Player != null ? $"{s.Player.FirstName} {s.Player.LastName}" : "",
        s.PlanName,
        s.TrainingsPerWeek,
        s.TotalTrainingsPerMonth,
        s.TrainingsUsedThisMonth,
        s.TrainingsRemaining,
        s.MonthlyPrice,
        s.AmountPaid,
        s.BalanceDue,
        s.StartDate,
        s.RenewalDate,
        s.IsActive
    );
}
