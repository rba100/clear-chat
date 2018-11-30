using System;
using System.Linq;
using ClearChat.Core.Repositories.Bindings;
using Microsoft.EntityFrameworkCore;

namespace ClearChat.Core.Repositories
{
    internal sealed class AutoResponseRepository : IAutoResponseRepository
    {
        private readonly string m_ConnectionString;

        public AutoResponseRepository(string connectionString)
        {
            m_ConnectionString = connectionString
                ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public string GetResponse(string message)
        {
            using (var db = new DatabaseContext(m_ConnectionString))
            {
                return db.AutoResponses.FirstOrDefault(m => m.Message == message)?.Response;
            }
        }

        public void AddResponse(string userId, string message, string response)
        {
            using (var db = new DatabaseContext(m_ConnectionString))
            {
                if (db.AutoResponses.Any(r => r.Message == message))
                {
                    throw new ArgumentException(nameof(message));
                }

                db.AutoResponses.Add(new AutoResponseBinding
                {
                    UserId = userId,
                    Message = message,
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

    internal interface IAutoResponseRepository
    {
        /// <summary>
        /// Gets auto response for given message
        /// </summary>
        /// <remarks>Can return null</remarks>
        string GetResponse(string message);


        void AddResponse(string userId, string message, string response);
    }
}
