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
            Category = dto.Category,
            Format = dto.Format,
            Surface = dto.Surface,
            SetsToWin = dto.SetsToWin
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
        if (dto.Format != null) tournament.Format = dto.Format;
        if (dto.Surface != null) tournament.Surface = dto.Surface;
        if (dto.SetsToWin.HasValue) tournament.SetsToWin = dto.SetsToWin.Value;

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
            participation.Id, player.Id, $"{player.FirstName} {player.LastName}",
            tournament.Id, tournament.Name, participation.RegisteredAt,
            participation.Result, participation.Placement
        );
    }

    public async Task<List<TournamentParticipationDto>> GetParticipantsAsync(int tournamentId)
    {
        return await _db.TournamentParticipations
            .Include(tp => tp.Player).Include(tp => tp.Tournament)
            .Where(tp => tp.TournamentId == tournamentId)
            .Select(tp => new TournamentParticipationDto(
                tp.Id, tp.PlayerId, tp.Player.FirstName + " " + tp.Player.LastName,
                tp.TournamentId, tp.Tournament.Name, tp.RegisteredAt,
                tp.Result, tp.Placement
            ))
            .ToListAsync();
    }

    public async Task<TournamentParticipationDto?> UpdateResultAsync(int participationId, UpdateParticipationResultDto dto)
    {
        var participation = await _db.TournamentParticipations
            .Include(tp => tp.Player).Include(tp => tp.Tournament)
            .FirstOrDefaultAsync(tp => tp.Id == participationId);

        if (participation == null) return null;

        if (dto.Result != null) participation.Result = dto.Result;
        if (dto.Placement.HasValue) participation.Placement = dto.Placement.Value;

        await _db.SaveChangesAsync();

        return new TournamentParticipationDto(
            participation.Id, participation.PlayerId,
            $"{participation.Player.FirstName} {participation.Player.LastName}",
            participation.TournamentId, participation.Tournament.Name,
            participation.RegisteredAt, participation.Result, participation.Placement
        );
    }

    // --- Draw / Bracket Generation ---

    public async Task<List<TournamentMatchDto>> GenerateDrawAsync(int tournamentId)
    {
        var tournament = await _db.Tournaments
            .Include(t => t.Participations).ThenInclude(p => p.Player)
            .Include(t => t.Matches)
            .FirstOrDefaultAsync(t => t.Id == tournamentId);

        if (tournament == null) return new();

        if (tournament.Matches.Any())
            _db.TournamentMatches.RemoveRange(tournament.Matches);

        var players = tournament.Participations
            .Select(p => p.Player)
            .OrderBy(_ => Random.Shared.Next())
            .ToList();

        var count = players.Count;
        if (count < 2) return new();

        int slots = 1;
        while (slots < count) slots *= 2;
        int totalRounds = (int)Math.Log2(slots);

        var allMatches = new List<TournamentMatch>();

        for (int round = 1; round <= totalRounds; round++)
        {
            int matchesInRound = slots / (int)Math.Pow(2, round);
            for (int m = 1; m <= matchesInRound; m++)
            {
                allMatches.Add(new TournamentMatch
                {
                    TournamentId = tournamentId,
                    Round = round,
                    MatchNumber = m
                });
            }
        }

        _db.TournamentMatches.AddRange(allMatches);
        await _db.SaveChangesAsync();

        for (int round = totalRounds; round >= 2; round--)
        {
            var thisRound = allMatches.Where(m => m.Round == round).OrderBy(m => m.MatchNumber).ToList();
            var nextRound = allMatches.Where(m => m.Round == round - 1).OrderBy(m => m.MatchNumber).ToList();
            for (int i = 0; i < thisRound.Count; i++)
            {
                thisRound[i].NextMatchId = nextRound[i / 2].Id;
            }
        }

        var firstRound = allMatches.Where(m => m.Round == totalRounds).OrderBy(m => m.MatchNumber).ToList();
        int slot = 0;
        foreach (var match in firstRound)
        {
            match.Player1Id = slot < count ? players[slot].Id : null;
            slot++;
            match.Player2Id = slot < count ? players[slot].Id : null;
            slot++;

            if (match.Player1Id != null && match.Player2Id == null)
            {
                match.WinnerId = match.Player1Id;
                match.Status = MatchStatus.Walkover;
                match.Score = "BYE";
            }
            else if (match.Player1Id == null && match.Player2Id != null)
            {
                match.WinnerId = match.Player2Id;
                match.Status = MatchStatus.Walkover;
                match.Score = "BYE";
            }
        }

        tournament.DrawGenerated = true;
        tournament.Status = TournamentStatus.InProgress;
        await _db.SaveChangesAsync();

        AdvanceByes(allMatches);
        await _db.SaveChangesAsync();

        return await GetMatchesAsync(tournamentId);
    }

    private void AdvanceByes(List<TournamentMatch> allMatches)
    {
        var walkovers = allMatches.Where(m => m.Status == MatchStatus.Walkover && m.WinnerId != null && m.NextMatchId != null);
        foreach (var wo in walkovers)
        {
            var next = allMatches.FirstOrDefault(m => m.Id == wo.NextMatchId);
            if (next == null) continue;

            if (next.Player1Id == null) next.Player1Id = wo.WinnerId;
            else if (next.Player2Id == null) next.Player2Id = wo.WinnerId;
        }
    }

    public async Task<List<TournamentMatchDto>> GetMatchesAsync(int tournamentId)
    {
        return await _db.TournamentMatches
            .Where(m => m.TournamentId == tournamentId)
            .Include(m => m.Player1).Include(m => m.Player2).Include(m => m.Winner)
            .OrderBy(m => m.Round).ThenBy(m => m.MatchNumber)
            .Select(m => new TournamentMatchDto(
                m.Id, m.TournamentId, m.Round, m.MatchNumber,
                m.Player1Id, m.Player1 != null ? m.Player1.FirstName + " " + m.Player1.LastName : null,
                m.Player2Id, m.Player2 != null ? m.Player2.FirstName + " " + m.Player2.LastName : null,
                m.WinnerId, m.Winner != null ? m.Winner.FirstName + " " + m.Winner.LastName : null,
                m.NextMatchId, m.Score,
                m.Player1Set1, m.Player2Set1, m.Player1Set2, m.Player2Set2,
                m.Player1Set3, m.Player2Set3,
                m.Status.ToString(), m.ScheduledAt, m.CourtName
            ))
            .ToListAsync();
    }

    public async Task<TournamentMatchDto?> UpdateMatchScoreAsync(int matchId, UpdateMatchScoreDto dto)
    {
        var match = await _db.TournamentMatches
            .Include(m => m.Player1).Include(m => m.Player2).Include(m => m.Winner)
            .FirstOrDefaultAsync(m => m.Id == matchId);

        if (match == null) return null;

        match.Player1Set1 = dto.Player1Set1;
        match.Player2Set1 = dto.Player2Set1;
        match.Player1Set2 = dto.Player1Set2;
        match.Player2Set2 = dto.Player2Set2;
        match.Player1Set3 = dto.Player1Set3;
        match.Player2Set3 = dto.Player2Set3;

        var scoreParts = new List<string>();
        scoreParts.Add($"{dto.Player1Set1}-{dto.Player2Set1}");
        scoreParts.Add($"{dto.Player1Set2}-{dto.Player2Set2}");
        if (dto.Player1Set3 > 0 || dto.Player2Set3 > 0)
            scoreParts.Add($"{dto.Player1Set3}-{dto.Player2Set3}");
        match.Score = string.Join(", ", scoreParts);

        if (dto.WinnerId.HasValue)
        {
            match.WinnerId = dto.WinnerId;
            match.Status = MatchStatus.Completed;

            if (match.NextMatchId.HasValue)
            {
                var nextMatch = await _db.TournamentMatches.FindAsync(match.NextMatchId.Value);
                if (nextMatch != null)
                {
                    if (nextMatch.Player1Id == null) nextMatch.Player1Id = dto.WinnerId;
                    else if (nextMatch.Player2Id == null) nextMatch.Player2Id = dto.WinnerId;
                }
            }
            else
            {
                var tournament = await _db.Tournaments
                    .Include(t => t.Participations)
                    .FirstOrDefaultAsync(t => t.Id == match.TournamentId);
                if (tournament != null)
                {
                    tournament.Status = TournamentStatus.Completed;
                    var winnerP = tournament.Participations.FirstOrDefault(p => p.PlayerId == dto.WinnerId);
                    if (winnerP != null) { winnerP.Result = "Winner"; winnerP.Placement = 1; }

                    var loserId = match.Player1Id == dto.WinnerId ? match.Player2Id : match.Player1Id;
                    var loserP = tournament.Participations.FirstOrDefault(p => p.PlayerId == loserId);
                    if (loserP != null) { loserP.Result = "Runner-up"; loserP.Placement = 2; }
                }
            }
        }
        else
        {
            match.Status = MatchStatus.InProgress;
        }

        await _db.SaveChangesAsync();

        return new TournamentMatchDto(
            match.Id, match.TournamentId, match.Round, match.MatchNumber,
            match.Player1Id, match.Player1 != null ? $"{match.Player1.FirstName} {match.Player1.LastName}" : null,
            match.Player2Id, match.Player2 != null ? $"{match.Player2.FirstName} {match.Player2.LastName}" : null,
            match.WinnerId, match.Winner != null ? $"{match.Winner.FirstName} {match.Winner.LastName}" : null,
            match.NextMatchId, match.Score,
            match.Player1Set1, match.Player2Set1, match.Player1Set2, match.Player2Set2,
            match.Player1Set3, match.Player2Set3,
            match.Status.ToString(), match.ScheduledAt, match.CourtName
        );
    }

    private static TournamentDto MapToDto(Tournament t) => new(
        t.Id, t.Name, t.Description, t.Location, t.StartDate, t.EndDate,
        t.MaxParticipants, t.Participations.Count, t.Category,
        t.Format, t.Surface, t.SetsToWin, t.DrawGenerated,
        t.Status.ToString(), t.CreatedAt
    );
}
