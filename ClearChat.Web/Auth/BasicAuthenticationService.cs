﻿using System.Linq;
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
            if (user.Length < 3) return Task.FromResult(false);

            var lowerCaseUser = user.ToLowerInvariant();

            if (m_BannedUserNames.Contains(lowerCaseUser))
            {
                return Task.FromResult(false);
            }

            if (m_UserRepository.UserIdExists(lowerCaseUser))
            {
                return Task.FromResult(m_UserRepository.ValidateUser(lowerCaseUser, password));
            }

            m_UserRepository.SaveUser(lowerCaseUser, password);
            return Task.FromResult(true);
        }
    }
}