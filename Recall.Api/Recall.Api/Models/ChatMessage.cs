using AngleSharp.Dom;
using Google.GenAI.Types;
using Recall.Api.Models;
using System.Reflection;
using System.Runtime.Intrinsics.Arm;
using UglyToad.PdfPig.Graphics.Operations.SpecialGraphicsState;
using static System.Net.Mime.MediaTypeNames;

namespace Recall.Api.Models
{
    public class ChatMessage
    {

        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ConversationId { get; set; }
        public string Role { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public Conversations Conversation { get; set; }
        public string Content { get; set; }

    }
}

