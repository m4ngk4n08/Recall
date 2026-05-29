using ElBruno.LocalEmbeddings;
using ElBruno.LocalEmbeddings.Options;
using Recall.Api.Services.Interfaces;

namespace Recall.Api.Services
{
    public class LocalEmbeddingService : IEmbeddingService, IDisposable
    {
        private readonly LocalEmbeddingGenerator _generator;
        private bool _disposed;

        public LocalEmbeddingService(IConfiguration configuration, IWebHostEnvironment env)
        {
            var modelPath = Path.Combine(env.ContentRootPath, "Models", "AllMiniLML6V2");
            var options = new LocalEmbeddingsOptions
            {
                ModelPath = modelPath,
            };
            _generator = new LocalEmbeddingGenerator(options);
        }

        public async Task<float[]> GenerateEmbeddingAsync(string text, string? context = null)
        {
            // 1. Contextual Anchoring: If context is provided, we can prepend it to the text to create a richer embedding. This helps the model understand the relevance of the text within a specific domain or topic.
            var input = string.IsNullOrWhiteSpace(context) ? text : $"Document: {context}. Content: {text}";
            var embedding = await _generator.GenerateEmbeddingAsync(input);
            var vector = embedding.Vector.ToArray();

            return L2Normalize(vector);
        }

        public async Task<List<float[]>> GenerateEmbeddingsListAsync(List<string> texts, string? context = null)
        {
            // Apply anchoring to every item in the list.
            var inputs = string.IsNullOrWhiteSpace(context)
                ? texts
                : texts.Select(t => $"Document: {context}. Content: {t}").ToList();    
            var embeddings = await _generator.GenerateAsync(inputs);
            return embeddings.Select(e => L2Normalize(e.Vector.ToArray())).ToList();
        }

        private float[] L2Normalize(float[] vector)
        {
            float squareSum = vector.Sum(x => x * x);
            float norm = (float)Math.Sqrt(squareSum);

            if (norm < 1e-10) return vector;

            for (int i = 0; i < vector.Length; i++) vector[i] /= norm;

            return vector;
        }
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _generator?.Dispose();
        }

    }
}
