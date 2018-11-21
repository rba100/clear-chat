namespace ClearChat.Core.Crypto
{
    /// <summary>
    /// Provides hashing for strings.
    /// </summary>
    public interface IStringHasher
    {
        byte[] Hash(string payload);

        byte[] Hash(string payload, byte[] salt);

        bool HashMatch(string match, byte[] hash, byte[] salt);
    }
}