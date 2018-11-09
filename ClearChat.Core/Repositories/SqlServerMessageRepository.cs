using System;
using System.Collections.Generic;
using System.Linq;
using ClearChat.Core.Crypto;
using ClearChat.Core.Domain;
using ClearChat.Core.Repositories.Bindings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace ClearChat.Core.Repositories
{
    public class SqlServerMessageRepository : IMessageRepository
    {
        private readonly string m_ConnectionString;

        private readonly IStringProtector m_StringProtector;

        public SqlServerMessageRepository(string connectionString, 
                                          IStringProtector stringProtector)
        {
            m_ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            m_StringProtector = stringProtector ?? throw new ArgumentNullException(nameof(stringProtector));
        }

        public IReadOnlyCollection<ChatMessage> ChannelMessages(string channelId)
        {
            using (var db = new DatabaseContext(m_ConnectionString))
            {
                var msgs = db.Messages
                             .Where(m => m.ChannelId == channelId)
                             .OrderByDescending(m => m.TimeStampUtc)
                             .Take(400)
                             .ToArray()
                             .Select(FromBinding)
                             .ToArray();
                return msgs;
            }
        }

        private ChatMessage FromBinding(MessageBinding arg)
        {
            return new ChatMessage(m_StringProtector.Unprotect(Convert.FromBase64String(arg.UserId)),
                                   arg.ChannelId,
                                   m_StringProtector.Unprotect(arg.Message),
                                   DateTime.SpecifyKind(arg.TimeStampUtc, DateTimeKind.Utc));
        }

        private MessageBinding ToBinding(ChatMessage arg)
        {
            return new MessageBinding
            {
                UserId = Convert.ToBase64String(m_StringProtector.Protect(arg.UserId)),
                ChannelId = arg.ChannelId,
                Message = m_StringProtector.Protect(arg.Message),
                TimeStampUtc = arg.TimeStampUtc
            };
        }

        public void WriteMessage(ChatMessage message)
        {
            using (var db = new DatabaseContext(m_ConnectionString))
            {
                db.Messages.Add(ToBinding(message));
                db.SaveChanges();
            }
        }

        public void ClearChannel(string channelId)
        {
            using (var db = new DatabaseContext(m_ConnectionString))
            {
                var messagesToRemove = db.Messages.Where(m => m.ChannelId == channelId);
                db.Messages.RemoveRange(messagesToRemove);
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
            public DbSet<MessageBinding> Messages { get; set; }
            // ReSharper restore UnusedMember.Local
        }
    }
}