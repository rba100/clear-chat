using System;

namespace ClearChat.Web.MessageHandling.MessageTransformers
{
    public class ImageLinkMessageTransformer : IMessageTransformer
    {
        public string Transform(string message)
        {
            var lower = message.ToLowerInvariant();
            if ((lower.EndsWith("jpg") || lower.EndsWith("gif")) &&
                Uri.IsWellFormedUriString(message, UriKind.Absolute))
            {
                return $"![auto-image]({message})";
            }
            return message;
            
        }
    }
}
