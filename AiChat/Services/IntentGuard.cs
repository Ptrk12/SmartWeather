using AiChat.Constants;
using SmartComponents.LocalEmbeddings;
using System.Numerics.Tensors;

namespace AiChat.Services
{
    public sealed class IntentGuard
    {
        private const float ALLOWED_THRESHLOD = 0.40f;

        private readonly LocalEmbedder _localEmbedder;

        private readonly EmbeddingF32[] _allowedEmbeddings;

        public IntentGuard()
        {
            _localEmbedder = new LocalEmbedder(Path.Combine(AppContext.BaseDirectory, "Resources"));

            _allowedEmbeddings = AiChatConstants.AllowedTopics.Select(topic => _localEmbedder.Embed(topic)).ToArray();
        }

        public bool IsTopicAllowed(string prompt)
        {
            var promptEmbedding = _localEmbedder.Embed(prompt);

            float bestAllowedScore = 0f;

            foreach(var allowed in _allowedEmbeddings)
            {
                var score = TensorPrimitives.CosineSimilarity(promptEmbedding.Values.Span, allowed.Values.Span);

                if(score > bestAllowedScore)
                {
                    bestAllowedScore = score;
                }
            }

            return bestAllowedScore >= ALLOWED_THRESHLOD;
        }


    }
}
