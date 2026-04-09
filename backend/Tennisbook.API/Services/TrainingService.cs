using Microsoft.EntityFrameworkCore;
using Tennisbook.API.Data;
using Tennisbook.API.DTOs;
using Tennisbook.API.Models;

namespace Tennisbook.API.Services;

public class TrainingService
{
    private readonly TennisbookDbContext _db;

    public TrainingService(TennisbookDbContext db) => _db = db;

    public async Task<List<TrainingSessionDto>> GetAllAsync()
    {
        return await _db.TrainingSessions
            .Include(t => t.Attendances)
            .Select(t => MapToDto(t))
            .ToListAsync();
    }

    public async Task<TrainingSessionDto?> GetByIdAsync(int id)
    {
        var session = await _db.TrainingSessions
            .Include(t => t.Attendances)
            .FirstOrDefaultAsync(t => t.Id == id);
        return session == null ? null : MapToDto(session);
    }

    public async Task<TrainingSessionDto> CreateAsync(CreateTrainingSessionDto dto)
    {
        var session = new TrainingSession
        {
            Title = dto.Title,
            Description = dto.Description,
            ScheduledAt = dto.ScheduledAt,
            DurationMinutes = dto.DurationMinutes,
            CourtName = dto.CourtName,
            TrainerName = dto.TrainerName,
            MaxParticipants = dto.MaxParticipants
        };

        _db.TrainingSessions.Add(session);
        await _db.SaveChangesAsync();
        return MapToDto(session);
    }

    public async Task<TrainingSessionDto?> UpdateAsync(int id, UpdateTrainingSessionDto dto)
    {
        var session = await _db.TrainingSessions
            .Include(t => t.Attendances)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (session == null) return null;

        if (dto.Title != null) session.Title = dto.Title;
        if (dto.Description != null) session.Description = dto.Description;
        if (dto.ScheduledAt.HasValue) session.ScheduledAt = dto.ScheduledAt.Value;
        if (dto.DurationMinutes.HasValue) session.DurationMinutes = dto.DurationMinutes.Value;
        if (dto.CourtName != null) session.CourtName = dto.CourtName;
        if (dto.TrainerName != null) session.TrainerName = dto.TrainerName;
        if (dto.MaxParticipants.HasValue) session.MaxParticipants = dto.MaxParticipants.Value;
        if (dto.Status.HasValue) session.Status = dto.Status.Value;

        await _db.SaveChangesAsync();
        return MapToDto(session);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var session = await _db.TrainingSessions.FindAsync(id);
        if (session == null) return false;

        _db.TrainingSessions.Remove(session);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<QrScanResultDto> CheckInByQrAsync(string qrCode, int trainingSessionId)
    {
        var player = await _db.Players
            .Include(p => p.Subscriptions)
            .FirstOrDefaultAsync(p => p.QrCode == qrCode);

        if (player == null)
            return new QrScanResultDto(false, "Player not found", null, null, null);

        var session = await _db.TrainingSessions.FindAsync(trainingSessionId);
        if (session == null)
            return new QrScanResultDto(false, "Training session not found", player.Id, $"{player.FirstName} {player.LastName}", null);

        var existingAttendance = await _db.TrainingAttendances
            .AnyAsync(a => a.PlayerId == player.Id && a.TrainingSessionId == trainingSessionId);

        if (existingAttendance)
            return new QrScanResultDto(false, "Player already checked in for this session",
                player.Id, $"{player.FirstName} {player.LastName}", null);

        var activeSub = player.Subscriptions.FirstOrDefault(s => s.IsActive);
        if (activeSub == null)
            return new QrScanResultDto(false, "No active subscription",
                player.Id, $"{player.FirstName} {player.LastName}", 0);

        if (activeSub.TrainingsRemaining <= 0)
            return new QrScanResultDto(false, "No trainings remaining this month",
                player.Id, $"{player.FirstName} {player.LastName}", 0);

        var attendance = new TrainingAttendance
        {
            PlayerId = player.Id,
            TrainingSessionId = trainingSessionId,
            WasQrScanned = true
        };

        _db.TrainingAttendances.Add(attendance);
        activeSub.TrainingsUsedThisMonth++;
        await _db.SaveChangesAsync();

        return new QrScanResultDto(true, "Check-in successful",
            player.Id, $"{player.FirstName} {player.LastName}", activeSub.TrainingsRemaining);
    }

    public async Task<List<TrainingAttendanceDto>> GetAttendancesAsync(int trainingSessionId)
    {
        return await _db.TrainingAttendances
            .Include(a => a.Player)
            .Include(a => a.TrainingSession)
            .Where(a => a.TrainingSessionId == trainingSessionId)
            .Select(a => new TrainingAttendanceDto(
                a.Id,
                a.PlayerId,
                a.Player.FirstName + " " + a.Player.LastName,
                a.TrainingSessionId,
                a.TrainingSession.Title,
                a.CheckedInAt,
                a.WasQrScanned
            ))
            .ToListAsync();
    }

    private static TrainingSessionDto MapToDto(TrainingSession t) => new(
        t.Id,
        t.Title,
        t.Description,
        t.ScheduledAt,
        t.DurationMinutes,
        t.CourtName,
        t.TrainerName,
        t.MaxParticipants,
        t.Attendances.Count,
        t.Status.ToString(),
        t.CreatedAt
    );
}
