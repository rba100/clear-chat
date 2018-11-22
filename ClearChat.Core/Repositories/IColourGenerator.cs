using System.Collections.Specialized;

namespace ClearChat.Core.Repositories
{
    public interface IColourGenerator
    {
        string GenerateFromString(string input);
        bool ValidColour(string colourStr, out string errorMessage);
    }
}
