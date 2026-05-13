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
            var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var chunks = new List<string>();
            var currentChunkWords = new List<string>();

            // Aprox: 200tokens ~ 150 words
            int maxWords = (int)(maxTokens * 0.75);

            foreach (var word in words)
            {
                currentChunkWords.Add(word);
                if(currentChunkWords.Count >= maxWords)
                {
                    chunks.Add(string.Join(' ', currentChunkWords));
                    currentChunkWords.Clear();
                }
            }

            if (currentChunkWords.Any())
                chunks.Add(string.Join(' ', currentChunkWords));

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
            var title = doc.DocumentNode.SelectSingleNode("//head/title")?.InnerText.Trim() ?? "Untitled";
            var body = doc.DocumentNode.SelectSingleNode("//body");
            if (body == null) throw new Exception("No body found");

            // Remove scripts, styles, etc.
            var nodesToRemove = body.SelectNodes(".//script|.//style|.//nav|.//footer|.//aside.//noscript");
            if (nodesToRemove != null)
                foreach (var node in nodesToRemove) node.Remove();

            var text = body.InnerText;
            text = Regex.Replace(text, @"\s+", " ").Trim();
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

            return (title, content, "YouTube");
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
            
            return (title, text, "PDF");
        }
    }
}
