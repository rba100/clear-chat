using System;
using System.Linq;
using ClearChat.Core.Crypto;
using ClearChat.Core.Exceptions;
using ClearChat.Core.Repositories.Bindings;
using Microsoft.EntityFrameworkCore;

namespace ClearChat.Core.Repositories
{
    public sealed class AutoResponseRepository : IAutoResponseRepository
    {
        private readonly string m_ConnectionString;
        private readonly IStringHasher m_StringHasher;

        public AutoResponseRepository(string connectionString, IStringHasher stringHasher)
        {
            m_ConnectionString = connectionString
                ?? throw new ArgumentNullException(nameof(connectionString));
            m_StringHasher = stringHasher ?? throw new ArgumentNullException(nameof(stringHasher));
        }

        public string GetResponse(string message)
        {
            using (var db = new DatabaseContext(m_ConnectionString))
            {
                return db.AutoResponses.FirstOrDefault(m => message.Contains(m.Substring))?.Response;
            }
        }

        public void AddResponse(string userId, string message, string response)
        {
            using (var db = new DatabaseContext(m_ConnectionString))
            {
                if (db.AutoResponses.Any(r => r.Substring == message))
                {
                    throw new ArgumentException(nameof(message));
                }

                db.AutoResponses.Add(new AutoResponseBinding
                {
                    UserIdHash = m_StringHasher.Hash(userId),
                    Substring = message,
                    Response = response
                });
                db.SaveChanges();
            }
        }

        private sealed class DatabaseContext : DbContext
        {
            public DatabaseContext(string connectionString)
                : base(new DbContextOptionsBuilder()
                    .UseSqlServer(connectionString)
                    .Options)
            {
            }

            public DbSet<AutoResponseBinding> AutoResponses { get; set; }
        }
    }

    public interface IAutoResponseRepository
    {
        /// <summary>
        /// Gets auto response for given message
        /// </summary>
        /// <remarks>Can return null</remarks>
        string GetResponse(string message);

        /// <summary>
        /// Registers an auto response with the server
        /// </summary>
        /// <exception cref="DuplicateAutoResponseException">
        /// Auto response for given phrase already exists
        /// </exception>
        void AddResponse(string userId, string message, string response);
    }
}
