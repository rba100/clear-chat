﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;

// ReSharper disable UnusedMember.Global

namespace ClearChat.Hubs
{
    public class ChatHub : Hub
    {
        private static readonly List<MessageItem> s_ChatHistory = new List<MessageItem>();
        private static readonly List<Client> s_Clients = new List<Client>();

        public void Send(string message)
        {
            if (Context.User.Identity.IsAuthenticated)
            {
                var messageItem = new MessageItem(Context.User.Identity.Name, message, DateTime.UtcNow);

                Clients.All.newMessage(messageItem);

                lock (s_ChatHistory)
                {
                    s_ChatHistory.Add(messageItem);
                    if(s_ChatHistory.Count > 400) s_ChatHistory.RemoveAt(0);
                }
            }
        }

        public void GetHistory()
        {
            lock (s_ChatHistory)
            {
                var items = s_ChatHistory.ToArray();
                Clients.Caller.initHistory(items);
            }
        }

        public void GetClients()
        {
            lock (s_ChatHistory)
            {
                var items = s_Clients.ToArray();
                Clients.Caller.initClients(items);
            }
        }

        private void PushUpdateClients()
        {
            var items = s_Clients.ToArray();
            Clients.All.initClients(items);
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            var name = Context.User.Identity.Name;
            lock (s_Clients)
            {
                s_Clients.RemoveAll(c => c.Name == name);
                PushUpdateClients();
            }
            return base.OnDisconnected(stopCalled);
        }

        public override Task OnConnected()
        {
            var name = Context.User.Identity.Name;
            lock (s_Clients)
            {
                if (!s_Clients.Any(c => c.Name == name))
                {
                    s_Clients.Add(new Client(name));
                    PushUpdateClients();
                }
            }
            return base.OnConnected();
        }

        public override Task OnReconnected()
        {
            var name = Context.User.Identity.Name;
            lock (s_Clients)
            {
                if (!s_Clients.Any(c => c.Name == name))
                {
                    s_Clients.Add(new Client(name));
                    PushUpdateClients();
                }
            }
            return base.OnReconnected();
        }
    }

    internal class Client
    {
        public Client(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }

    public class MessageItem
    {
        public MessageItem(string name, string message, DateTime timeStamp)
        {
            Name = name;
            Message = message;
            TimeStamp = timeStamp;
        }

        public string Name { get; }
        public string Message { get; }
        public DateTime TimeStamp { get; }
    }
}