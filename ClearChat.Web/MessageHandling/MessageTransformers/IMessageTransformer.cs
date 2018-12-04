namespace ClearChat.Web.MessageHandling.MessageTransformers
{
    public interface IMessageTransformer
    {
        string Transform(string message);
    }
}