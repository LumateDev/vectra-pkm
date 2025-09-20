using Vectra.Modules.Identity.Application.Services;
using BC = BCrypt.Net.BCrypt;

namespace Vectra.Modules.Identity.Infrastructure.Security
{
    public class PasswordHasher : IPasswordHasher
    {
        private const int WorkFactor = 12;

        public string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be empty", nameof(password));

            return BC.HashPassword(password, WorkFactor);
        }

        public bool VerifyPassword(string password, string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;

            if (string.IsNullOrWhiteSpace(passwordHash))
                return false;

            try
            {
                return BC.Verify(password, passwordHash);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool NeedsRehash(string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(passwordHash))
                return true;

            try
            {
                var hashInfo = BC.InterrogateHash(passwordHash);

                if (int.TryParse(hashInfo.WorkFactor, out int currentWorkFactor))
                {
                    return currentWorkFactor < WorkFactor;
                }

                return true;
            }
            catch (Exception)
            {
                return true;
            }
        }
    }
}