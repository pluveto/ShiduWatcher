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

        [HttpPost("program-usage-report")]
        public async Task<IActionResult> PostProgramUsageReport([FromBody] ProgramUsage usage)
        {
            try
            {
                await _usageService.AddUsage(usage);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("usage-report")]
        public async Task<IActionResult> GetUsageReport([FromQuery] long? start_time, [FromQuery] long? end_time)
        {
            try
            {
                DateTime startTime = start_time == null ? DateTime.MinValue : DateTimeOffset.FromUnixTimeSeconds((long)start_time).UtcDateTime;
                DateTime endTime = end_time == null ? DateTime.MaxValue : DateTimeOffset.FromUnixTimeSeconds((long)end_time).UtcDateTime;

                UsageReport<ProgramUsageSummary> report = await _usageService.GetUsageReportAsync(startTime, endTime);
                return Ok(report);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("webpage-usage-report")]
        public async Task<IActionResult> PostWebpageUsageReport([FromBody] WebpageUsage usage)
        {
            try
            {
                await _usageService.AddWebpageUsage(usage);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("webpage-usage-report")]
        public async Task<IActionResult> GetWebpageUsageReportAsync([FromQuery] long? start_time, [FromQuery] long? end_time)
        {
            try
            {
                DateTime startTime = start_time == null ? DateTime.MinValue : DateTimeOffset.FromUnixTimeSeconds((long)start_time).UtcDateTime;
                DateTime endTime = end_time == null ? DateTime.MaxValue : DateTimeOffset.FromUnixTimeSeconds((long)end_time).UtcDateTime;

                UsageReport<WebpageUsageSummary> report = await _usageService.GetWebpageUsagesAsync(startTime, endTime);
                return Ok(report);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

    }
}
