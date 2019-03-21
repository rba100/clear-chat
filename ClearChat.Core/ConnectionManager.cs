
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using ClearChat.Core.Domain;

namespace ClearChat.Core
{
    public class ConnectionManager : IConnectionManager
    {
        private readonly ConcurrentDictionary<string, List<string>> m_UserNameToConnection = new ConcurrentDictionary<string, List<string>>();
        private readonly ConcurrentDictionary<string, User> m_ConnectionToUser = new ConcurrentDictionary<string, User>();

        public void RegisterConnection(string connectionId, User user)
        {
            var connectionList = m_UserNameToConnection.GetOrAdd(user.UserName, new List<string>());
            lock (connectionList) connectionList.Add(connectionId);
            m_ConnectionToUser[connectionId] = user;
        }

        public void RegisterDisconnection(string connectionId)
        {
            if(!m_ConnectionToUser.Remove(connectionId, out User user)) return;
            if(m_UserNameToConnection.TryGetValue(user.UserName, out var connectionList))
            {
                lock (connectionList) connectionList.Remove(connectionId);
            }
        }

        public IReadOnlyCollection<string> GetConnectionsForUser(User user)
        {
            var connectionList = m_UserNameToConnection.GetOrAdd(user.UserName, new List<string>());
            lock (connectionList) return connectionList.ToArray();
        }

        public IReadOnlyCollection<User> GetUsers()
        {
            return m_ConnectionToUser.Select(kvp => kvp.Value).Distinct().ToArray();
        }

        public User GetUserForConnection(string connectionId)
        {
            return m_ConnectionToUser[connectionId];
        }
    }
}