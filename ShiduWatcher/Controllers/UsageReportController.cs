using Microsoft.AspNetCore.Mvc;
using ShiduWatcher;
using ShiduWatcher.Types;
using System;
using System.Threading.Tasks;

namespace ShiduWatcher.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsageReportController : ControllerBase
    {
        private readonly ProgramUsageService _usageService;

        public UsageReportController(ProgramUsageService usageService)
        {
            _usageService = usageService;
        }

        [HttpGet("usage-report")]
        public async Task<IActionResult> GetUsageReport([FromQuery] long? start_time, [FromQuery] long? end_time)
        {
            try
            {
                DateTime startTime = start_time == null ? DateTime.MinValue : DateTimeOffset.FromUnixTimeSeconds((long)start_time).UtcDateTime;
                DateTime endTime = end_time == null ? DateTime.MaxValue : DateTimeOffset.FromUnixTimeSeconds((long)end_time).UtcDateTime;

                UsageReport report = await _usageService.GetUsageReportAsync(startTime, endTime);
                return Ok(report);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
