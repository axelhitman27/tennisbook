using Microsoft.EntityFrameworkCore;
using Tennisbook.API.Data;
using Tennisbook.API.DTOs;
using Tennisbook.API.Models;

namespace Tennisbook.API.Services;

public class PaymentService
{
    private readonly TennisbookDbContext _db;

    public PaymentService(TennisbookDbContext db) => _db = db;

    public async Task<PaymentDto> CreateAsync(CreatePaymentDto dto)
    {
        var payment = new Payment
        {
            PlayerId = dto.PlayerId,
            TrainingAttendanceId = dto.TrainingAttendanceId,
            Amount = dto.Amount,
            Description = dto.Description,
            PaymentType = "PerSession",
            IsPaid = false
        };

        _db.Payments.Add(payment);
        await _db.SaveChangesAsync();

        await _db.Entry(payment).Reference(p => p.Player).LoadAsync();
        if (payment.TrainingAttendanceId.HasValue)
            await _db.Entry(payment).Reference(p => p.TrainingAttendance).LoadAsync();

        return MapToDto(payment);
    }

    public async Task<PaymentDto?> MarkPaidAsync(int paymentId)
    {
        var payment = await _db.Payments
            .Include(p => p.Player)
            .Include(p => p.TrainingAttendance)
                .ThenInclude(a => a!.TrainingSession)
            .FirstOrDefaultAsync(p => p.Id == paymentId);

        if (payment == null) return null;

        payment.IsPaid = true;
        payment.PaidAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return MapToDto(payment);
    }

    public async Task<List<PaymentDto>> GetByPlayerAsync(int playerId)
    {
        return await _db.Payments
            .Include(p => p.Player)
            .Include(p => p.TrainingAttendance)
                .ThenInclude(a => a!.TrainingSession)
            .Where(p => p.PlayerId == playerId)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => MapToDto(p))
            .ToListAsync();
    }

    public async Task<List<PaymentDto>> GetPendingAsync()
    {
        return await _db.Payments
            .Include(p => p.Player)
            .Include(p => p.TrainingAttendance)
                .ThenInclude(a => a!.TrainingSession)
            .Where(p => !p.IsPaid)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => MapToDto(p))
            .ToListAsync();
    }

    private static PaymentDto MapToDto(Payment p) => new(
        p.Id,
        p.PlayerId,
        p.Player != null ? $"{p.Player.FirstName} {p.Player.LastName}" : "",
        p.TrainingAttendanceId,
        p.TrainingAttendance?.TrainingSession?.Title,
        p.Amount,
        p.PaymentType,
        p.Description,
        p.IsPaid,
        p.CreatedAt,
        p.PaidAt
    );
}
