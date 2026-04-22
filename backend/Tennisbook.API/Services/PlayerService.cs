using Microsoft.EntityFrameworkCore;
using Tennisbook.API.Data;
using Tennisbook.API.DTOs;
using Tennisbook.API.Models;

namespace Tennisbook.API.Services;

public class PlayerService
{
    private readonly TennisbookDbContext _db;
    private readonly QrCodeService _qrService;

    public PlayerService(TennisbookDbContext db, QrCodeService qrService)
    {
        _db = db;
        _qrService = qrService;
    }

    public async Task<List<PlayerSummaryDto>> GetAllAsync()
    {
        return await _db.Players
            .Include(p => p.Subscriptions)
            .Select(p => new PlayerSummaryDto(
                p.Id,
                p.FirstName,
                p.LastName,
                p.Email,
                p.Level,
                p.IsActive,
                p.Subscriptions
                    .Where(s => s.IsActive)
                    .Select(s => s.TotalTrainingsPerMonth - s.TrainingsUsedThisMonth)
                    .FirstOrDefault()
            ))
            .ToListAsync();
    }

    public async Task<PlayerDto?> GetByIdAsync(int id)
    {
        var player = await _db.Players
            .Include(p => p.Subscriptions)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (player == null) return null;

        var activeSub = player.Subscriptions.FirstOrDefault(s => s.IsActive);

        return new PlayerDto(
            player.Id,
            player.FirstName,
            player.LastName,
            player.Email,
            player.Phone,
            player.DateOfBirth,
            player.Level,
            player.QrCode,
            player.IsActive,
            player.CreatedAt,
            activeSub != null ? MapSubscription(activeSub) : null
        );
    }

    public async Task<PlayerDto> CreateAsync(CreatePlayerDto dto)
    {
        var player = new Player
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            DateOfBirth = dto.DateOfBirth,
            Level = dto.Level,
            QrCode = Guid.NewGuid().ToString()
        };

        _db.Players.Add(player);
        await _db.SaveChangesAsync();

        return new PlayerDto(
            player.Id,
            player.FirstName,
            player.LastName,
            player.Email,
            player.Phone,
            player.DateOfBirth,
            player.Level,
            player.QrCode,
            player.IsActive,
            player.CreatedAt,
            null
        );
    }

    public async Task<PlayerDto?> UpdateAsync(int id, UpdatePlayerDto dto)
    {
        var player = await _db.Players
            .Include(p => p.Subscriptions)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (player == null) return null;

        if (dto.FirstName != null) player.FirstName = dto.FirstName;
        if (dto.LastName != null) player.LastName = dto.LastName;
        if (dto.Email != null) player.Email = dto.Email;
        if (dto.Phone != null) player.Phone = dto.Phone;
        if (dto.DateOfBirth.HasValue) player.DateOfBirth = dto.DateOfBirth.Value;
        if (dto.Level != null) player.Level = dto.Level;
        if (dto.IsActive.HasValue) player.IsActive = dto.IsActive.Value;

        await _db.SaveChangesAsync();

        var activeSub = player.Subscriptions.FirstOrDefault(s => s.IsActive);
        return new PlayerDto(
            player.Id,
            player.FirstName,
            player.LastName,
            player.Email,
            player.Phone,
            player.DateOfBirth,
            player.Level,
            player.QrCode,
            player.IsActive,
            player.CreatedAt,
            activeSub != null ? MapSubscription(activeSub) : null
        );
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var player = await _db.Players.FindAsync(id);
        if (player == null) return false;

        _db.Players.Remove(player);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<PlayerDto?> GetByQrCodeAsync(string qrCode)
    {
        var player = await _db.Players
            .Include(p => p.Subscriptions)
            .FirstOrDefaultAsync(p => p.QrCode == qrCode);

        if (player == null) return null;

        var activeSub = player.Subscriptions.FirstOrDefault(s => s.IsActive);
        return new PlayerDto(
            player.Id,
            player.FirstName,
            player.LastName,
            player.Email,
            player.Phone,
            player.DateOfBirth,
            player.Level,
            player.QrCode,
            player.IsActive,
            player.CreatedAt,
            activeSub != null ? MapSubscription(activeSub) : null
        );
    }

    public byte[] GenerateQrImage(string qrCode)
    {
        return _qrService.GenerateQrCode(qrCode);
    }

    private static SubscriptionDto MapSubscription(Subscription s) => new(
        s.Id,
        s.PlayerId,
        "",
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
