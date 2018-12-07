
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
                var channelId = isDefaultChannel ? 0 : db.Channels
                                                         .SingleOrDefault(c => c.ChannelNameHash == channelNameHash)
                                                         ?.ChannelId;

                if (!channelId.HasValue) return new ChatMessage[0];

                var msgs = db.Messages
                             .Where(m => m.ChannelId == channelId.Value)
                             .OrderByDescending(m => m.TimeStampUtc)
                             .Take(400)
                             .ToArray()
                             .Reverse()
                             .ToArray();

                var mIds = msgs.Select(m => m.Id).ToArray();

                var attachments = db.MessageAttachments.Where(a => mIds.Contains(a.MessageId))
                                    .Select(m => new { m.MessageId, m.Id })
                                    .GroupBy(a => a.MessageId)
                                    .ToDictionary(g => g.Key, g => g.Select(a => a.Id).ToArray());

                return msgs.Select(m => FromBinding(m, attachments, channelName)).ToArray();
            }
        }

        private ChatMessage FromBinding(MessageBinding arg,
                                        Dictionary<int, int[]> attachments,
                                        string channelName)
        {
            var userId = m_StringProtector.Unprotect(Convert.FromBase64String(arg.UserId));

            var attachmentIds = attachments != null && attachments.ContainsKey(arg.Id) ? attachments[arg.Id] : new int[0];

            return new ChatMessage(arg.Id,
                                   userId,
                                   channelName,
                                   m_StringProtector.Unprotect(arg.Message),
                                   attachmentIds,
                                   DateTime.SpecifyKind(arg.TimeStampUtc, DateTimeKind.Utc));
        }

        public ChatMessage WriteMessage(string userId, string channelName, string message, DateTime timeStampUtc)
        {
            var isDefaultChannel = channelName == "default";
            var channelNameHash = isDefaultChannel ? null : m_StringHasher.Hash(channelName);
            using (var db = new DatabaseContext(m_ConnectionString))
            {
                var channelId = isDefaultChannel ? 0 : db.Channels
                                                         .Single(c => c.ChannelNameHash == channelNameHash)
                                                         .ChannelId;
                var messageBinding = new MessageBinding
                {
                    UserId = Convert.ToBase64String(m_StringProtector.Protect(userId)),
                    ChannelId = channelId,
                    Message = m_StringProtector.Protect(message),
                    TimeStampUtc = timeStampUtc
                };
                db.Messages.Add(messageBinding);
                db.SaveChanges();
                return FromBinding(messageBinding, null, channelName);
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

        public int AddAttachment(int messageId, string contentType, byte[] content)
        {
            using (var db = new DatabaseContext(m_ConnectionString))
            {
                var binding = new MessageAttachmentBinding
                {
                    MessageId = messageId,
                    Content = content,
                    ContentType = contentType,
                };
                db.MessageAttachments.Add(binding);
                db.SaveChanges();
                return binding.Id;
            }
        }

        public IReadOnlyCollection<MessageAttachment> GetAttachments(IReadOnlyCollection<int> messageIds)
        {
            using (var db = new DatabaseContext(m_ConnectionString))
            {
                return db.MessageAttachments.Where(a => messageIds.Contains(a.MessageId))
                         .ToArray()
                         .Select(FromBinding)
                         .ToArray();
            }
        }

        public MessageAttachment GetAttachment(int attachmentId)
        {
            using (var db = new DatabaseContext(m_ConnectionString))
            {
                return db.MessageAttachments
                         .Where(a => a.Id == attachmentId)
                         .AsEnumerable()
                         .Select(FromBinding)
                         .FirstOrDefault();
            }
        }

        public void DeleteAttachment(int messageAttachmentId)
        {
            using (var db = new DatabaseContext(m_ConnectionString))
            {
                var binding = db.MessageAttachments.Single(a => a.Id == messageAttachmentId);
                db.Remove(binding);
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

                var ids = db.MessageAttachments
                            .Where(a => a.MessageId == messageId)
                            .Select(a => a.Id);

                foreach (var id in ids)
                {
                    db.MessageAttachments
                      .Attach(new MessageAttachmentBinding { Id = id, MessageId = messageId })
                      .State = EntityState.Deleted;
                }

                db.Messages.Remove(message);
                db.SaveChanges();
            }
        }

        private static MessageAttachment FromBinding(MessageAttachmentBinding binding)
        {
            return new MessageAttachment(binding.Id,
                                         binding.MessageId,
                                         binding.Content,
                                         binding.ContentType);
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
            public DbSet<MessageAttachmentBinding> MessageAttachments { get; set; }
            public DbSet<ChannelBinding> Channels { get; set; }
            public DbSet<ChannelMembershipBinding> Memberships { get; set; }
            // ReSharper restore UnusedAutoPropertyAccessor.Local
            // ReSharper restore UnusedMember.Local
        }
    }
}