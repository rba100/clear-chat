using System.Collections.Generic;

namespace ClearChat.Core
{
    public interface IConnectionManager
    {
        void RegisterConnection(string connectionId, string userId);
        void RegisterDisconnection(string connectionId);
        IReadOnlyCollection<string> GetConnectionsForUser(string userId);
        IReadOnlyCollection<string> GetUsers();
    }
}