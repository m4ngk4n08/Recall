import { SearchResult } from "./item.model";

export interface ChatRequest{
    query: string;
    model?: string;
    conversationId?: string;
}

export interface ChatResponse{
    answer: string;
    sources: SearchResult[];
    conversationId: string;
}

export interface ChatMessageDto{
    role: 'user' | 'assistant';
    content: string;
    timestamp: string;
}