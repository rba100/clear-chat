using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

using ClearChat.Core.Domain;

namespace ClearChat.Core.Repositories
{
    public class RateLimitingAutoResponseRepository : IAutoResponseRepository
    {
        private readonly ConcurrentDictionary<string, DateTime> m_LastHits 
            = new ConcurrentDictionary<string, DateTime>();

        private readonly IAutoResponseRepository m_InnerRepository;
        private readonly TimeSpan m_MinimumResponseInterval;

        public RateLimitingAutoResponseRepository(IAutoResponseRepository innerRepository, 
                                                  TimeSpan minimumResponseInterval)
        {
            m_InnerRepository = innerRepository;
            m_MinimumResponseInterval = minimumResponseInterval;
            foreach (var autoResponse in innerRepository.GetAll())
            {
                m_LastHits.TryAdd(autoResponse.SubstringTrigger,
                    DateTime.MinValue.ToUniversalTime());
            }
        }

        public string GetResponse(int channelId, string message)
        {
            var response = m_LastHits.Keys.FirstOrDefault(k => message.Contains(k));
            if (response != null)
            {
                var now = DateTime.UtcNow;
                if (now - m_LastHits[response] < m_MinimumResponseInterval) return null;
                m_LastHits[response] = now;
            }

            return m_InnerRepository.GetResponse(channelId, message);
        }

        public void AddResponse(int authorUserId, int channelId, string substring, string response)
        {
            m_InnerRepository.AddResponse(authorUserId, channelId, substring, response);
            m_LastHits.TryAdd(substring, DateTime.MinValue.ToUniversalTime());
        }

        public void RemoveResponse(int channelId, string substring)
        {
            m_InnerRepository.RemoveResponse(channelId, substring);
            m_LastHits.TryRemove(substring, out DateTime _);
        }

        public IReadOnlyCollection<AutoResponseTemplate> GetAll()
        {
            return m_InnerRepository.GetAll();
        }
    }
}