using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using ClearChat.Core.Crypto;
using ClearChat.Core.Domain;

namespace ClearChat.Core.Repositories
{
    public class CachingAutoResponseRepository : IAutoResponseRepository
    {
        private readonly IAutoResponseRepository m_InnerRepository;

        private readonly ConcurrentDictionary<string, AutoResponseTemplate> m_Cache;

        public CachingAutoResponseRepository(IAutoResponseRepository innerRepository, IStringHasher hasher)
        {
            m_InnerRepository = innerRepository;
            m_Cache = new ConcurrentDictionary<string, AutoResponseTemplate>(m_InnerRepository.GetAll()
                .ToDictionary(i => i.SubstringTrigger, i => i));
        }

        public string GetResponse(int channelId, string message)
        {
            return m_Cache.FirstOrDefault(kvp => message.Contains(kvp.Key)).Value?.Response;
        }

        public void AddResponse(int authorUserId, int channelId, string substring, string response)
        {
            m_InnerRepository.AddResponse(authorUserId, channelId, substring, response);
            m_Cache.TryAdd(substring, new AutoResponseTemplate(authorUserId, channelId, substring, response));
        }

        public void RemoveResponse(int channelId, string substring)
        {
            m_InnerRepository.RemoveResponse(channelId, substring);
            m_Cache.TryRemove(substring, out _);
        }

        public IReadOnlyCollection<AutoResponseTemplate> GetAll()
        {
            return m_Cache.Values.ToArray();
        }
    }
}