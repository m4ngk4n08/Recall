using HtmlAgilityPack;
using Recall.Api.Services.Interfaces;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;
using YoutubeExplode;

namespace Recall.Api.Services
{
    public class ExtractionService : IExtractionService
    {
        private readonly HttpClient _httpClient;
        private readonly YoutubeClient _youtubeClient;
        public ExtractionService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _youtubeClient = new YoutubeClient();
        }
        public List<string> ChunkText(string text, int maxTokens = 200)
        {
            if (string.IsNullOrWhiteSpace(text)) return new List<string>();

            // Approx: 200 tokens ~ 150 words
            int maxWords = (int)(maxTokens * 0.75);
            int overlapWords = (int)(maxWords * 0.15); // 15% overlap for context preservation

            // Split by sentence boundaries to avoid cutting mid-thought
            var sentences = Regex.Split(text, @"(?<=[.!?])\s+");
            var chunks = new List<string>();
            var currentChunkWords = new List<string>();
            int currentCount = 0;

            foreach (var sentence in sentences)
            {
                var words = sentence.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                
                // If adding this sentence exceeds maxWords, save current chunk
                if (currentCount + words.Length > maxWords && currentChunkWords.Any())
                {
                    chunks.Add(string.Join(" ", currentChunkWords));
                    
                    // Sliding window: keep last N words for next chunk context
                    var overlap = currentChunkWords.TakeLast(overlapWords).ToList();
                    currentChunkWords = new List<string>(overlap);
                    currentCount = currentChunkWords.Count;
                }

                currentChunkWords.AddRange(words);
                currentCount += words.Length;

                // Handle edge case where a single sentence is longer than maxWords
                if (currentCount >= maxWords)
                {
                    chunks.Add(string.Join(" ", currentChunkWords));
                    var overlap = currentChunkWords.TakeLast(overlapWords).ToList();
                    currentChunkWords = new List<string>(overlap);
                    currentCount = currentChunkWords.Count;
                }
            }

            if (currentChunkWords.Count > overlapWords)
                chunks.Add(string.Join(" ", currentChunkWords));

            return chunks;
        }

        public async Task<(string Title, string Content, string SourceType)> ExtractFromUrlAsync(string url)
        {
            if (url.Contains("youtube.com") || url.Contains("youtu.be"))
                return await ExtractYouTubeAsync(url);
            if(url.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                return await ExtractPdfAsync(url);

            return await ExtractWebAsync(url);
        }

        private async Task<(string Title, string Content, string SourceType)> ExtractWebAsync(string url)
        {
            var web = new HtmlWeb();
            var doc = await web.LoadFromWebAsync(url);
            
            // 1. Title extraction (OG tags -> H1 -> Title)
            var title = doc.DocumentNode.SelectSingleNode("//meta[@property='og:title']")?.GetAttributeValue("content", null)
                        ?? doc.DocumentNode.SelectSingleNode("//h1")?.InnerText.Trim()
                        ?? doc.DocumentNode.SelectSingleNode("//head/title")?.InnerText.Trim() 
                        ?? "Untitled";

            // 2. Identify the main content container
            var mainContent = doc.DocumentNode.SelectSingleNode("//article") 
                              ?? doc.DocumentNode.SelectSingleNode("//main")
                              ?? doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'content') or contains(@class, 'post') or contains(@id, 'content')]")
                              ?? doc.DocumentNode.SelectSingleNode("//body");

            if (mainContent == null) return (title, "", "Web");

            // 3. Boilerplate removal
            var boilerplateSelectors = new[] { 
                ".//script", ".//style", ".//nav", ".//footer", ".//aside", ".//header", 
                ".//iframe", ".//form", ".//button", ".//svg", ".//noscript",
                ".//*[contains(@class, 'sidebar') or contains(@class, 'nav') or contains(@class, 'footer') or contains(@class, 'ads')]"
            };

            foreach (var selector in boilerplateSelectors)
            {
                var nodes = mainContent.SelectNodes(selector);
                if (nodes != null) foreach (var node in nodes) node.Remove();
            }

            // 4. Metadata description
            var description = doc.DocumentNode.SelectSingleNode("//meta[@name='description']")?.GetAttributeValue("content", null)
                              ?? doc.DocumentNode.SelectSingleNode("//meta[@property='og:description']")?.GetAttributeValue("content", null);

            var text = SanitizeContent(mainContent.InnerText);

            // Prepend description if it's not already at the start of the text
            if (!string.IsNullOrEmpty(description))
            {
                description = SanitizeContent(description);
                if (!text.StartsWith(description.Substring(0, Math.Min(20, description.Length))))
                {
                    text = description + ". " + text;
                }
            }

            return (title, text, "Web");
        }

        private async Task<(string Title, string Content, string SourceType)> ExtractYouTubeAsync(string url)
        {
            var video = await _youtubeClient.Videos.GetAsync(url);
            var title = video.Title;

            // Get closed caption if available
            var trackManifest = await _youtubeClient.Videos.ClosedCaptions.GetManifestAsync(video.Id);
            var captionTrack = trackManifest.GetByLanguage("en") ?? trackManifest.Tracks.FirstOrDefault();
            string content = video.Description;

            if (captionTrack != null)
            {
                var captions = await _youtubeClient.Videos.ClosedCaptions.GetAsync(captionTrack);
                content += "\n" + string.Join(" ", captions.Captions.Select(c => c.Text));
            }

            return (title, SanitizeContent(content), "YouTube");
        }

        private async Task<(string Title, string Content, string SourceType)> ExtractPdfAsync(string url)
        {
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var pdfBytes = await response.Content.ReadAsByteArrayAsync();
            string text = "";

            using(var stream = new MemoryStream(pdfBytes))
            using(var pdf = PdfDocument.Open(stream))
            {
                foreach (var page in pdf.GetPages())
                    text += page.Text;
            }
            var title = Path.GetFileNameWithoutExtension(new Uri(url).AbsolutePath ?? "PDF Document");
            
            return (title, SanitizeContent(text), "PDF");
        }

        private string SanitizeContent(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;

            // Remove non-ASCII/unnecessary characters, keeping English words, numbers and basic punctuation
            string sanitized = Regex.Replace(text, @"[^a-zA-Z0-9\s.,!?;:'""()\[\]{}\-_]", "");
            
            // Normalize whitespace
            sanitized = Regex.Replace(sanitized, @"\s+", " ").Trim();
            
            return sanitized;
        }
    }
}
