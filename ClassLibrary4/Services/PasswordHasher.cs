using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuthentificationService.Service.Interfaces;

namespace AuthentificationService.Service.Services
{
    public class PasswordHasher : IPasswordHasher
    {
        public string HashPassword(string password)
        {
            //BCrypt genere automatiquement un salt et l'inclut dans le hash retourné
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool VerifyPassword(string password, string hash)
        {
            //BCrypt extrait automatiquement le salt du hash et l'utilise pour vérifier le mot de passe
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
    }
}
