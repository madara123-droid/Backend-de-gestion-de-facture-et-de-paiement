using AuthentificationSerice.Core.DTOs;
using AuthentificationSerice.Core.Entitie;
using AuthentificationSerice.Core.Interfaces.Repositories;
using AuthentificationService.Service.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AuthentificationSerice.Core.DTOs.LoginDto;



namespace AuthentificationService.Service.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;// C'est une methode qui fournit une interface pour hasher et verifier les mots de passes
        private readonly ITokenService _tokenService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            ITokenService tokenService,
            ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
            _logger = logger;
        }

        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            try
            {
                // 1. Rechercher l'utilisateur par email
                var user = await _userRepository.GetByEmailAsync(request.Email);

                // 2. Vérifier si l'utilisateur existe et est actif
                if (user == null || user.IsActive != true)
                {
                    _logger.LogWarning("Tentative de connexion avec email inconnu: {Email}", request.Email);
                    return null;
                }

                // 3. Vérifier le mot de passe
                if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
                {
                    _logger.LogWarning("Mot de passe incorrect pour l'utilisateur: {Email}", request.Email);
                    return null;
                }

                // 4. Générer les tokens
                var accessToken = _tokenService.GenerateAccessToken(user);
                var refreshToken = _tokenService.GenerateRefreshToken();

                // 5. Sauvegarder le refresh token
                var refreshTokenEntity = new RefreshToken
                {
                    Token = refreshToken,
                    UserId = user.Id,
                    ExpiryDate = DateTime.UtcNow.AddDays(7),
                    IsRevoked = false
                };

                await _userRepository.addRefreshTokenAsync(refreshTokenEntity);

                // 6. Retourner la réponse
                return new LoginResponse
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    Role = user.Role,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(15)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la connexion pour {Email}", request.Email);
                throw;
            }
        }

        public async Task<bool> RegisterAsync(RegisterRequest request)
        {
            try
            {
                // 1. Vérifier si l'email existe déjà
                if (await _userRepository.EmailExistAsync(request.Email))
                {
                    _logger.LogWarning("Tentative d'inscription avec email existant: {Email}", request.Email);
                    return false;
                }

                // 2. Valider le rôle
                if (request.Role != "Admin" && request.Role != "Comptable")
                {
                    request.Role = "Comptable"; // Rôle par défaut
                }

                // 3. Créer le nouvel utilisateur
                var user = new User
                {
                    Id = Guid.NewGuid(),
                    Email = request.Email,
                    PasswordHash = _passwordHasher.HashPassword(request.Password),
                    Role = request.Role,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                // 4. Sauvegarder dans la base
                await _userRepository.AddAsync(user);

                _logger.LogInformation("Nouvel utilisateur créé: {Email} avec rôle {Role}", user.Email, user.Role);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'inscription pour {Email}", request.Email);
                throw;
            }
        }

        public async Task<LoginResponse?> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                // 1. Valider le format du refresh token
                if (!_tokenService.ValidateRefreshToken(refreshToken))
                {
                    _logger.LogWarning("Format de refresh token invalide");
                    return null;
                }

                // 2. Récupérer le refresh token depuis la base
                var existingToken = await _userRepository.GetRefreshTokenAsync(refreshToken);

                // 3. Vérifier s'il existe, n'est pas révoqué et n'est pas expiré
                if (existingToken == null ||
                    (existingToken.IsRevoked.HasValue && existingToken.IsRevoked.Value) ||
                    existingToken.ExpiryDate <= DateTime.UtcNow)
                {
                    _logger.LogWarning("Refresh token invalide, expiré ou révoqué");
                    return null;
                }

                // 4. Récupérer l'utilisateur associé
                var user = existingToken.User;
                if (user == null || user.IsActive != true )
                {
                    _logger.LogWarning("Utilisateur associé au refresh token introuvable ou inactif");
                    return null;
                }

                // 5. Générer un nouveau refresh token
                var newRefreshToken = _tokenService.GenerateRefreshToken();

                // 6. Révoquer l'ancien token et ajouter le nouveau
                existingToken.IsRevoked = true;
                await _userRepository.UpdateAsync(user); // Met à jour le token révoqué

                var newTokenEntity = new RefreshToken
                {
                    Token = newRefreshToken,
                    UserId = user.Id,
                    ExpiryDate = DateTime.UtcNow.AddDays(7),
                    IsRevoked = false
                };
                await _userRepository.addRefreshTokenAsync(newTokenEntity);

                // 7. Générer un nouvel access token
                var accessToken = _tokenService.GenerateAccessToken(user);

                // 8. Retourner la réponse
                return new LoginResponse
                {
                    AccessToken = accessToken,
                    RefreshToken = newRefreshToken,
                    Role = user.Role,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(15)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du rafraîchissement du token");
                throw;
            }
        }

        public async Task<bool> LogoutAsync(string refreshToken)
        {
            try
            {
                var token = await _userRepository.GetRefreshTokenAsync(refreshToken);
                if (token == null || (token.IsRevoked.HasValue && token.IsRevoked.Value))
                {
                    return false;
                }

                token.IsRevoked = true;
                await _userRepository.UpdateAsync(token.User); // Met à jour le token

                _logger.LogInformation("Déconnexion réussie pour l'utilisateur: {UserId}", token.UserId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la déconnexion");
                throw;
            }
        }

        public async Task<bool> RevokeUserTokensAsync(Guid userId)
        {
            try
            {
                await _userRepository.RevokeAllUserRefreshTokenAsync(userId);
                _logger.LogInformation("Tous les tokens révoqués pour l'utilisateur: {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la révocation des tokens pour l'utilisateur {UserId}", userId);
                throw;
            }
        }
    }
}
