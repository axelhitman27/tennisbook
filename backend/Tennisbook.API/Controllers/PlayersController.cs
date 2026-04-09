using Microsoft.AspNetCore.Mvc;
using Tennisbook.API.DTOs;
using Tennisbook.API.Services;

namespace Tennisbook.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlayersController : ControllerBase
{
    private readonly PlayerService _playerService;

    public PlayersController(PlayerService playerService) => _playerService = playerService;

    [HttpGet]
    public async Task<ActionResult<List<PlayerSummaryDto>>> GetAll()
    {
        return await _playerService.GetAllAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PlayerDto>> GetById(int id)
    {
        var player = await _playerService.GetByIdAsync(id);
        return player == null ? NotFound() : Ok(player);
    }

    [HttpPost]
    public async Task<ActionResult<PlayerDto>> Create(CreatePlayerDto dto)
    {
        var player = await _playerService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = player.Id }, player);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<PlayerDto>> Update(int id, UpdatePlayerDto dto)
    {
        var player = await _playerService.UpdateAsync(id, dto);
        return player == null ? NotFound() : Ok(player);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        return await _playerService.DeleteAsync(id) ? NoContent() : NotFound();
    }

    [HttpGet("qr/{qrCode}")]
    public async Task<ActionResult<PlayerDto>> GetByQrCode(string qrCode)
    {
        var player = await _playerService.GetByQrCodeAsync(qrCode);
        return player == null ? NotFound() : Ok(player);
    }

    [HttpGet("{id}/qr-image")]
    public async Task<IActionResult> GetQrImage(int id)
    {
        var player = await _playerService.GetByIdAsync(id);
        if (player == null) return NotFound();

        var image = _playerService.GenerateQrImage(player.QrCode);
        return File(image, "image/png");
    }
}
