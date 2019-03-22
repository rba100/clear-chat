using System;
using System.Linq;
using System.Threading.Tasks;
using ClearChat.Core;
using ClearChat.Core.Domain;
using ClearChat.Core.Repositories;

namespace ClearChat.Web.Auth
{
    class BasicAuthenticationService : IBasicAuthenticationService
    {
        private readonly IUserRepository m_UserRepository;

        private readonly string[] m_BannedUserNames = { "system", "admin", "administrator" };

        public BasicAuthenticationService(IUserRepository userRepository)
        {
            m_UserRepository = userRepository;
        }

        public Task<bool> IsValidUserAsync(string userName, string password)
        {
            var lowerCaseUserName = userName.Trim().ToLowerInvariant();

            if (m_BannedUserNames.Contains(lowerCaseUserName))
            {
                return Task.FromResult(false);
            }

            if (m_UserRepository.UserNameExists(userName))
            {
                return Task.FromResult(m_UserRepository.ValidateUser(userName, password));
            }

            m_UserRepository.SaveUser(new User(userName, null, false), password);
            return Task.FromResult(true);
        }
    }
}