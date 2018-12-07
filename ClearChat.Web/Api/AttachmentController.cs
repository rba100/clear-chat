
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using ClearChat.Core.Repositories;

namespace ClearChat.Web.Api
{
    [Route("api/attachment")]
    [ApiController]
    [Authorize]
    public class AttachmentController : ControllerBase
    {
        private readonly IMessageRepository m_MessageRepository;

        public AttachmentController(IMessageRepository messageRepository)
        {
            m_MessageRepository = messageRepository;
        }

        // TODO Channel Security
        [HttpGet]
        [Route("{attachmentId}")]
        public void Get(int attachmentId)
        {
            var attachment = m_MessageRepository.GetAttachments(new[] { attachmentId }).SingleOrDefault();
            if (attachment == null)
            {
                Response.StatusCode = 404;
                return;
            }

            Response.ContentType = attachment.ContentType;
            Response.Body.Write(attachment.Content);
            Response.Body.Flush();
        }
    }
}
