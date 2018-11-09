namespace ClearChat.Crypto
{
    /// <summary>
    /// Provides obfuscation for strings.
    /// </summary>
    public interface IStringProtector
    {
        string Unprotect(byte[] bytes);
        byte[] Protect(string payload);
    }
}