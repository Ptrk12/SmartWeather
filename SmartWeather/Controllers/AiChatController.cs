using AiChat.Services;
using Interfaces.Ai;
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
        public async Task<IActionResult> Ask([FromBody] string userPrompt, [FromServices] IAiChatManager manager)
        {
            Response.Headers.Append("Content-Type", "text/event-stream");
            Response.Headers.Append("Cache-Control", "no-cache");
            Response.Headers.Append("Connection", "keep-alive");

            bool sentAnyContent = false;

            try
            {
                await foreach (var contentChunk in manager.GetAiChatResponseStreamAsync(userPrompt))
                {
                    var safeContent = contentChunk.Replace("\n", " ").Replace("\r", "");

                    var data = Encoding.UTF8.GetBytes($"data: {safeContent}\n\n");
                    await Response.Body.WriteAsync(data);
                    await Response.Body.FlushAsync();

                    sentAnyContent = true;
                }
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest("The provided prompt is not allowed based on security policy");
            }

            if (!sentAnyContent)
            {
                var fallbackMessage = "data: I couldn't find any information about that\n\n";
                await Response.Body.WriteAsync(Encoding.UTF8.GetBytes(fallbackMessage));
            }

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
