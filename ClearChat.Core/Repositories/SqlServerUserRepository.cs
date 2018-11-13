using System;
using System.Linq;
using ClearChat.Core.Crypto;
using ClearChat.Core.Repositories.Bindings;
using Microsoft.EntityFrameworkCore;

namespace ClearChat.Core.Repositories
{
    public class SqlServerUserRepository : IUserRepository
    {
        private readonly string m_ConnectionString;
        private readonly IStringHasher m_StringHasher;

        public SqlServerUserRepository(string connectionString, 
                                       IStringHasher stringHasher)
        {
            m_ConnectionString = connectionString;
            m_StringHasher = stringHasher;
        }

        private class DatabaseContext : DbContext
        {
            public DatabaseContext(string connectionString)
                : base(new DbContextOptionsBuilder()
                       .UseSqlServer(connectionString)
                       .Options)
            {
            }

            // ReSharper disable UnusedMember.Local
            public DbSet<UserBinding> Users { get; set; }
            // ReSharper restore UnusedMember.Local
        }

        public bool UserIdExists(string userId)
        {
            var userIdHashed = m_StringHasher.Hash(userId, new byte[0]);

            using (var db = new DatabaseContext(m_ConnectionString))
            {
                return db.Users.Any(u => u.UserIdHash == userIdHashed);
            }
        }

        public void SaveUser(string userId, string password)
        {
            var userIdHashed = m_StringHasher.Hash(userId, new byte[0]);
            var salt = Guid.NewGuid().ToByteArray();
            var passwordHashed = m_StringHasher.Hash(password, salt);

            using (var db = new DatabaseContext(m_ConnectionString))
            {
                var user = new UserBinding
                {
                    UserIdHash = userIdHashed,
                    PasswordHash = passwordHashed,
                    PasswordSalt = salt
                };
                db.Users.Add(user);
                db.SaveChanges();
            }
        }

        public bool ValidateUser(string userId, string password)
        {
            var userIdHashed = m_StringHasher.Hash(userId, new byte[0]);

            using (var db = new DatabaseContext(m_ConnectionString))
            {
                var user = db.Users.FirstOrDefault(u => u.UserIdHash == userIdHashed);
                return user != null && m_StringHasher.HashMatch(password, 
                                                                user.PasswordHash,
                                                                user.PasswordSalt);
            }
        }
    }
}