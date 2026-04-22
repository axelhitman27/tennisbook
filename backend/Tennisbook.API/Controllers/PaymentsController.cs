using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tennisbook.API.DTOs;
using Tennisbook.API.Services;

namespace Tennisbook.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class PaymentsController : ControllerBase
{
    private readonly PaymentService _paymentService;

    public PaymentsController(PaymentService paymentService) => _paymentService = paymentService;

    [HttpPost]
    public async Task<ActionResult<PaymentDto>> Create(CreatePaymentDto dto)
    {
        var payment = await _paymentService.CreateAsync(dto);
        return Ok(payment);
    }

    [HttpPut("{id}/pay")]
    public async Task<ActionResult<PaymentDto>> MarkPaid(int id)
    {
        var payment = await _paymentService.MarkPaidAsync(id);
        return payment == null ? NotFound() : Ok(payment);
    }

    [HttpGet("player/{playerId}")]
    [Authorize]
    public async Task<ActionResult<List<PaymentDto>>> GetByPlayer(int playerId)
    {
        return await _paymentService.GetByPlayerAsync(playerId);
    }

    [HttpGet("pending")]
    public async Task<ActionResult<List<PaymentDto>>> GetPending()
    {
        return await _paymentService.GetPendingAsync();
    }
}
