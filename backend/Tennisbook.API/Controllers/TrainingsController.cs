using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tennisbook.API.DTOs;
using Tennisbook.API.Services;

namespace Tennisbook.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TrainingsController : ControllerBase
{
    private readonly TrainingService _trainingService;

    public TrainingsController(TrainingService trainingService)
        => _trainingService = trainingService;

    [HttpGet]
    public async Task<ActionResult<List<TrainingSessionDto>>> GetAll()
    {
        return await _trainingService.GetAllAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TrainingSessionDto>> GetById(int id)
    {
        var session = await _trainingService.GetByIdAsync(id);
        return session == null ? NotFound() : Ok(session);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<TrainingSessionDto>> Create(CreateTrainingSessionDto dto)
    {
        var session = await _trainingService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = session.Id }, session);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<TrainingSessionDto>> Update(int id, UpdateTrainingSessionDto dto)
    {
        var session = await _trainingService.UpdateAsync(id, dto);
        return session == null ? NotFound() : Ok(session);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        return await _trainingService.DeleteAsync(id) ? NoContent() : NotFound();
    }

    [HttpPost("{id}/check-in")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<QrScanResultDto>> CheckIn(int id, [FromQuery] string qrCode)
    {
        var result = await _trainingService.CheckInByQrAsync(qrCode, id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{id}/attendances")]
    public async Task<ActionResult<List<TrainingAttendanceDto>>> GetAttendances(int id)
    {
        return await _trainingService.GetAttendancesAsync(id);
    }

    // --- Enrollment ---

    [HttpPost("{id}/enroll")]
    public async Task<ActionResult<TrainingEnrollmentDto>> Enroll(int id)
    {
        var playerIdClaim = User.FindFirst("PlayerId")?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;

        if (playerIdClaim == null && role != "Admin")
            return Forbid();

        var playerId = int.Parse(playerIdClaim!);
        var result = await _trainingService.EnrollAsync(new EnrollPlayerDto(playerId, id));
        return result == null
            ? BadRequest(new { message = "Enrollment failed. Session full or already enrolled." })
            : Ok(result);
    }

    [HttpPost("{id}/enroll-player")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<TrainingEnrollmentDto>> EnrollPlayer(int id, [FromBody] EnrollPlayerDto dto)
    {
        var result = await _trainingService.EnrollAsync(new EnrollPlayerDto(dto.PlayerId, id));
        return result == null
            ? BadRequest(new { message = "Enrollment failed. Session full or already enrolled." })
            : Ok(result);
    }

    [HttpDelete("enrollment/{enrollmentId}")]
    public async Task<IActionResult> Unenroll(int enrollmentId)
    {
        return await _trainingService.UnenrollAsync(enrollmentId) ? NoContent() : NotFound();
    }

    [HttpGet("{id}/enrollments")]
    public async Task<ActionResult<List<TrainingEnrollmentDto>>> GetEnrollments(int id)
    {
        return await _trainingService.GetEnrollmentsAsync(id);
    }

    [HttpGet("my-enrollments")]
    public async Task<ActionResult<List<TrainingEnrollmentDto>>> GetMyEnrollments()
    {
        var playerIdClaim = User.FindFirst("PlayerId")?.Value;
        if (playerIdClaim == null) return Forbid();
        return await _trainingService.GetPlayerEnrollmentsAsync(int.Parse(playerIdClaim));
    }
}
