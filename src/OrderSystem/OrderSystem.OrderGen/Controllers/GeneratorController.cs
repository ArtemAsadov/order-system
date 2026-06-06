using Microsoft.AspNetCore.Mvc;
using OrderSystem.OrderGen.Services;

namespace OrderSystem.OrderGen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GeneratorController : ControllerBase
    {
        private OrderGeneratorService _generator;

        public GeneratorController(OrderGeneratorService generator)
        {
            _generator = generator;
        }

        [HttpPost("start")]
        public IActionResult Start()
        {
            if (_generator.IsRunning)
                return BadRequest(new { error = "Generator already running" });

            _generator.Start();
            return Ok(new { running = true });
        }

        [HttpPost("stop")]
        public IActionResult Stop()
        {
            if (!_generator.IsRunning)
                return BadRequest(new { error = "Generator not running" });

            _generator.Stop();
            return Ok(new { running = false });
        }

        [HttpGet("status")]
        public IActionResult Status()
        {
            return Ok(new
            {
                running = _generator.IsRunning,
                totalSent = _generator.TotalOrdersSent,
            });
        }
    }
}
