using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tennisbook.API.DTOs;
using Tennisbook.API.Services;

namespace Tennisbook.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly ReportService _reportService;

    public ReportsController(ReportService reportService) => _reportService = reportService;

    [HttpGet("player/{playerId}")]
    public async Task<ActionResult<PlayerReportDto>> GetPlayerReport(int playerId)
    {
        var report = await _reportService.GetPlayerReportAsync(playerId);
        return report == null ? NotFound() : Ok(report);
    }

    [HttpGet("dashboard")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<DashboardDto>> GetDashboard()
    {
        return await _reportService.GetDashboardAsync();
    }
}
