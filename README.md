# Recall: Your AI-Powered Personal Knowledge Base

Recall is a full-stack application that acts as a central repository for your digital knowledge. It extracts content from the web, documents, and videos, turning them into a searchable, queryable vector database that you can chat with using local AI.

---

## 🚀 Key Features

*   **Multi-Source Ingestion**:
    *   **Web Articles**: Clean extraction of main content, removing ads and navigation.
    *   **YouTube**: Automatic transcript fetching and description analysis.
    *   **PDF Documents**: Full text extraction using `PdfPig`.
*   **Semantic Search & RAG**:
    *   Uses **Local Embeddings** (`AllMiniLML6V2`) to convert text into 384-dimensional vectors.
    *   Implements **Contextual Anchoring** and **L2 Normalization** for high-precision retrieval.
    *   Stored in **PostgreSQL** using the `pgvector` extension.
*   **AI Chat**: Talk to your documents via **Ollama** integration.
*   **Auto-Tagging**: Organizes content by topics for easy navigation.

---

## 🛠️ Tech Stack

### Backend (Recall.Api)
*   **Framework**: .NET 8.0 Web API
*   **Database**: PostgreSQL with `pgvector`
*   **ORM**: Entity Framework Core
*   **AI/Embeddings**: 
    *   `ElBruno.LocalEmbeddings` (ONNX runtime for local inference)
    *   Gemini API (Optional fallback)
    *   Ollama (Local LLM orchestration)
*   **Extraction**: HtmlAgilityPack, YoutubeExplode, PdfPig

### Frontend (recall-app)
*   **Framework**: Angular 21 (Signals, Zoneless)
*   **State Management**: RxJS & Angular Signals
*   **Styling**: Vanilla CSS (Modern CSS variables and flexbox/grid)

---

## 📋 Prerequisites

*   [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
*   [Node.js & NPM](https://nodejs.org/)
*   [PostgreSQL](https://www.postgresql.org/) (with `vector` extension installed)
*   [Ollama](https://ollama.ai/) (for local chat capabilities)

---

## 🛠️ Setup Instructions

### 1. Database Configuration
Ensure your PostgreSQL instance has the `pgvector` extension enabled.
```sql
CREATE EXTENSION IF NOT EXISTS vector;
```

### 2. Backend Setup
Update `appsettings.json` with your connection string and (optional) Gemini API Key.
```bash
cd Recall.Api/Recall.Api
dotnet run
```

### 3. Frontend Setup
```bash
cd recall-app
npm install
npm start
```

---

## 🧠 Core Architecture Logic

### Vectorization Pipeline
1.  **Extraction**: Raw content is pulled and sanitized.
2.  **Chunking**: Text is split into ~200 token windows with a 15% overlap.
3.  **Anchoring**: The document title is prepended to each chunk to preserve context.
4.  **Normalization**: Vectors are L2-normalized to ensure mathematical consistency during Cosine Similarity searches.
