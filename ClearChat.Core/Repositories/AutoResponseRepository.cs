﻿using System;
using System.Collections.Generic;
using System.Linq;
using ClearChat.Core.Crypto;
using ClearChat.Core.Domain;
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

        public void AddResponse(string creatorId, string substring, string response)
        {
            using (var db = new DatabaseContext(m_ConnectionString))
            {
                if (db.AutoResponses.Any(r => r.Substring.Contains(substring)))
                {
                    throw new ArgumentException(
                        $"There's is already an auto-response that will respond to '{substring}'", nameof(substring));
                }

                db.AutoResponses.Add(new AutoResponseBinding
                {
                    UserIdHash = m_StringHasher.Hash(creatorId),
                    Substring = substring,
                    Response = response
                });
                db.SaveChanges();
            }
        }

        public void RemoveResponse(string substring)
        {
            using (var db = new DatabaseContext(m_ConnectionString))
            {
                var existing = db.AutoResponses.FirstOrDefault(s => s.Substring.Contains(substring));
                if (existing == null) return;
                db.Remove(existing);
                db.SaveChanges();
            }
        }

        public IReadOnlyCollection<AutoResponseTemplate> GetAll()
        {
            using (var db = new DatabaseContext(m_ConnectionString))
            {
                return db.AutoResponses
                         .ToArray()
                         .Select(r => new AutoResponseTemplate(r.UserIdHash, 
                                                               r.Substring, 
                                                               r.Response))
                         .ToArray();
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
}
