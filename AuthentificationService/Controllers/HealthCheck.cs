// AuthenticationService.API/Controllers/HealthController.cs
using AuthentificationService.DAL;
using AuthentificationService.DAL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly AuthDbContext _context;
        private readonly ILogger<HealthController> _logger;

        public HealthController(AuthDbContext context, ILogger<HealthController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Vérifier que l'API et la base de données fonctionnent
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> CheckHealth()
        {
            var canConnect = await _context.Database.CanConnectAsync();
            return Ok(new { connected = canConnect, message = canConnect ? "Connexion DB OK" : "Connexion DB échouée" });
            // "?"+:" signifie si la variable est true c'est l'un sinon c'est l'autre 
        }
    }
}