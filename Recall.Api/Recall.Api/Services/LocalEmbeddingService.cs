using ElBruno.LocalEmbeddings;
using ElBruno.LocalEmbeddings.Options;
using Recall.Api.Services.Interfaces;

namespace Recall.Api.Services
{
    public class LocalEmbeddingService : IEmbeddingService, IDisposable
    {
        private readonly LocalEmbeddingGenerator _generator;
        private bool _disposed;

        public LocalEmbeddingService(IConfiguration configuration)
        {
            var modelPath = configuration["LocalModel:ModelPath"] ?? "Models/all-MiniLM-L6-v2.F32.gguf";
            var options = new LocalEmbeddingsOptions
            {
                ModelPath = modelPath
            };
            _generator = new LocalEmbeddingGenerator(options);
        }

        public async Task<float[]> GenerateEmbeddingAsync(string text)
        {
            var embedding = await _generator.GenerateEmbeddingAsync(text);
            return embedding.Vector.ToArray();
        }

        public async Task<List<float[]>> GenerateEmbeddingsListAsync(List<string> texts)
        {
            var embeddings = await _generator.GenerateAsync(texts);
            return embeddings.Select(e => e.Vector.ToArray()).ToList();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _generator?.Dispose();
        }

    }
}
