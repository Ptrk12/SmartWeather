using AiChat.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text;

namespace SmartWeather.Controllers
{
    [Route("api/ai-chat")]
    [ApiController]
    [Authorize]
    public class AiChatController(Kernel kernel, IntentGuard intentGuard) : ControllerBase
    {
        [HttpPost("ask")]
        public async Task<IActionResult> Ask([FromBody] string userPrompt)
        {
            if(!intentGuard.IsTopicAllowed(userPrompt))
            {
                return BadRequest("The provided prompt is not allowed");
            }

            var chatHistory = new ChatHistory();

            chatHistory.AddSystemMessage(
                "You are a helpful SmartWeather assistant. " +
                "Rules: " +
                "1. Answer ONLY specifically what was asked. Do NOT volunteer extra details (like descriptions, creation dates, IDs, or locations) unless explicitly asked. " +
                "2. Be concise. If asked for a count, just provide the count in a full sentence. " +
                "3. Start answers with 'You have' or 'I found'. " +
                "Example: User: 'How many devices?'. AI: 'You have 5 devices.' (Do NOT add 'created on...')."
            );

            chatHistory.AddUserMessage(userPrompt);

            var chatService = kernel.GetRequiredService<IChatCompletionService>();
            var settings = new PromptExecutionSettings
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() 
            };

            Response.Headers.Append("Content-Type", "text/event-stream");
            Response.Headers.Append("Cache-Control", "no-cache");
            Response.Headers.Append("Connection", "keep-alive");

            bool sentAnyContent = false;

            await foreach (var message in chatService.GetStreamingChatMessageContentsAsync(chatHistory, settings, kernel))
            {
                if (!string.IsNullOrEmpty(message.Content))
                {
                    var safeContent = message.Content.Replace("\n", " ").Replace("\r", "");

                    var data = Encoding.UTF8.GetBytes($"data: {safeContent}\n\n");
                    await Response.Body.WriteAsync(data);
                    await Response.Body.FlushAsync();

                    sentAnyContent = true;
                }
            }

            if (!sentAnyContent)
            {
                var fallbackMessage = "data: I couldn't find any information about that in the documentation.\n\n";
                await Response.Body.WriteAsync(Encoding.UTF8.GetBytes(fallbackMessage));
            }

            await Response.Body.WriteAsync(Encoding.UTF8.GetBytes("data: [DONE]\n\n"));

            return new EmptyResult();
        }

        [HttpPost("seed-knowledge-base")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> SeedKnowledgeBase([FromServices] QdrantSeeder seeder, [FromServices] IWebHostEnvironment env)
        {
            if (!env.IsDevelopment())
            {
                return NotFound();
            }

            await seeder.SeedAsync();
            return Ok("Knowledge base seeded successfully");
        }
    }
}
