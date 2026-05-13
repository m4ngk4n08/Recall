using Google.GenAI;
using Microsoft.Extensions.AI;
using Recall.Api.Services.Interfaces;

namespace Recall.Api.Services
{
    public class EmbeddingService 
    {
        private readonly Client _client;
        private const string ModelId = "gemini-embedding-2";
        public EmbeddingService(IConfiguration conf)
        {
            var apiKey = conf["Gemini:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
                throw new InvalidOperationException("Gemini API key is not configured. Please set the 'Gemini:ApiKey' configuration value.");
            _client = new Client(apiKey: apiKey);
        }

        public async Task<float[]> GenerateEmbeddingAsync(string text)
        {
            var response = await _client.Models.EmbedContentAsync(
                model: ModelId,
                contents: text
            );

            return response.Embeddings[0].Values.Select(v => (float)v).ToArray();
        }
        public async Task<List<float[]>> GenerateEmbeddingsListAsync(List<string> text)
        {
            var result = new List<float[]>();

            // Gemini batch limit is 100 items per request, so we need to split the input text into chunks if it exceeds that limit.
            const int batchSize = 100;
            for(int i = 0; i < text.Count(); i += batchSize)
            {
                var batch = text.Skip(i).Take(batchSize);
                foreach(var item in batch)
                {
                    var response = await _client.Models.EmbedContentAsync(model: ModelId, contents: item.ToString());
                    result.Add(response.Embeddings[0].Values.Select(v => (float)v).ToArray());
                }
            }

            return result;
        }
    }
}
