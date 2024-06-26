using Microsoft.AspNetCore.Mvc;
using ShiduWatcher;

namespace ShiduWatcher.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ControlController : ControllerBase
    {
        private readonly ProgramUsageService _usageService;

        public ControlController(ProgramUsageService usageService)
        {
            _usageService = usageService;
        }

        [HttpPost("pause")]
        public IActionResult Pause()
        {
            _usageService.Pause();
            return Ok(new { message = "Usage tracking paused" });
        }

        [HttpPost("continue")]
        public IActionResult Continue()
        {
            _usageService.Resume();
            return Ok(new { message = "Usage tracking continued" });
        }

        [HttpGet("status")]
        public IActionResult Status()
        {
            var status = _usageService.IsPaused() ? "paused" : "running";
            return Ok(new { name = "ShiduWatcher", status, interval = _usageService.GetInterval() });
        }

        [HttpPost("set-interval")]
        public IActionResult SetInterval([FromBody] IntervalRequest request)
        {
            if (request.Interval < 100)
            {
                return BadRequest(new { message = "Interval must be greater than 100ms" });
            }

            _usageService.SetInterval(request.Interval);
            return Ok(new { message = "Interval updated", interval = request.Interval });
        }
    }

    public class IntervalRequest
    {
        public int Interval { get; set; }
    }
}
