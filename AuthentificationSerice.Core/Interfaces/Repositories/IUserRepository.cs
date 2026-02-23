using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuthentificationService.DAL.Models;

namespace AuthentificationSerice.Core.Interfaces.Repositories
{
    public interface IUserRepository
    {
        // Methode pour user
         Task<User?> GetByIdAsync(Guid id);
        Task<User?> GetByEmailAsync(string email);
        Task<IEnumerable<User>> GetAllAsync();
        Task AddAsync(User user);
        Task UpdateAsync(User user);
        Task DeleteAsync(Guid id);
        Task<bool> EmailExistAsync(string email);

        // Methode pour RefreshToken
        Task<RefreshToken?> GetRefreshTokenAsync(string token);
        Task<IEnumerable<RefreshToken>> GetRefreshTokensByUserIdAsync(Guid userId);
        Task addRefreshTokenAsync(RefreshToken refreshToken);
        Task RevokeRefreshTokenAsync(string token);
        Task RevokeAllUserRefreshTokenAsync(Guid userId);
    }
}
