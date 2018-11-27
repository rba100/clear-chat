
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

using ClearChat.Core.Crypto;
using ClearChat.Core.Domain;
using ClearChat.Core.Repositories.Bindings;

namespace ClearChat.Core.Repositories
{
    public class SqlServerMessageRepository : IMessageRepository
    {
        private readonly string m_ConnectionString;
        private IColourGenerator m_ColourGenerator = new ColourGenerator();

        private readonly IStringProtector m_StringProtector;
        private readonly IStringHasher m_StringHasher;

        public SqlServerMessageRepository(string connectionString,
                                          IStringProtector stringProtector,
                                          IStringHasher stringHasher)
        {
            m_ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            m_StringProtector = stringProtector ?? throw new ArgumentNullException(nameof(stringProtector));
            m_StringHasher = stringHasher ?? throw new ArgumentNullException(nameof(stringHasher));
        }

        public IReadOnlyCollection<ChatMessage> ChannelMessages(string channelName)
        {
            var isDefaultChannel = channelName == "default";
            var channelNameHash = isDefaultChannel ? null : m_StringHasher.Hash(channelName);

            using (var db = new DatabaseContext(m_ConnectionString))
            {
                var channel = db.Channels.SingleOrDefault(c => c.ChannelNameHash == channelNameHash);
                var channelId = isDefaultChannel ? 0 : channel?.ChannelId ?? -1;

                if (!isDefaultChannel && channel == null) return new ChatMessage[0];

                var msgs = db.Messages
                             .Where(m => m.ChannelId == channelId)
                             .OrderByDescending(m => m.TimeStampUtc)
                             .Take(400)
                             .ToArray()
                             .Select(m => FromBinding(m, channelName))
                             .ToArray();
                return msgs;
            }
        }

        private ChatMessage FromBinding(MessageBinding arg, string channelName)
        {
            var userId = m_StringProtector.Unprotect(Convert.FromBase64String(arg.UserId));

            return new ChatMessage(m_StringProtector.Unprotect(Convert.FromBase64String(arg.UserId)),
                                   channelName,
                                   m_StringProtector.Unprotect(arg.Message),
                                   m_ColourGenerator.GenerateFromString(userId),
                                   DateTime.SpecifyKind(arg.TimeStampUtc, DateTimeKind.Utc));
        }

        public void WriteMessage(ChatMessage message)
        {
            var isDefaultChannel = message.ChannelName == "default";
            var channelNameHash = isDefaultChannel ? null : m_StringHasher.Hash(message.ChannelName);
            using (var db = new DatabaseContext(m_ConnectionString))
            {
                var channel = db.Channels.SingleOrDefault(c => c.ChannelNameHash == channelNameHash);
                var channelId = isDefaultChannel ? 0 : channel.ChannelId;
                var messageBinding = new MessageBinding
                {
                    UserId = Convert.ToBase64String(m_StringProtector.Protect(message.UserId)),
                    ChannelId = channelId,
                    Message = m_StringProtector.Protect(message.Message),
                    TimeStampUtc = message.TimeStampUtc
                };
                db.Messages.Add(messageBinding);
                db.SaveChanges();
            }
        }

        public void ClearChannel(string channelName)
        {
            var isDefaultChannel = channelName == "default";
            var channelNameHash = m_StringHasher.Hash(channelName);

            using (var db = new DatabaseContext(m_ConnectionString))
            {
                var channel = db.Channels.SingleOrDefault(c => c.ChannelNameHash == channelNameHash);
                var channelId = isDefaultChannel ? 0 : channel.ChannelId;
                var messagesToRemove = db.Messages.Where(m => m.ChannelId == channelId);
                db.Messages.RemoveRange(messagesToRemove);
                db.SaveChanges();
            }
        }

        public SwitchChannelResult GetOrCreateChannel(string channelName, string channelPassword)
        {
            if (channelName == "default") return SwitchChannelResult.Accepted;

            var channelNameHash = m_StringHasher.Hash(channelName);

            using (var db = new DatabaseContext(m_ConnectionString))
            {
                var channel = db.Channels.SingleOrDefault(c => c.ChannelNameHash == channelNameHash);
                if (channel == null)
                {
                    var salt = Guid.NewGuid().ToByteArray();
                    channel = new ChannelBinding
                    {
                        ChannelNameHash = channelNameHash,
                        PasswordHash = m_StringHasher.Hash(channelPassword, salt),
                        PasswordSalt = salt
                    };
                    db.Channels.Add(channel);
                    db.SaveChanges();
                    return SwitchChannelResult.Created;
                }
                else
                {
                    if (!m_StringHasher.HashMatch(channelPassword, channel.PasswordHash, channel.PasswordSalt))
                    {
                        return SwitchChannelResult.Denied;
                    }
                }
                return SwitchChannelResult.Accepted;
            }
        }

        public void AddChannelMembership(string userId, string channelName)
        {
            var userIdHash = m_StringHasher.Hash(userId);

            using (var db = new DatabaseContext(m_ConnectionString))
            {
                var memberships = db.Memberships.Where(m => m.UserIdHash == userIdHash).ToArray();
                if (memberships.Any(m => m_StringProtector.Unprotect(m.ChannelName) == channelName)) return;
                var newMembership = new ChannelMembershipBinding
                {
                    ChannelName = m_StringProtector.Protect(channelName),
                    UserIdHash = userIdHash
                };
                db.Memberships.Add(newMembership);
                db.SaveChanges();
            }
        }

        public void RemoveChannelMembership(string userId, string channelName)
        {
            var userIdHash = m_StringHasher.Hash(userId);

            using (var db = new DatabaseContext(m_ConnectionString))
            {
                var membership =
                    db.Memberships.FirstOrDefault(m => m.UserIdHash == userIdHash &&
                                                       m_StringProtector.Unprotect(m.ChannelName) == channelName);
                if (membership == null) return;
                db.Memberships.Remove(membership);
                db.SaveChanges();
            }
        }

        public IReadOnlyCollection<string> GetChannelMemberships(string userId)
        {
            var userIdHash = m_StringHasher.Hash(userId);

            using (var db = new DatabaseContext(m_ConnectionString))
            {
                var memberships = db.Memberships.Where(m => m.UserIdHash == userIdHash).ToArray();
                return memberships.Select(m => m_StringProtector.Unprotect(m.ChannelName)).ToArray();
            }
        }

        class DatabaseContext : DbContext
        {
            public DatabaseContext(string connectionString)
                : base(new DbContextOptionsBuilder()
                       .UseSqlServer(connectionString)
                       .Options)
            {
            }

            // ReSharper disable UnusedMember.Local
            public DbSet<MessageBinding> Messages { get; set; }
            public DbSet<ChannelBinding> Channels { get; set; }
            public DbSet<ChannelMembershipBinding> Memberships { get; set; }
            // ReSharper restore UnusedMember.Local
        }
    }
}