
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
                             .Reverse()
                             .ToArray();
                return msgs;
            }
        }

        private ChatMessage FromBinding(MessageBinding arg, string channelName)
        {
            var userId = m_StringProtector.Unprotect(Convert.FromBase64String(arg.UserId));
            return new ChatMessage(arg.Id,
                                   userId,
                                   channelName,
                                   m_StringProtector.Unprotect(arg.Message),
                                   DateTime.SpecifyKind(arg.TimeStampUtc, DateTimeKind.Utc));
        }

        public ChatMessage WriteMessage(string userId, string channelName, string message, DateTime timeStampUtc)
        {
            var isDefaultChannel = channelName == "default";
            var channelNameHash = isDefaultChannel ? null : m_StringHasher.Hash(channelName);
            using (var db = new DatabaseContext(m_ConnectionString))
            {
                var channel = db.Channels.SingleOrDefault(c => c.ChannelNameHash == channelNameHash);
                var channelId = isDefaultChannel ? 0 : channel.ChannelId;
                var messageBinding = new MessageBinding
                {
                    UserId = Convert.ToBase64String(m_StringProtector.Protect(userId)),
                    ChannelId = channelId,
                    Message = m_StringProtector.Protect(message),
                    TimeStampUtc = timeStampUtc
                };
                db.Messages.Add(messageBinding);
                db.SaveChanges();
                return FromBinding(messageBinding, channelName);
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

        public bool IsChannelPrivate(string channelName)
        {
            if (channelName == "default") return false;

            var channelNameHash = m_StringHasher.Hash(channelName);

            using (var db = new DatabaseContext(m_ConnectionString))
            {
                var channel = db.Channels.Single(c => c.ChannelNameHash == channelNameHash);
                return m_StringHasher.HashMatch("", channel.PasswordHash, channel.PasswordSalt);
            }
        }

        public void AddPermission(string userId, string channelName, string permissionName)
        {
            throw new NotImplementedException();
        }

        public bool HasPermission(string userId, string channelName, string permissionName)
        {
            var userIdHash = m_StringHasher.Hash(userId);
            var channelNameHash = m_StringHasher.Hash(channelName);


            using (var db = new DatabaseContext(m_ConnectionString))
            {
                var channel = db.Channels.FirstOrDefault(c => c.ChannelNameHash == channelNameHash);
                if(channel == null)
                    throw new ArgumentException(nameof(channelName));

                var permission = db.Permissions.FirstOrDefault
                    (p => p.UserIdHash == userIdHash
                          && p.ChannelId == channel.ChannelId
                          && p.PermissionName == permissionName);

                return permission != null;
            }
        }

        public SwitchChannelResult GetOrCreateChannel(string userId, string channelName, string channelPassword)
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

                if (!m_StringHasher.HashMatch(channelPassword, channel.PasswordHash, channel.PasswordSalt))
                {
                    return SwitchChannelResult.Denied;
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

        public IReadOnlyCollection<string> GetChannelMembershipsForUser(string userId)
        {
            var userIdHash = m_StringHasher.Hash(userId);

            using (var db = new DatabaseContext(m_ConnectionString))
            {
                var memberships = db.Memberships.Where(m => m.UserIdHash == userIdHash).ToArray();
                return new[] { "default" }.Concat(memberships.Select(m => m_StringProtector.Unprotect(m.ChannelName)))
                                          .ToArray();
            }
        }

        public IReadOnlyCollection<byte[]> GetChannelMembershipsForChannel(string channelName)
        {
            using (var db = new DatabaseContext(m_ConnectionString))
            {
                var memberships = db.Memberships.Where(m => m_StringProtector.Unprotect(m.ChannelName) == channelName).ToArray();
                return memberships.Select(m => m.UserIdHash).ToArray();
            }
        }

        public void DeleteMessage(int messageId)
        {
            using (var db = new DatabaseContext(m_ConnectionString))
            {
                var message = db.Messages.FirstOrDefault(m => m.Id == messageId);
                if (message == null) return;
                db.Messages.Remove(message);
                db.SaveChanges();
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
            // ReSharper disable UnusedAutoPropertyAccessor.Local
            public DbSet<MessageBinding> Messages { get; set; }
            public DbSet<ChannelBinding> Channels { get; set; }
            public DbSet<ChannelMembershipBinding> Memberships { get; set; }
            public DbSet<ChannelPermissionsBinding> Permissions { get; set; }
            // ReSharper restore UnusedAutoPropertyAccessor.Local
            // ReSharper restore UnusedMember.Local
        }
    }
}