using Microsoft.EntityFrameworkCore;
using Tennisbook.API.Data;
using Tennisbook.API.DTOs;
using Tennisbook.API.Models;

namespace Tennisbook.API.Services;

public class TournamentService
{
    private readonly TennisbookDbContext _db;

    public TournamentService(TennisbookDbContext db) => _db = db;

    public async Task<List<TournamentDto>> GetAllAsync()
    {
        return await _db.Tournaments
            .Include(t => t.Participations)
            .Select(t => MapToDto(t))
            .ToListAsync();
    }

    public async Task<TournamentDto?> GetByIdAsync(int id)
    {
        var tournament = await _db.Tournaments
            .Include(t => t.Participations)
            .FirstOrDefaultAsync(t => t.Id == id);
        return tournament == null ? null : MapToDto(tournament);
    }

    public async Task<TournamentDto> CreateAsync(CreateTournamentDto dto)
    {
        var tournament = new Tournament
        {
            Name = dto.Name,
            Description = dto.Description,
            Location = dto.Location,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            MaxParticipants = dto.MaxParticipants,
            Category = dto.Category
        };

        _db.Tournaments.Add(tournament);
        await _db.SaveChangesAsync();
        return MapToDto(tournament);
    }

    public async Task<TournamentDto?> UpdateAsync(int id, UpdateTournamentDto dto)
    {
        var tournament = await _db.Tournaments
            .Include(t => t.Participations)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (tournament == null) return null;

        if (dto.Name != null) tournament.Name = dto.Name;
        if (dto.Description != null) tournament.Description = dto.Description;
        if (dto.Location != null) tournament.Location = dto.Location;
        if (dto.StartDate.HasValue) tournament.StartDate = dto.StartDate.Value;
        if (dto.EndDate.HasValue) tournament.EndDate = dto.EndDate.Value;
        if (dto.MaxParticipants.HasValue) tournament.MaxParticipants = dto.MaxParticipants.Value;
        if (dto.Category != null) tournament.Category = dto.Category;
        if (dto.Status.HasValue) tournament.Status = dto.Status.Value;

        await _db.SaveChangesAsync();
        return MapToDto(tournament);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var tournament = await _db.Tournaments.FindAsync(id);
        if (tournament == null) return false;

        _db.Tournaments.Remove(tournament);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<TournamentParticipationDto?> RegisterPlayerAsync(RegisterTournamentDto dto)
    {
        var player = await _db.Players.FindAsync(dto.PlayerId);
        if (player == null) return null;

        var tournament = await _db.Tournaments.FindAsync(dto.TournamentId);
        if (tournament == null) return null;

        var existing = await _db.TournamentParticipations
            .AnyAsync(tp => tp.PlayerId == dto.PlayerId && tp.TournamentId == dto.TournamentId);
        if (existing) return null;

        var participation = new TournamentParticipation
        {
            PlayerId = dto.PlayerId,
            TournamentId = dto.TournamentId
        };

        _db.TournamentParticipations.Add(participation);
        await _db.SaveChangesAsync();

        return new TournamentParticipationDto(
            participation.Id,
            player.Id,
            $"{player.FirstName} {player.LastName}",
            tournament.Id,
            tournament.Name,
            participation.RegisteredAt,
            participation.Result,
            participation.Placement
        );
    }

    public async Task<List<TournamentParticipationDto>> GetParticipantsAsync(int tournamentId)
    {
        return await _db.TournamentParticipations
            .Include(tp => tp.Player)
            .Include(tp => tp.Tournament)
            .Where(tp => tp.TournamentId == tournamentId)
            .Select(tp => new TournamentParticipationDto(
                tp.Id,
                tp.PlayerId,
                tp.Player.FirstName + " " + tp.Player.LastName,
                tp.TournamentId,
                tp.Tournament.Name,
                tp.RegisteredAt,
                tp.Result,
                tp.Placement
            ))
            .ToListAsync();
    }

    public async Task<TournamentParticipationDto?> UpdateResultAsync(int participationId, UpdateParticipationResultDto dto)
    {
        var participation = await _db.TournamentParticipations
            .Include(tp => tp.Player)
            .Include(tp => tp.Tournament)
            .FirstOrDefaultAsync(tp => tp.Id == participationId);

        if (participation == null) return null;

        if (dto.Result != null) participation.Result = dto.Result;
        if (dto.Placement.HasValue) participation.Placement = dto.Placement.Value;

        await _db.SaveChangesAsync();

        return new TournamentParticipationDto(
            participation.Id,
            participation.PlayerId,
            $"{participation.Player.FirstName} {participation.Player.LastName}",
            participation.TournamentId,
            participation.Tournament.Name,
            participation.RegisteredAt,
            participation.Result,
            participation.Placement
        );
    }

    private static TournamentDto MapToDto(Tournament t) => new(
        t.Id,
        t.Name,
        t.Description,
        t.Location,
        t.StartDate,
        t.EndDate,
        t.MaxParticipants,
        t.Participations.Count,
        t.Category,
        t.Status.ToString(),
        t.CreatedAt
    );
}
