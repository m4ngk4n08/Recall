1. YouTube & Video Support (not yet started)
  Past a YouTube link, and the app fetches the transcript to index it.
   * Why: Video content is a massive source of information that is hard to "search" through manually.
   * Implementation: Add a service to fetch transcripts (using youtube-transcript-api or similar) and feed them
     into your existing ExtractionService.

  2. Multi-Format File Uploads (not yet started)
  A "Drop Zone" to upload PDF, Word (.docx), and Markdown files directly.
   * Why: Not all information is on the web. Local PDFs and documents are usually where the "heavy" knowledge
     lives.
   * Implementation: Enhance the ExtractionService with libraries like iTextSharp (PDF) or DocumentFormat.OpenXml
     (Word).

  3. "Thought Dump" / Quick Notes (not yet started)
  A simple "Scratchpad" where you can type or paste text directly without needing a URL.
   * Why: Perfect for capturing fleeting ideas or copy-pasting snippets from apps that don't have a public URL
     (like Slack or Teams).
   * Implementation: A "Quick Ingest" UI component that sends raw text to the IngestionService.

  4. AI-Powered "Peek" (Citations)(not yet started)
  When the AI answers in the chat, let the user click a source to see the exact paragraph used for the answer.
   * Why: This builds trust. Users can verify that the AI isn't "hallucinating" by seeing the evidence.
   * Implementation: Update the RAG logic to return the specific text chunks found by Pgvector and display them in
     a "Source Preview" modal.

  5. Automatic Summarization(not yet started)
  As soon as you save a link or file, the AI generates a 2-3 sentence summary for the Dashboard card.
   * Why: Helps you quickly remember what a saved item is about without re-reading it.
   * Implementation: Add a background task that calls Ollama with a "summarize" prompt right after ingestion.