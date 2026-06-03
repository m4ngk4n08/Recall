namespace Recall.Api.Settings
{
    public class OllamaConnectionSettings
    {
        public const string SectionName = "Ollama";
        public string BaseUrl { get; set; } = string.Empty;
        public string DefaultModel { get; set; } = "llama3";
    }
}
