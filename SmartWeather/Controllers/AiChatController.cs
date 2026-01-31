using AiChat.Services;
using Interfaces.Ai;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using System.Text;

namespace SmartWeather.Controllers
{
    [Route("api/ai-chat")]
    [ApiController]
    [Authorize]
    public class AiChatController(Kernel kernel, IntentGuard intentGuard) : ControllerBase
    {
        /// <summary>
        /// Submits a prompt to the AI assistant and streams the generated response
        /// </summary>
        /// <remarks>
        /// This endpoint utilizes Server-Sent Events (SSE) with the 'text/event-stream' Content-Type.
        /// Response chunks are formatted as "data: {content}\n\n".
        /// Newlines within the content are replaced with spaces to ensure stream integrity.
        /// </remarks>
        /// <param name="userPrompt">The text prompt to send to the AI.</param>
        /// <returns>Returns a 200 OK stream on success, or 400 Bad Request if the prompt violates security policies.</returns>
        [HttpPost("ask")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
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
