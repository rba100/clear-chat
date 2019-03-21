using System;
using System.Linq;
using ClearChat.Core.Crypto;
using ClearChat.Core.Domain;
using ClearChat.Core.Repositories.Bindings;
using Microsoft.EntityFrameworkCore;

namespace ClearChat.Core.Repositories
{
    public class SqlServerUserRepository : IUserRepository
    {
        private static readonly string[] s_BuiltInUsers = {"System"};

        private readonly string m_ConnectionString;
        private readonly IStringHasher m_StringHasher;
        private readonly IColourGenerator m_ColourGenerator;

        public SqlServerUserRepository(string connectionString, 
                                       IStringHasher stringHasher, 
                                       IColourGenerator colourGenerator)
        {
            m_ConnectionString = connectionString;
            m_StringHasher = stringHasher;
            m_ColourGenerator = colourGenerator;
        }

        private class DatabaseContext : DbContext
        {
            public DatabaseContext(string connectionString)
                : base(new DbContextOptionsBuilder()
                       .UseSqlServer(connectionString)
                       .Options)
            { }

            // ReSharper disable UnusedMember.Local
            public DbSet<UserBinding> Users { get; set; }
            // ReSharper restore UnusedMember.Local
        }

        public bool UserNameExists(string userName)
        {
            using (var db = new DatabaseContext(m_ConnectionString))
            {
                return db.Users.Any(u => u.UserName == userName);
            }
        }

        public void SaveUser(User user, string password)
        {
            var salt = Guid.NewGuid().ToByteArray();
            var passwordHashed = m_StringHasher.Hash(password, salt);

            using (var db = new DatabaseContext(m_ConnectionString))
            {
                var binding = new UserBinding
                {
                    UserName = user.UserName,
                    PasswordHash = passwordHashed,
                    PasswordSalt = salt
                };
                db.Users.Add(binding);
                db.SaveChanges();
            }
        }

        public bool ValidateUser(string userName, string password)
        {
            using (var db = new DatabaseContext(m_ConnectionString))
            {
                var user = db.Users.FirstOrDefault(u => u.UserName == userName);
                return user != null && m_StringHasher.HashMatch(password, 
                                                                user.PasswordHash,
                                                                user.PasswordSalt);
            }
        }

        public User GetUserDetails(string userName)
        {
            if(s_BuiltInUsers.Contains(userName)) return new User(-1, userName, "000000", true);

            using (var db = new DatabaseContext(m_ConnectionString))
            {
                var userBinding = db.Users.FirstOrDefault(u => u.UserName == userName);
                if (userBinding == null) return null;
                return new User(userBinding.Id,
                                userName,
                                userBinding.HexColour ?? m_ColourGenerator.GenerateFromString(userName),
                                IsVerifiedPublicIdentity(userName));
            }
        }

        public User GetUserDetails(int userId)
        {
            using (var db = new DatabaseContext(m_ConnectionString))
            {
                var userBinding = db.Users.Find(userId);
                if (userBinding == null) return null;
                return new User(userBinding.Id,
                                userBinding.UserName,
                                userBinding.HexColour ?? m_ColourGenerator.GenerateFromString(userBinding.UserName),
                                IsVerifiedPublicIdentity(userBinding.UserName));
            }
        }

        public void UpdateUser(User user)
        {
            using (var db = new DatabaseContext(m_ConnectionString))
            {
                var userBinding = db.Users.FirstOrDefault(u => u.UserName == user.UserName);
                userBinding.HexColour = user.HexColour;
                db.SaveChanges();
            }
        }

        private static bool IsVerifiedPublicIdentity(string userId)
        {
            return userId == "Robin";
        }
    }
}