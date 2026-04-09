using Microsoft.AspNetCore.Mvc;
using Tennisbook.API.DTOs;
using Tennisbook.API.Services;

namespace Tennisbook.API.Controllers;

[ApiController]
[Route("api/[controller]")]
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
    public async Task<ActionResult<TrainingSessionDto>> Create(CreateTrainingSessionDto dto)
    {
        var session = await _trainingService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = session.Id }, session);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TrainingSessionDto>> Update(int id, UpdateTrainingSessionDto dto)
    {
        var session = await _trainingService.UpdateAsync(id, dto);
        return session == null ? NotFound() : Ok(session);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        return await _trainingService.DeleteAsync(id) ? NoContent() : NotFound();
    }

    [HttpPost("{id}/check-in")]
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
}
