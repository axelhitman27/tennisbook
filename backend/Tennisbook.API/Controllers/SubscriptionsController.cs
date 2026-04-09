using Microsoft.AspNetCore.Mvc;
using Tennisbook.API.DTOs;
using Tennisbook.API.Services;

namespace Tennisbook.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubscriptionsController : ControllerBase
{
    private readonly SubscriptionService _subscriptionService;

    public SubscriptionsController(SubscriptionService subscriptionService)
        => _subscriptionService = subscriptionService;

    [HttpGet]
    public async Task<ActionResult<List<SubscriptionDto>>> GetAll()
    {
        return await _subscriptionService.GetAllAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SubscriptionDto>> GetById(int id)
    {
        var sub = await _subscriptionService.GetByIdAsync(id);
        return sub == null ? NotFound() : Ok(sub);
    }

    [HttpGet("player/{playerId}")]
    public async Task<ActionResult<List<SubscriptionDto>>> GetByPlayer(int playerId)
    {
        return await _subscriptionService.GetByPlayerIdAsync(playerId);
    }

    [HttpPost]
    public async Task<ActionResult<SubscriptionDto>> Create(CreateSubscriptionDto dto)
    {
        var sub = await _subscriptionService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = sub.Id }, sub);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<SubscriptionDto>> Update(int id, UpdateSubscriptionDto dto)
    {
        var sub = await _subscriptionService.UpdateAsync(id, dto);
        return sub == null ? NotFound() : Ok(sub);
    }
}
