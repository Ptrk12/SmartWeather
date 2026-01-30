namespace Interfaces.Ai
{
    public interface IAiChatManager
    {
        IAsyncEnumerable<string> GetAiChatResponseStreamAsync(string userPrompt);
    }
}
