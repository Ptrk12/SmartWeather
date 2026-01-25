using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Qdrant.Client;
using SmartComponents.LocalEmbeddings;
using System.ComponentModel;

namespace AiChat.Plugins
{
    public class KnoweldgeBasePlugin
    {
        private readonly QdrantClient _client;
        private readonly LocalEmbedder _localEmbedder;
        private readonly string _collectionName;

        public KnoweldgeBasePlugin(IConfiguration config)
        {

            var url = config["Qdrant:Uri"];
            _collectionName = config["Qdrant:CollectionName"];

            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException("Qdrant URL is not configured");
            }
            if (string.IsNullOrEmpty(_collectionName))
            {
                throw new ArgumentNullException("Qdrant Collection Name is not configured");
            }

            _localEmbedder = new LocalEmbedder(Path.Combine(AppContext.BaseDirectory, "Resources"));
            _client = new QdrantClient(new Uri(url));
        }

        [KernelFunction]
        [Description("Searches the technical documentation/knowledge base for help and configuration.")]
        public async Task<string> SearchDocumentation(
        [Description("The specific query or issue to search for (e.g, 'Is device serial number is unique in the system?')")] string query)
        {
            var queryEmbedding = _localEmbedder.Embed(query);

            var searchResult = await _client.SearchAsync(_collectionName, queryEmbedding.Values.ToArray(), limit: 3, scoreThreshold: 0.5f);

            if (!searchResult.Any())
            {
                return "No relevant documentation found.";
            }

            var results = searchResult.Select(point => $"- {point.Payload["content"].StringValue} (Score: {point.Score:F2})");

            return string.Join("\n\n", results);
        }
    }
}
