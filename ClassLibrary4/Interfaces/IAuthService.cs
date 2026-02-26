using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AuthentificationSerice.Core.DTOs.LoginDto;

namespace AuthentificationService.Service.Interfaces
{
    public interface IAuthService
    {
        // Authentification
        Task<LoginResponse?> LoginAsync(LoginRequest request);

        // Inscription (réservé aux admins)
        Task<bool> RegisterAsync(RegisterRequest request);

        // Rafraîchissement de token
        Task<LoginResponse?> RefreshTokenAsync(string refreshToken);

        // Déconnexion
        Task<bool> LogoutAsync(string refreshToken);

        // Révocation de tous les tokens d'un utilisateur
        Task<bool> RevokeUserTokensAsync(Guid userId);
    }
}
