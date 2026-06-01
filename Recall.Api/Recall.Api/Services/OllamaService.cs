using System.Text;
using System.Text.Json;
using Recall.Api.DTOs.Chat;
using Recall.Api.Services.Interfaces;

namespace Recall.Api.Services
{
    public class OllamaService : IOllamaService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<OllamaService> _logger;
        private const string OllamaEndpoint = "http://localhost:11434/api";

        public OllamaService(HttpClient httpClient, ILogger<OllamaService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<string> GenerateAnswerAsync(string query, string context, string model = "llama3")
        {
            var systemPrompt = "You are a helpful assistant for the 'Recall' application. " +
                               "Use the following pieces of retrieved context to answer the user's question. " +
                               "If you don't know the answer or it's not in the context, just say that you don't know based on the provided documents. " +
                               "Keep the answer concise.\n\n" +
                               $"Context:\n{context}";

            var requestBody = new
            {
                model = model,
                prompt = $"Question: {query}\n\nAnswer based on the context above:",
                system = systemPrompt,
                stream = false
            };

            try
            {
                var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{OllamaEndpoint}/generate", content);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Ollama returned error: {StatusCode}. Body: {ErrorBody}", response.StatusCode, errorBody);

                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        return $"Error: Ollama returned 404. This usually means the model '{model}' is not downloaded. Please run 'ollama pull {model}' in your terminal.";
                    }
                    return $"Error: Ollama returned {response.StatusCode}. Check server logs.";
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(jsonResponse);
                return doc.RootElement.GetProperty("response").GetString() ?? "No response generated.";
            }
            catch (TaskCanceledException ex) when (!ex.CancellationToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "Ollama request timed out.");
                return "Error: Ollama request timed out. The model might be taking too long to process.";
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error connecting to Ollama.");
                return "Error: Could not reach Ollama server. Ensure Ollama is running at http://localhost:11434.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error calling Ollama API.");
                return $"Error: {ex.Message}";
            }
        }

        public async Task<string> GenerateChatResponseAsync(string query, string context, List<ChatMessageDto> conversationHistory, string model = "llama3")
        {
            var messages = new List<object>();

            // 1. Add System Message with Context
            messages.Add(new
            {
                role = "system",
                content = "You are a helpful assistant for the 'Recall' application. " +
                          "Use the following pieces of retrieved context to answer the user's question. " +
                          "If you don't know the answer or it's not in the context, just say that you don't know based on the provided documents. " +
                          "Keep the answer concise.\n\n" +
                          $"Context:\n{context}"
            });

            // 2. Add Histroy (Previous back-and-forth messages)
            foreach (var message in conversationHistory)
            {
                messages.Add(new { role = message.Role, content = message.Content
                });
            };

            // 3. Add Current User Query
            messages.Add(new { role = "user", content = query });

            var requestBody = new
            {
                model = model,
                messages = messages, // Structured list of messages
                stream = false
            };

            try
            {
                var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{OllamaEndpoint}/chat", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Ollama returned errror: {StatusCode}. Body: {ErrorBody}", response.StatusCode, errorBody);
                    return "Error: Ollama returned {response.StatusCode}. Check server logs.";
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(jsonResponse);
                return doc.RootElement.GetProperty("message").GetProperty("content").GetString() ?? "No response";

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error calling Ollama API.");
                return $"Error: {ex.Message}";
            }

        }
    }
}

