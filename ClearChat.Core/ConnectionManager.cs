using System;
using System.Collections.Generic;
using System.Text;

namespace ClearChat.Core
{
    public interface IConnectionManager
    {
        void RegisterConnection(string connectionId, string userId);
        void RegisterDisconnection(string connectionId, string userId);
        int CountUniqueUsers(string channelId);
        string GetChannelForConnection(string connectionId);
    }
}