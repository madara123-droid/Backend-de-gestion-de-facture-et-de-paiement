using  AuthentificationSerice.Core.DTOs;
using AuthentificationService.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static AuthentificationSerice.Core.DTOs.LoginDto;





namespace AuthentificationService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]// ici va prendre le nom de la classe sans le suffixe "Controller" pour definir la route de base de ce controller, donc dans ce cas "api/auth"
    [Produces("application/json")]// cela indique que les réponses de ce controller seront au format JSON, ce qui est standard pour les API RESTful
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            this._authService = authService;
            this._logger = logger;
        }

            /// <summary>
            /// Authentifier un utilisateur et obtenir un token JWT
            /// </summary>
            /// <param name="request">Email et mot de passe</param>
            /// <returns>Token d'accès et refresh token</returns>
            /// <response code="200">Authentification réussie</response>
            /// <response code="401">Email ou mot de passe incorrect</response>
            /// <response code="500">Erreur interne du serveur</response>
            [HttpPost("login")]//cela indique que cette action répondra aux requêtes POST envoyées à "api/auth/login"
        [AllowAnonymous]
            [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
            [ProducesResponseType(StatusCodes.Status401Unauthorized)]
            [ProducesResponseType(StatusCodes.Status500InternalServerError)]//ici je personalise la réponse pour les erreurs internes du serveur, en indiquant que le code de statut HTTP sera 500 et que le corps de la réponse contiendra un message d'erreur générique
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);// verifie que les données envoyées dans la requête sont valides selon les règles de validation définies dans le modèle LoginRequest. Si les données ne sont pas valides, il retourne une réponse 400 Bad Request avec les détails des erreurs de validation.

            try
                {
                    var result = await _authService.LoginAsync(request);

                    if (result == null)
                        return Unauthorized(new { message = "Email ou mot de passe incorrect" });

                    return Ok(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erreur lors de la connexion pour {Email}", request.Email);
                    return StatusCode(500, new { message = "Une erreur est survenue lors de la connexion" });
                }
            }

            /// <summary>
            /// Créer un nouvel utilisateur (réservé aux administrateurs)
            /// </summary>
            /// <param name="request">Email, mot de passe et rôle</param>
            /// <returns>Confirmation de création</returns>
            /// <response code="200">Utilisateur créé avec succès</response>
            /// <response code="400">Email déjà utilisé ou données invalides</response>
            /// <response code="401">Non authentifié</response>
            /// <response code="403">Pas les droits administrateur</response>
            [HttpPost("register")]
            [Authorize(Roles = "Admin")]
            [ProducesResponseType(StatusCodes.Status200OK)]
            [ProducesResponseType(StatusCodes.Status400BadRequest)]
            [ProducesResponseType(StatusCodes.Status401Unauthorized)]
            [ProducesResponseType(StatusCodes.Status403Forbidden)]
            public async Task<IActionResult> Register([FromBody] RegisterRequest request)
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                try
                {
                    var result = await _authService.RegisterAsync(request);

                    if (!result)
                        return BadRequest(new { message = "L'email est déjà utilisé" });

                    return Ok(new { message = "Utilisateur créé avec succès" });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erreur lors de l'inscription pour {Email}", request.Email);
                    return StatusCode(500, new { message = "Une erreur est survenue lors de l'inscription" });
                }
            }

            /// <summary>
            /// Rafraîchir un token d'accès expiré
            /// </summary>
            /// <param name="refreshToken">Token de rafraîchissement</param>
            /// <returns>Nouveau token d'accès et nouveau refresh token</returns>
            /// <response code="200">Rafraîchissement réussi</response>
            /// <response code="400">Token invalide</response>
            [HttpPost("refresh")]
            [AllowAnonymous]
            [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
            [ProducesResponseType(StatusCodes.Status400BadRequest)]
            public async Task<IActionResult> Refresh([FromBody] string refreshToken)
            {
                if (string.IsNullOrWhiteSpace(refreshToken))
                    return BadRequest(new { message = "Token de rafraîchissement requis" });

                try
                {
                    var result = await _authService.RefreshTokenAsync(refreshToken);

                    if (result == null)
                        return BadRequest(new { message = "Token de rafraîchissement invalide ou expiré" });

                    return Ok(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erreur lors du rafraîchissement du token");
                    return StatusCode(500, new { message = "Une erreur est survenue" });
                }
            }

            /// <summary>
            /// Déconnexion (invalide le refresh token)
            /// </summary>
            /// <param name="refreshToken">Token de rafraîchissement à révoquer</param>
            /// <returns>Confirmation de déconnexion</returns>
            [HttpPost("logout")]
            [Authorize]
            [ProducesResponseType(StatusCodes.Status200OK)]
            [ProducesResponseType(StatusCodes.Status400BadRequest)]
            public async Task<IActionResult> Logout([FromBody] string refreshToken)
            {
                if (string.IsNullOrWhiteSpace(refreshToken))
                    return BadRequest(new { message = "Token de rafraîchissement requis" });

                try
                {
                    var result = await _authService.LogoutAsync(refreshToken);

                    if (!result)
                        return BadRequest(new { message = "Token invalide ou déjà révoqué" });

                    return Ok(new { message = "Déconnexion réussie" });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erreur lors de la déconnexion");
                    return StatusCode(500, new { message = "Une erreur est survenue" });
                }
            }

            /// <summary>
            /// Obtenir les informations de l'utilisateur connecté
            /// </summary>
            /// <returns>Informations sur l'utilisateur</returns>
            [HttpGet("me")]
            [Authorize]
            [ProducesResponseType(StatusCodes.Status200OK)]
            [ProducesResponseType(StatusCodes.Status401Unauthorized)]
            public IActionResult GetCurrentUser()
            {
                var userId = User.FindFirst("userId")?.Value;
                var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

                return Ok(new
                {
                    UserId = userId,
                    Email = email,
                    Role = role
                });
            }
        }
    }

