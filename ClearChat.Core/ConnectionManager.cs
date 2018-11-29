
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace ClearChat.Core
{
    public class ConnectionManager : IConnectionManager
    {
        private readonly ConcurrentDictionary<string, List<string>> m_UserIdToConnection = new ConcurrentDictionary<string, List<string>>();
        private readonly ConcurrentDictionary<string, string> m_ConnectionToUserId = new ConcurrentDictionary<string, string>();

        public void RegisterConnection(string connectionId, string userId)
        {
            var connectionList = m_UserIdToConnection.GetOrAdd(userId, new List<string>());
            lock (connectionList) connectionList.Add(connectionId);
            m_ConnectionToUserId[connectionId] = userId;
        }

        public void RegisterDisconnection(string connectionId)
        {
            m_ConnectionToUserId.Remove(connectionId, out string userId);
            if(m_UserIdToConnection.TryGetValue(userId, out var connectionList))
            {
                lock (connectionList) connectionList.Remove(connectionId);
            }
        }

        public IReadOnlyCollection<string> GetConnectionsForUser(string userId)
        {
            var connectionList = m_UserIdToConnection.GetOrAdd(userId, new List<string>());
            lock (connectionList) return connectionList.ToArray();
        }

        public IReadOnlyCollection<string> GetUsers()
        {
            return m_ConnectionToUserId.Select(kvp => kvp.Value).Distinct().ToArray();
        }

        public string GetUserIdForConnection(string connectionId)
        {
            return m_ConnectionToUserId[connectionId];
        }
    }
}