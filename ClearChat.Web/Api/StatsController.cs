using System.Linq;
using ClearChat.Core;
using ClearChat.Core.Crypto;
using ClearChat.Core.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClearChat.Web.Api
{
    [Route("api/stats")]
    [ApiController]
    [Authorize]
    public class StatsController : ControllerBase
    {
        private readonly IConnectionManager m_ConnectionManager;
        private readonly IMessageRepository m_MessageRepository;
        private readonly IStringHasher m_StringHasher;

        public StatsController(IConnectionManager connectionManager, 
                               IMessageRepository messageRepository, 
                               IStringHasher stringHasher)
        {
            m_ConnectionManager = connectionManager;
            m_MessageRepository = messageRepository;
            m_StringHasher = stringHasher;
        }
    }
}