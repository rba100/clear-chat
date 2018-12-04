﻿using System.Collections.Generic;

namespace ClearChat.Core.Domain
{
    public class ChannelInformation
    {
        public ChannelInformation(string name,
                                  bool isPrivate, 
                                  IReadOnlyCollection<string> members, 
                                  string messageOfTheDay)
        {
            Name = name;
            IsPrivate = isPrivate;
            Members = members;
            MessageOfTheDay = messageOfTheDay;
        }

        public string Name { get; }
        public bool IsPrivate { get; }
        public IReadOnlyCollection<string> Members { get; }
        public string MessageOfTheDay { get; }
    }
}