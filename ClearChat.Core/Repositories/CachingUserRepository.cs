using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using ClearChat.Core.Domain;

namespace ClearChat.Core.Repositories
{
    public class CachingUserRepository : IUserRepository
    {
        private readonly IUserRepository m_UserRepository;
        private readonly IDictionary<string, User> m_Cache = new ConcurrentDictionary<string, User>();

        public CachingUserRepository(IUserRepository userRepository)
        {
            m_UserRepository = userRepository;
        }

        public User GetUserDetails(string userName)
        {
            if (m_Cache.TryGetValue(userName, out User val))
            {
                return val;
            }

            var user = m_UserRepository.GetUserDetails(userName);
            if (user == null) return null;
            m_Cache.TryAdd(userName, user);
            return user;
        }

        public User GetUserDetails(int userId)
        {
            var user = m_Cache.Values.FirstOrDefault(u => u.Id == userId);
            return user ?? m_UserRepository.GetUserDetails(userId);
        }

        public void SaveUser(User user, string password)
        {
            m_UserRepository.SaveUser(user, password);
        }

        public void UpdateUser(User user)
        {
            m_UserRepository.UpdateUser(user);
            m_Cache[user.UserName] = user;
        }

        public bool UserNameExists(string userName)
        {
            return m_Cache.ContainsKey(userName) || m_UserRepository.UserNameExists(userName);
        }

        public bool ValidateUser(string userName, string password)
        {
            return m_UserRepository.ValidateUser(userName, password);
        }
    }
}