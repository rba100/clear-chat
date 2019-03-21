using System.Collections.Generic;
using ClearChat.Core.Domain;

namespace ClearChat.Core
{
    public interface IConnectionManager
    {
        void RegisterConnection(string connectionId, User user);
        void RegisterDisconnection(string connectionId);
        IReadOnlyCollection<string> GetConnectionsForUser(User user);
        IReadOnlyCollection<User> GetUsers();
        User GetUserForConnection(string connectionId);
    }
}