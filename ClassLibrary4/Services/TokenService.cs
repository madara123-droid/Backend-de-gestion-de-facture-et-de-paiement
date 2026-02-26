using AuthentificationSerice.Core.Entitie;
using AuthentificationService;
using AuthentificationService.Service.Interfaces;
using Microsoft.Extensions.Configuration;// te permet d'acceder a tes configurations via un objet
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
/*
 * Service de gestion des tokens JWT et des refresh tokens
 * - Génération de JWT avec les claims nécessaires (userId, email, role)
 * - Génération de refresh tokens sécurisés
 * - Validation des JWT (signature, expiration, etc.)
 * - Validation basique des refresh tokens (la validation complète se fait dans AuthService)
 */
using System.Linq;
using System.Security.Claims;
/*
 * Note : La validation complète des refresh tokens (expiration, révocation) doit être gérée dans AuthService
 * car elle nécessite l'accès à la base de données pour vérifier que le refresh token est toujours valide.
 */
using System.Security.Cryptography;
using System.Text;
using System.Text;
using System.Threading.Tasks;

namespace AuthentificationService.Service.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly string _secret;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _accessTokenExpirationMinutes;


        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
            _secret = _configuration["JwtSettings:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");
            _issuer = _configuration["JwtSettings:Issuer"] ?? throw new InvalidOperationException("JWT Issuer not configured");
            _audience = _configuration["JwtSettings:Audience"] ?? throw new InvalidOperationException("JWT Audience not configured");
            _accessTokenExpirationMinutes = int.Parse(_configuration["JwtSettings:AccessTokenExpirationMinutes"] ?? "15");
        }
        public string GenerateAccessToken(User user)
            {
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));// encoding... convertit ta cle secret en tableau de byte car les algorithme de cruypto travaillent sur des binaire, pas sur des chaine 
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);//ici j'associe la key a un algorithme de hashage

                var claims = new[]
                {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("userId", user.Id.ToString())
            };

                var token = new JwtSecurityToken(
                    issuer: _issuer,
                    audience: _audience,
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(_accessTokenExpirationMinutes),
                    signingCredentials: credentials
                );

                return new JwtSecurityTokenHandler().WriteToken(token);//prend les differents elements creer , les easemblage dns un format Base64url
            }

            public string GenerateRefreshToken()
            {
                var randomNumber = new byte[64];// je creer un tableau de 64 octets qui me servir a stocker des donnes generer
                using var rng = RandomNumberGenerator.Create();//ici j'estancie un generator 
            /*
             * RandomNumberGenerator est une classe derive de system.Security.Cryptography qui genere des nombre aleatoire  cryptographique securise
             * using var garantit que l’objet sera correctement libéré après utilisation.

             */
            rng.GetBytes(randomNumber);// remplit le tableau avec des octet aleatoire , chaqu ex
                return Convert.ToBase64String(randomNumber);// convertit le tableau en une chaine encodee en base64 
            }

            public ClaimsPrincipal? ValidateAccessToken(string token)
            {
                try
                {
                    var tokenHandler = new JwtSecurityTokenHandler();//intancie cette fois ci un objet qui va servire a lire et valider les token 
                    var key = Encoding.UTF8.GetBytes(_secret);//convertit la cle en byte pour la validation de la signature

                var parameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,//je definie les regles de validation

                        ValidIssuer = _issuer,
                        ValidAudience = _audience,
                        IssuerSigningKey = new SymmetricSecurityKey(key),//la cle de symetrie utilise pour validr la signature 
                        ClockSkew = TimeSpan.Zero //pas de de tolerance sur l'expiration par defaut , il y'a 5 min de marge 
                    };

                    var principal = tokenHandler.ValidateToken(token, parameters, out _);//si la validation reussi, il retourne un objet ClaimsPrincipal qui contient les claims du token (userId, email, role, etc.)
                return principal;
                }
                catch
                {
                    return null;
                }
            }

            public bool ValidateRefreshToken(string refreshToken)
            {
                // La validation de base : juste vérifier que ce n'est pas vide
                // La validation complète (expiration, révocation) sera faite dans AuthService
                return !string.IsNullOrEmpty(refreshToken) && refreshToken.Length >= 32;
            }
        }
    }
