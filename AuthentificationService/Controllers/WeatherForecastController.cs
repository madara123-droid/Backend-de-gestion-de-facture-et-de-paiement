using Microsoft.AspNetCore.Mvc;
using AuthentificationService.DAL;
using Microsoft.EntityFrameworkCore;
using AuthentificationService.DAL.Models;

namespace AuthentificationService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly AuthDbContext _context;

        public WeatherForecastController(AuthDbContext context)
        {
            _context = context;
        }

        [HttpGet("test-db")]
        public async Task<IActionResult> TestDb()
        {
            var canConnect = await _context.Database.CanConnectAsync();
            return Ok( new {connected = canConnect, message = canConnect ? "Connexion DB OK" : "Connexion DB échouée" });
        // "?"+:" signifie si la variable est true c'est l'un sinon c'est l'autre 
        }

        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}
