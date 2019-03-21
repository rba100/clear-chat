
using System.Collections.Concurrent;
using System.Collections.Generic;

using ClearChat.Core.Domain;

namespace ClearChat.Core.Repositories
{
    public class CachingUserRepository : IUserRepository
    {
        private readonly IUserRepository m_UserRepository;
        private readonly IDictionary<string, User> m_NameCache = new ConcurrentDictionary<string, User>();
        private readonly IDictionary<int, User> m_IdCache = new ConcurrentDictionary<int, User>();

        public CachingUserRepository(IUserRepository userRepository)
        {
            m_UserRepository = userRepository;
        }

        public User GetUserDetails(string userName)
        {
            if (m_NameCache.TryGetValue(userName, out User val))
            {
                return val;
            }

            var user = m_UserRepository.GetUserDetails(userName);
            if (user == null) return null;
            m_NameCache.TryAdd(user.UserName, user);
            m_IdCache.TryAdd(user.Id, user);
            return user;
        }

        public User GetUserDetails(int userId)
        {
            if (m_IdCache.TryGetValue(userId, out User val))
            {
                return val;
            }

            var user = m_UserRepository.GetUserDetails(userId);
            if (user == null) return null;
            m_NameCache.TryAdd(user.UserName, user);
            m_IdCache.TryAdd(user.Id, user);
            return user;
        }

        public void SaveUser(User user, string password)
        {
            m_UserRepository.SaveUser(user, password);
            m_IdCache.Add(user.Id, user);
            m_NameCache.Add(user.UserName, user);
        }

        public void UpdateUser(User user)
        {
            m_UserRepository.UpdateUser(user);
            m_NameCache[user.UserName] = user;
            m_IdCache[user.Id] = user;
        }

        public bool UserNameExists(string userName)
        {
            return m_NameCache.ContainsKey(userName) || m_UserRepository.UserNameExists(userName);
        }

        public bool ValidateUser(string userName, string password)
        {
            return m_UserRepository.ValidateUser(userName, password);
        }
    }
}