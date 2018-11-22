using System.Collections.Generic;

namespace ClearChat.Core
{
    public interface IConnectionManager
    {
        void RegisterConnection(string connectionId, string userId, string channelName);
        void RegisterDisconnection(string connectionId);
        string GetChannelForConnection(string connectionId);
        void ChangeConnectionChannel(string connectionId, string channel);
        IReadOnlyCollection<string> GetConnectionsForUser(string userId);
    }
}