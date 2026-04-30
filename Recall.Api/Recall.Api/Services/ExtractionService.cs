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
        public List<string> ChunkText(string text, int chunkSize = 500, int overlap = 50)
        {
            var chunks = new List<string>();
            var words = text.Split(' ');

            for (int i = 0; i < words.Length; i += chunkSize - overlap)
            {
                var chunk = string.Join(' ', words.Skip(i).Take(chunkSize));
                chunks.Add(chunk);
                if (i + chunkSize >= words.Length) break; // Avoid adding empty chunk at the end
            }
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
