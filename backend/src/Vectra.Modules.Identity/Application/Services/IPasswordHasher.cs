namespace Vectra.Modules.Identity.Application.Services
{
    public interface IPasswordHasher
    {
        /// <summary>
        /// Хеширует пароль используя безопасный алгоритм
        /// </summary>
        /// <param name="password">Открытый пароль</param>
        /// <returns>Хешированный пароль</returns>
        string HashPassword(string password);

        /// <summary>
        /// Проверяет соответствие пароля и хеша
        /// </summary>
        /// <param name="password">Открытый пароль для проверки</param>
        /// <param name="passwordHash">Сохраненный хеш пароля</param>
        /// <returns>true если пароль соответствует хешу</returns>
        bool VerifyPassword(string password, string passwordHash);

        /// <summary>
        /// Проверяет, нужно ли перехешировать пароль (например, при обновлении алгоритма)
        /// </summary>
        /// <param name="passwordHash">Текущий хеш пароля</param>
        /// <returns>true если требуется перехеширование</returns>
        bool NeedsRehash(string passwordHash);
    }
}
