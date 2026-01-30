using Interfaces.Ai;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace AiChat.Services
{
    public class AiChatManager(
        Kernel kernel,
        IntentGuard intentGuard
        ) : IAiChatManager
    {
        public async IAsyncEnumerable<string> GetAiChatResponseStreamAsync(string userPrompt)
        {
            if (!intentGuard.IsTopicAllowed(userPrompt))
            {
                throw new InvalidOperationException("The provided prompt is not allowed");
            }

            var chatHistory = new ChatHistory();

            chatHistory.AddSystemMessage(
                "You are a helpful and efficient SmartWeather assistant. " +
                "Response Rules: " +
                "1. **Conciseness:** Answer specifically what was asked. Avoid fluff. " +
                "2. **Data Presentation:** When asked for measurements or history, present the data clearly (e.g., as a list or bullet points). " +
                "3. **Chain of Thought (CRITICAL):** Users refer to devices by name (e.g.,'Kitchen sensor'), but tools require IDs. You must follow this sequence if IDs are missing: " +
                "   a) Call 'GetUserGroups' to find the Group ID. " +
                "   b) Call 'GetDevicesInGroup' using that Group ID to find the specific Device ID matching the user's description. " +
                "   c) ONLY THEN call specific tools like 'GetDeviceMeasurements' using the discovered Device ID. " +
                "4. **Knowledge Base:** If the query is about technical documentation search the knowledge base."
            );

            chatHistory.AddUserMessage(userPrompt);

            var chatService = kernel.GetRequiredService<IChatCompletionService>();
            var settings = new PromptExecutionSettings
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            };

            await foreach (var message in chatService.GetStreamingChatMessageContentsAsync(chatHistory, settings, kernel))
            {
                if (!string.IsNullOrEmpty(message.Content))
                {
                    yield return message.Content;
                }
            }
        }
    }
}
