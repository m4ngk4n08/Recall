// fix file processing

&#x09;- read file

&#x09;- need to parse other file not just pdf.

// fix video parsing

&#x09;- read transcript



reorganize the chunk card interface



Context: We are in the middle of a step-by-step implementation to enable robust PDF extraction in the

&#x20; Recall.Api project. The goal is to allow the application to extract text from PDFs whether they are provided

&#x20; via a URL or a local file upload.



&#x20; Current Status:

&#x20;  \* Step 1 (Completed): The IExtractionService interface has been updated to include:

&#x20;     Task<(string Title, string Content, string SourceType)> ExtractPdfAsync(Stream stream, string fileName);

&#x20;  \* Step 2 (Completed): Implement the logic for this method in ExtractionService.cs.



&#x20; Objective for Resumption:

&#x20; Please guide me through Step 2 of the implementation. I am the one writing the code, so explain the logic and

&#x20; provide the code snippets for me to apply.



&#x20; Technical Details to remember:

&#x20;  \* We are using the PdfPig library.

&#x20;  \* We need to handle non-seekable streams by copying to a MemoryStream.

&#x20;  \* We should use StringBuilder and AppendLine for performance and to ensure words don't merge across page

&#x20;    breaks.

