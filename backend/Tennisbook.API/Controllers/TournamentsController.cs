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
    {
        return await _tournamentService.GetAllAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TournamentDto>> GetById(int id)
    {
        var tournament = await _tournamentService.GetByIdAsync(id);
        return tournament == null ? NotFound() : Ok(tournament);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<TournamentDto>> Create(CreateTournamentDto dto)
    {
        var tournament = await _tournamentService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = tournament.Id }, tournament);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<TournamentDto>> Update(int id, UpdateTournamentDto dto)
    {
        var tournament = await _tournamentService.UpdateAsync(id, dto);
        return tournament == null ? NotFound() : Ok(tournament);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        return await _tournamentService.DeleteAsync(id) ? NoContent() : NotFound();
    }

    [HttpPost("register")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<TournamentParticipationDto>> RegisterPlayer(RegisterTournamentDto dto)
    {
        var result = await _tournamentService.RegisterPlayerAsync(dto);
        return result == null ? BadRequest("Registration failed.") : Ok(result);
    }

    [HttpGet("{id}/participants")]
    public async Task<ActionResult<List<TournamentParticipationDto>>> GetParticipants(int id)
    {
        return await _tournamentService.GetParticipantsAsync(id);
    }

    [HttpPut("participation/{participationId}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<TournamentParticipationDto>> UpdateResult(int participationId, UpdateParticipationResultDto dto)
    {
        var result = await _tournamentService.UpdateResultAsync(participationId, dto);
        return result == null ? NotFound() : Ok(result);
    }
}
