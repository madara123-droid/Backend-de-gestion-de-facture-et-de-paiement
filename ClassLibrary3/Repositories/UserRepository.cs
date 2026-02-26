using Microsoft.EntityFrameworkCore;
using AuthentificationSerice.Core;
using AuthentificationSerice.Core.Interfaces.Repositories;
using AuthentificationService.DAL.Models;
using AuthentificationSerice.Core.Entitie;


namespace AuthentificationService.DAL.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AuthDbContext _authDbContext;

        public UserRepository(AuthDbContext authDbContext)
        {
            _authDbContext = authDbContext;
        }

        // ==== Methode pour User ====

        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await _authDbContext.Users.Include(u => u.RefreshTokens).FirstOrDefaultAsync(u => u.Id == id);

        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _authDbContext.Users.Include(u => u.RefreshTokens).FirstOrDefaultAsync(u => u.Email == email);

        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _authDbContext.Users.Include(u => u.RefreshTokens).ToListAsync();
            // ".ToListAsync" est une methode qui transforme le resulta attendtu en list executer de maniere asynchrone 

        }

        public async Task AddAsync(User user)
        {
            await _authDbContext.Users.AddAsync(user);
            await _authDbContext.SaveChangesAsync();// cette ligne permet d'envoyer les modifications a la base de donnees 
        }

        public async Task UpdateAsync(User user)
        {
            _authDbContext.Users.Update(user);
            await _authDbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var users = await GetByIdAsync(id);
            if (users != null)
            {
                _authDbContext.Users.Remove(users);
                await _authDbContext.SaveChangesAsync();
            }
        }

        public async Task<bool> EmailExistAsync(string email)
        {
            return await _authDbContext.Users.AnyAsync(u => u.Email == email);// retourne un boollean
        }





        // ===== Methode pour Refreshtoken ======


        public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
        {
            return await _authDbContext.RefreshTokens.Include(rt => rt.User).FirstOrDefaultAsync(rt => rt.Token == token);
        }

    public async Task<IEnumerable<RefreshToken>> GetRefreshTokensByUserIdAsync(Guid userId)
        {
            return await _authDbContext.RefreshTokens.Where(rt=>rt.UserId == userId).ToListAsync();
        }

    public async Task addRefreshTokenAsync(RefreshToken refreshToken)
        {
            await _authDbContext.RefreshTokens.AddAsync(refreshToken);
            await _authDbContext.SaveChangesAsync();

        }

    public async Task RevokeRefreshTokenAsync(string token)
        {
            var refreshToken = await GetRefreshTokenAsync(token);
            if (refreshToken != null) { 
            refreshToken.IsRevoked = true;
            _authDbContext.RefreshTokens.Update(refreshToken);
            await _authDbContext.SaveChangesAsync();
            
            }

        }
        public async Task RevokeAllUserRefreshTokenAsync(Guid userId)
        {
            var tokens = await GetRefreshTokensByUserIdAsync(userId);
            foreach (var token in tokens)
            {
                token.IsRevoked = true;
            }
            _authDbContext.RefreshTokens.UpdateRange(tokens);
            await _authDbContext.SaveChangesAsync();
        }
    }









}
