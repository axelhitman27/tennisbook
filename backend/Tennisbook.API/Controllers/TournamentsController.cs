using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tennisbook.API.DTOs;
using Tennisbook.API.Services;

namespace Tennisbook.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TournamentsController : ControllerBase
{
    private readonly TournamentService _tournamentService;

    public TournamentsController(TournamentService tournamentService)
        => _tournamentService = tournamentService;

    [HttpGet]
    public async Task<ActionResult<List<TournamentDto>>> GetAll()
        => await _tournamentService.GetAllAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<TournamentDto>> GetById(int id)
    {
        var t = await _tournamentService.GetByIdAsync(id);
        return t == null ? NotFound() : Ok(t);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<TournamentDto>> Create(CreateTournamentDto dto)
    {
        var t = await _tournamentService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = t.Id }, t);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<TournamentDto>> Update(int id, UpdateTournamentDto dto)
    {
        var t = await _tournamentService.UpdateAsync(id, dto);
        return t == null ? NotFound() : Ok(t);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
        => await _tournamentService.DeleteAsync(id) ? NoContent() : NotFound();

    [HttpPost("register")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<TournamentParticipationDto>> RegisterPlayer(RegisterTournamentDto dto)
    {
        var r = await _tournamentService.RegisterPlayerAsync(dto);
        return r == null ? BadRequest("Registration failed.") : Ok(r);
    }

    [HttpGet("{id}/participants")]
    public async Task<ActionResult<List<TournamentParticipationDto>>> GetParticipants(int id)
        => await _tournamentService.GetParticipantsAsync(id);

    [HttpPut("participation/{participationId}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<TournamentParticipationDto>> UpdateResult(int participationId, UpdateParticipationResultDto dto)
    {
        var r = await _tournamentService.UpdateResultAsync(participationId, dto);
        return r == null ? NotFound() : Ok(r);
    }

    [HttpPost("{id}/generate-draw")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<TournamentMatchDto>>> GenerateDraw(int id)
        => await _tournamentService.GenerateDrawAsync(id);

    [HttpGet("{id}/matches")]
    public async Task<ActionResult<List<TournamentMatchDto>>> GetMatches(int id)
        => await _tournamentService.GetMatchesAsync(id);

    [HttpPut("matches/{matchId}/score")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<TournamentMatchDto>> UpdateMatchScore(int matchId, UpdateMatchScoreDto dto)
    {
        var m = await _tournamentService.UpdateMatchScoreAsync(matchId, dto);
        return m == null ? NotFound() : Ok(m);
    }
}
