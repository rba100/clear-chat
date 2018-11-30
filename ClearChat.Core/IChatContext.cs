﻿using System.Collections.Generic;
using System.Threading.Tasks;
using ClearChat.Core.Domain;

namespace ClearChat.Core
{
    public interface IChatContext
    {
        Task SignalConnection(string connectionId, string method, object argument);
        Task SignalConnection(string connectionId, string method, object argument1, object argument2);
        Task SignalChannel(string channelName, string method, params object[] arguments);
        void AddToGroup(string connectionId, string channelName);
        void RemoveFromGroup(string connectionId, string channelName);
        void SignalAll(string method, object argument);
    }
}