using System.Collections.Concurrent;
using System.Collections.Generic;
using ClearChat.Core.Domain;

namespace ClearChat.Core.Repositories
{
    public interface IUserRepository
    {
        bool UserIdExists(string userId);
        User GetUserDetails(string userId);
        void SaveUser(string userId, string password);
        void UpdateUser(User user);
        bool ValidateUser(string userId, string password);
    }

    public class CachingUserRepository : IUserRepository
    {
        private readonly IUserRepository m_UserRepository;
        private readonly IDictionary<string, User> m_Cache = new ConcurrentDictionary<string, User>();

        public CachingUserRepository(IUserRepository userRepository)
        {
            m_UserRepository = userRepository;
        }

        public User GetUserDetails(string userId)
        {
            if (m_Cache.TryGetValue(userId, out User val))
            {
                return val;
            }

            var user = m_UserRepository.GetUserDetails(userId);
            if (user == null) return null;
            m_Cache.TryAdd(userId, user);
            return user;
        }

        public void SaveUser(string userId, string password)
        {
            m_UserRepository.SaveUser(userId, password);
        }

        public void UpdateUser(User user)
        {
            m_UserRepository.UpdateUser(user);
            m_Cache[user.UserId] = user;
        }

        public bool UserIdExists(string userId)
        {
            return m_Cache.ContainsKey(userId) || m_UserRepository.UserIdExists(userId);
        }

        public bool ValidateUser(string userId, string password)
        {
            return m_UserRepository.ValidateUser(userId, password);
        }
    }
}