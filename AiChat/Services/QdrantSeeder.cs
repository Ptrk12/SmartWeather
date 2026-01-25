using Microsoft.Extensions.Configuration;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using SmartComponents.LocalEmbeddings;


namespace AiChat.Services
{
    public class QdrantSeeder
    {
        private const uint VECTOR_SIZE = 384;

        private readonly QdrantClient _client;
        private readonly string _collectionName;

        public QdrantSeeder(IConfiguration config)
        {
            var url = config["Qdrant:Uri"];
            _collectionName = config["Qdrant:CollectionName"];

            if(string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException("Qdrant URL is not configured");
            }
            if(string.IsNullOrEmpty(_collectionName))
            {
                throw new ArgumentNullException("Qdrant Collection Name is not configured");
            }

            _client = new QdrantClient(new Uri(url));
        }

        public async Task SeedAsync()
        {
            var modelPath = Path.Combine(AppContext.BaseDirectory, "Resources");
            using var embedder = new LocalEmbedder(modelPath);

            var collections = await _client.ListCollectionsAsync();

            if (collections.Contains(_collectionName))
            {
                await _client.DeleteCollectionAsync(_collectionName);
            }

            await _client.CreateCollectionAsync(_collectionName, new VectorParams { Size = VECTOR_SIZE, Distance = Distance.Cosine });

            var docsPath = Path.Combine(AppContext.BaseDirectory, "KnowledgeBase");
            if (!Directory.Exists(docsPath)) return;

            var files = Directory.GetFiles(docsPath, "*.md"); 
            var points = new List<PointStruct>();

            foreach (var file in files)
            {
                var text = await File.ReadAllTextAsync(file);

                var paragraphs = text.Split(new[] { "\r\n\r\n", "\n\n" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var paragraph in paragraphs)
                {
                    if (paragraph.Length < 10) continue;

                    var embedding = embedder.Embed(paragraph);

                    var point = new PointStruct
                    {
                        Id = Guid.NewGuid(),

                        Vectors = embedding.Values.ToArray(),
                        Payload = {
                            ["content"] = paragraph,
                            ["source"] = Path.GetFileName(file)
                        }
                    };
                    points.Add(point);
                }
            } 

            if (points.Any())
            {
                await _client.UpsertAsync(_collectionName, points);
            }
        }
    }
}
