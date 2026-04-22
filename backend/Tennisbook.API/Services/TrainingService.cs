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
            .Include(t => t.Enrollments)
            .Select(t => MapToDto(t))
            .ToListAsync();
    }

    public async Task<TrainingSessionDto?> GetByIdAsync(int id)
    {
        var session = await _db.TrainingSessions
            .Include(t => t.Attendances)
            .Include(t => t.Enrollments)
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
            MaxParticipants = dto.MaxParticipants,
            IsRecurring = dto.IsRecurring,
            RecurrenceDay = dto.RecurrenceDay,
            RecurrenceTime = dto.RecurrenceTime
        };

        _db.TrainingSessions.Add(session);
        await _db.SaveChangesAsync();
        return MapToDto(session);
    }

    public async Task<TrainingSessionDto?> UpdateAsync(int id, UpdateTrainingSessionDto dto)
    {
        var session = await _db.TrainingSessions
            .Include(t => t.Attendances)
            .Include(t => t.Enrollments)
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
        if (dto.IsRecurring.HasValue) session.IsRecurring = dto.IsRecurring.Value;
        if (dto.RecurrenceDay.HasValue) session.RecurrenceDay = dto.RecurrenceDay.Value;
        if (dto.RecurrenceTime != null) session.RecurrenceTime = dto.RecurrenceTime;

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

        var attendance = new TrainingAttendance
        {
            PlayerId = player.Id,
            TrainingSessionId = trainingSessionId,
            WasQrScanned = true
        };
        _db.TrainingAttendances.Add(attendance);

        var activeSub = player.Subscriptions.FirstOrDefault(s => s.IsActive);

        if (activeSub == null || activeSub.TrainingsRemaining <= 0)
        {
            await _db.SaveChangesAsync();
            return new QrScanResultDto(true, "Check-in successful — payment required",
                player.Id, $"{player.FirstName} {player.LastName}", 0,
                RequiresPayment: true, AttendanceId: attendance.Id);
        }

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

    // --- Enrollment ---

    public async Task<TrainingEnrollmentDto?> EnrollAsync(EnrollPlayerDto dto)
    {
        var player = await _db.Players.FindAsync(dto.PlayerId);
        if (player == null) return null;

        var session = await _db.TrainingSessions.FindAsync(dto.TrainingSessionId);
        if (session == null) return null;

        var existing = await _db.TrainingEnrollments
            .AnyAsync(e => e.PlayerId == dto.PlayerId && e.TrainingSessionId == dto.TrainingSessionId);
        if (existing) return null;

        var enrolledCount = await _db.TrainingEnrollments
            .CountAsync(e => e.TrainingSessionId == dto.TrainingSessionId && e.IsActive);
        if (enrolledCount >= session.MaxParticipants) return null;

        var enrollment = new TrainingEnrollment
        {
            PlayerId = dto.PlayerId,
            TrainingSessionId = dto.TrainingSessionId
        };

        _db.TrainingEnrollments.Add(enrollment);
        await _db.SaveChangesAsync();

        return MapEnrollmentToDto(enrollment, player, session);
    }

    public async Task<bool> UnenrollAsync(int enrollmentId)
    {
        var enrollment = await _db.TrainingEnrollments.FindAsync(enrollmentId);
        if (enrollment == null) return false;

        enrollment.IsActive = false;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<List<TrainingEnrollmentDto>> GetEnrollmentsAsync(int trainingSessionId)
    {
        return await _db.TrainingEnrollments
            .Include(e => e.Player)
            .Include(e => e.TrainingSession)
            .Where(e => e.TrainingSessionId == trainingSessionId && e.IsActive)
            .Select(e => new TrainingEnrollmentDto(
                e.Id,
                e.PlayerId,
                e.Player.FirstName + " " + e.Player.LastName,
                e.TrainingSessionId,
                e.TrainingSession.Title,
                e.TrainingSession.RecurrenceDay != null ? e.TrainingSession.RecurrenceDay.ToString() : null,
                e.TrainingSession.RecurrenceTime,
                e.TrainingSession.CourtName,
                e.IsActive,
                e.EnrolledAt
            ))
            .ToListAsync();
    }

    public async Task<List<TrainingEnrollmentDto>> GetPlayerEnrollmentsAsync(int playerId)
    {
        return await _db.TrainingEnrollments
            .Include(e => e.Player)
            .Include(e => e.TrainingSession)
            .Where(e => e.PlayerId == playerId && e.IsActive)
            .Select(e => new TrainingEnrollmentDto(
                e.Id,
                e.PlayerId,
                e.Player.FirstName + " " + e.Player.LastName,
                e.TrainingSessionId,
                e.TrainingSession.Title,
                e.TrainingSession.RecurrenceDay != null ? e.TrainingSession.RecurrenceDay.ToString() : null,
                e.TrainingSession.RecurrenceTime,
                e.TrainingSession.CourtName,
                e.IsActive,
                e.EnrolledAt
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
        t.IsRecurring,
        t.RecurrenceDay?.ToString(),
        t.RecurrenceTime,
        t.Enrollments.Count(e => e.IsActive),
        t.CreatedAt
    );

    private static TrainingEnrollmentDto MapEnrollmentToDto(TrainingEnrollment e, Player p, TrainingSession s) => new(
        e.Id,
        p.Id,
        $"{p.FirstName} {p.LastName}",
        s.Id,
        s.Title,
        s.RecurrenceDay?.ToString(),
        s.RecurrenceTime,
        s.CourtName,
        e.IsActive,
        e.EnrolledAt
    );
}
