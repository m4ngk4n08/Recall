// fix file processing

&#x09;- read file

&#x09;- need to parse other file not just pdf.

// fix video parsing

&#x09;- read transcript



reorganize the chunk card interface(done)



// add ingestion for a note dump or thought dump.

// login and registration


// chat history

// remember conversation

&#x20;Chat History \& Memory: Plan and Explanation



&#x20; Currently, your "Recall" app is stateless. This means every time you ask a question, the AI forgets who you are

&#x20; and what you just said. Implementing memory turns it into a Conversation.

&#x20; 1. The Impact Analysis



&#x20; Implementing chat history has three main impacts:



&#x20;  \* Database (Persistence): You will need a new table to store ChatMessages. Without this, history disappears if

&#x20;    the user refreshes the page.

&#x20;  \* LLM Context (The "Memory"): Every time you send a new message, you will now send a summary or a list of the

&#x20;    last few messages back to the AI. This uses more "tokens" (computational units), so very long conversations

&#x20;    might get slower.

&#x20;  \* User Experience (UX): The UI will feel more natural. You can ask follow-up questions like "Tell me more about

&#x20;    that" or "What was the third point again?" without repeating the original topic.



&#x20; ---



&#x20; 2. The Plan (Step-by-Step)



&#x20; Phase A: Backend (The Infrastructure)

&#x20;  1. Create a ChatMessage Model:

&#x20;      \* Fields: Id, ConversationId (to group messages), Role (User or Assistant), Content, Timestamp.

&#x20;  2. Update AppDbContext: Add a DbSet<ChatMessage> and run a migration.

&#x20;  3. Update ChatController:

&#x20;      \* Accept a ConversationId in the request.

&#x20;      \* Before calling the AI, fetch the last 5-10 messages from the database for that ConversationId.

&#x20;  4. Update OllamaService:

&#x20;      \* Modify the prompt to include the "Conversation History" before the "Context" and "Question".



&#x20; Phase B: Frontend (The Interface)

&#x20;  1. Create a Chat UI: A scrolling list of messages (User on the right, AI on the left).

&#x20;  2. State Management: Store a currentConversationId (generate a new one when the user clicks "New Chat").

&#x20;  3. Service Update: Modify ChatService to handle the ConversationId.



&#x20; ---



&#x20; 3. Detailed Explanation (How it works)



&#x20; Think of the AI as a librarian with a very short memory.



&#x20; Current Flow (No Memory):

&#x20;  1. User: "Who is Einstein?"

&#x20;  2. App: Goes to the database, finds Einstein info, tells the AI.

&#x20;  3. AI: "He was a physicist."

&#x20;  4. User: "When was he born?"

&#x20;  5. AI: "I don't know who 'he' is." (Because the AI reset after step 3).



&#x20; New Flow (With Memory):

&#x20;  1. User: "Who is Einstein?"

&#x20;  2. App: Stores "User: Who is Einstein?" in the database.

&#x20;  3. AI: "He was a physicist."

&#x20;  4. App: Stores "Assistant: He was a physicist." in the database.

&#x20;  5. User: "When was he born?"

&#x20;  6. App: Gathers history:

&#x20;      \* History: User asked about Einstein. Assistant said he was a physicist.

&#x20;      \* New Question: When was he born?

&#x20;  7. AI: "Einstein was born in 1879." (The AI now knows 'he' refers to the Einstein mentioned in the history).



&#x20; Key Technical Trick: The "Sliding Window"

&#x20; We don't send every message ever sent (that would eventually be too much data). We usually send the last 5 or 10

&#x20; messages. This gives the AI enough context for a natural conversation without overwhelming the system.

