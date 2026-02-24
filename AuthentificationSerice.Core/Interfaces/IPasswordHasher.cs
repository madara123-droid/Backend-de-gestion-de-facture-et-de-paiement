namespace AuthenticationService.Service.Interfaces
{
    public interface IPasswordHasher
    {
        /// <summary>
        /// Hache un mot de passe en clair.
        /// </summary>
        /// <param name="password">Mot de passe en clair</param>
        /// <returns>Mot de passe haché</returns>
        string HashPassword(string password);

        /// <summary>
        /// Vérifie si un mot de passe en clair correspond au haché.
        /// </summary>
        /// <param name="password">Mot de passe en clair</param>
        /// <param name="hash">Mot de passe haché</param>
        /// <returns>True si correspond, sinon false</returns>
        bool VerifyPassword(string password, string hash);
    }
}
