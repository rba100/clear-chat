
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
            using (var db = new DatabaseContext(m_ConnectionString))
            {
                var channelId = db.Channels.SingleOrDefault(c => c.ChannelName == channelName)?.Id;

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

                return msgs.Select(m => FromBinding(db, m, attachments, channelName)).ToArray();
            }
        }

        private ChatMessage FromBinding(DatabaseContext db,
                                        MessageBinding arg,
                                        Dictionary<int, int[]> attachments,
                                        string channelName)
        {
            var attachmentIds = attachments != null && attachments.ContainsKey(arg.Id) ? attachments[arg.Id] : new int[0];

            var userName = db.Users.Single(u => u.Id == arg.UserId).UserName;

            return new ChatMessage(arg.Id,
                                   userName,
                                   channelName,
                                   m_StringProtector.Unprotect(arg.Message),
                                   attachmentIds,
                                   DateTime.SpecifyKind(arg.TimeStampUtc, DateTimeKind.Utc));
        }

        public ChatMessage WriteMessage(int userId, string channelName, string message, DateTime timeStampUtc)
        {
            using (var db = new DatabaseContext(m_ConnectionString))
            {
                var channelId = db.Channels.Single(c => c.ChannelName == channelName).Id;
                var messageBinding = new MessageBinding
                {
                    UserId = userId,
                    ChannelId = channelId,
                    Message = m_StringProtector.Protect(message),
                    TimeStampUtc = timeStampUtc
                };
                db.Messages.Add(messageBinding);
                db.SaveChanges();
                return FromBinding(db, messageBinding, null, channelName);
            }
        }

        public void ClearChannel(string channelName)
        {
            using (var db = new DatabaseContext(m_ConnectionString))
            {
                var channel = db.Channels.SingleOrDefault(c => c.ChannelName == channelName);
                var channelId = channel.Id;
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

            using (var db = new DatabaseContext(m_ConnectionString))
            {
                var channel = db.Channels.Single(c => c.ChannelName == channelName);
                return channel.PasswordSalt != null
                       && m_StringHasher.HashMatch("", channel.PasswordHash, channel.PasswordSalt);
            }
        }

        public SwitchChannelResult GetOrCreateChannel(string channelName, string channelPassword)
        {
            if (channelName == "default") return SwitchChannelResult.Accepted;

            using (var db = new DatabaseContext(m_ConnectionString))
            {
                var channel = db.Channels.SingleOrDefault(c => c.ChannelName == channelName);
                if (channel == null)
                {
                    var salt = Guid.NewGuid().ToByteArray();
                    channel = new ChannelBinding
                    {
                        ChannelName = channelName,
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

        public void AddChannelMembership(int userId, string channelName)
        {
            using (var db = new DatabaseContext(m_ConnectionString))
            {
                var channelId = db.Channels.Single(u => u.ChannelName == channelName).Id;
                var exists = db.Memberships.Any(m => m.UserId == userId && m.ChannelId == channelId);
                if (exists) return;
                var newMembership = new ChannelMembershipBinding
                {
                    ChannelId = channelId,
                    UserId = userId
                };
                db.Memberships.Add(newMembership);
                db.SaveChanges();
            }
        }

        public void RemoveChannelMembership(int userId, string channelName)
        {
            using (var db = new DatabaseContext(m_ConnectionString))
            {
                var channelId = db.Channels.Single(u => u.ChannelName == channelName).Id;
                var membership = db.Memberships.FirstOrDefault(m => m.UserId == userId && m.ChannelId == channelId);
                if (membership == null) return;
                db.Memberships.Remove(membership);
                db.SaveChanges();
            }
        }

        public Channel GetChannelInformation(string channelName)
        {
            using (var db = new DatabaseContext(m_ConnectionString))
            {
                var channel = db.Channels.Single(u => u.ChannelName == channelName);
                var members = db.Memberships.Where(m => m.ChannelId == channel.Id).Select(m=>m.UserId).ToArray();
                return new Channel(channel.Id,channelName);
            }
        }

        public IReadOnlyCollection<string> GetChannelMembershipsForUser(int userId)
        {
            using (var db = new DatabaseContext(m_ConnectionString))
            {
                var memberships = db.Memberships.Where(m => m.UserId == userId).Select(m => m.ChannelId).ToArray();
                var channels = db.Channels.Where(c => memberships.Contains(c.Id));
                return new[] { "default" }.Concat(channels.Select(c => c.ChannelName))
                                          .ToArray();
            }
        }

        public IReadOnlyCollection<int> GetChannelMembershipsForChannel(string channelName)
        {
            using (var db = new DatabaseContext(m_ConnectionString))
            {
                var channelId = db.Channels.Single(u => u.ChannelName == channelName).Id;
                var memberships = db.Memberships.Where(m => m.ChannelId == channelId).ToArray();
                return memberships.Select(m=>m.UserId).ToArray();
            }
        }

        public bool UserIsInChannel(User user, Channel channel)
        {
            if (channel.IsDefault) return true;
            using (var db = new DatabaseContext(m_ConnectionString))
            {
                return db.Memberships.Any(m => m.ChannelId == channel.Id && m.UserId == user.Id);
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

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<ChannelMembershipBinding>().HasKey(c => new {c.UserId, c.ChannelId});
                base.OnModelCreating(modelBuilder);
            }

            // ReSharper disable UnusedMember.Local
            // ReSharper disable UnusedAutoPropertyAccessor.Local
            public DbSet<UserBinding> Users { get; set; }
            public DbSet<MessageBinding> Messages { get; set; }
            public DbSet<MessageAttachmentBinding> MessageAttachments { get; set; }
            public DbSet<ChannelBinding> Channels { get; set; }
            public DbSet<ChannelMembershipBinding> Memberships { get; set; }
            // ReSharper restore UnusedAutoPropertyAccessor.Local
            // ReSharper restore UnusedMember.Local
        }
    }
}