
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ClearChat.Core
{
    public class ConnectionManager : IConnectionManager
    {
        private readonly ConcurrentDictionary<string, List<string>> m_UserIdToConnection = new ConcurrentDictionary<string, List<string>>();
        private readonly ConcurrentDictionary<string, string> m_ConnectionToUserId = new ConcurrentDictionary<string, string>();
        private readonly ConcurrentDictionary<string, string> m_ConnectionToChannel = new ConcurrentDictionary<string, string>();

        public void RegisterConnection(string connectionId, string userId, string channelName)
        {
            m_ConnectionToUserId[connectionId] = userId;
            m_ConnectionToChannel[connectionId] = channelName;

            var connectionList = m_UserIdToConnection.GetOrAdd(userId, new List<string>());
            lock(connectionList) connectionList.Add(connectionId);
        }

        public void RegisterDisconnection(string connectionId)
        {
            var userId = m_ConnectionToChannel[connectionId];
            m_ConnectionToUserId.TryRemove(connectionId, out string _);
            m_ConnectionToChannel.TryRemove(connectionId, out string _);
            if(m_UserIdToConnection.ContainsKey(userId))
                m_UserIdToConnection[userId].Remove(connectionId);
        }

        public string GetChannelForConnection(string connectionId)
        {
            return m_ConnectionToChannel[connectionId];
        }

        public void ChangeConnectionChannel(string connectionId, string channel)
        {
            m_ConnectionToChannel[connectionId] = channel;
        }

        public IReadOnlyCollection<string> GetConnectionsForUser(string userId)
        {
            var connectionList = m_UserIdToConnection.GetOrAdd(userId, new List<string>());
            lock (connectionList) return connectionList.ToArray();
        }
    }
}