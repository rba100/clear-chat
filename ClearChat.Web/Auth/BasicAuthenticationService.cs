using System.Linq;
using System.Threading.Tasks;
using ClearChat.Core.Repositories;

namespace ClearChat.Web.Auth
{
    class BasicAuthenticationService : IBasicAuthenticationService
    {
        private readonly IUserRepository m_UserRepository;

        private readonly string[] m_BannedUserNames = { "system" };

        public BasicAuthenticationService(IUserRepository userRepository)
        {
            m_UserRepository = userRepository;
        }

        public Task<bool> IsValidUserAsync(string user, string password)
        {
            var lowerCaseUser = user.ToLowerInvariant().Trim();

            if (lowerCaseUser.Length < 3) return Task.FromResult(false);

            if (m_BannedUserNames.Contains(lowerCaseUser))
            {
                return Task.FromResult(false);
            }

            if (m_UserRepository.UserIdExists(user))
            {
                return Task.FromResult(m_UserRepository.ValidateUser(user, password));
            }

            m_UserRepository.SaveUser(user, password);
            return Task.FromResult(true);
        }
    }
}