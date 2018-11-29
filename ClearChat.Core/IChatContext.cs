namespace ClearChat.Core
{
    public interface IChatContext
    {
        void SignalConnection(string connectionId, string method, params object[] arguments);
        void SignalChannel(string channelName, string method, params object[] arguments);
        void AddToGroup(string connectionId, string channelName);
        void RemoveFromGroup(string connectionId, string channelName);
    }
}