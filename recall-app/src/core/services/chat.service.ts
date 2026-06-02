import { HttpClient } from "@angular/common/http";
import { inject, Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { API_ENDPOINTS } from "../../app/api.config";
import { ChatMessageDto, ChatRequest, ChatResponse } from "../../app/models/chat.model";

@Injectable({ providedIn: "root" })
export class ChatService {
        private readonly http = inject(HttpClient);
        private readonly apiUrl = API_ENDPOINTS.chat;

    sendChat(request: ChatRequest): Observable<ChatResponse> {
        return this.http.post<ChatResponse>(`${this.apiUrl}/chat`, request);
    }

    // Fetches history for a specific conversation
    getChatHistory(conversationId: string): Observable<ChatMessageDto[]> {
        return this.http.get<ChatMessageDto[]>(`${this.apiUrl}/history/${conversationId}`);
    }

    getChatConversations(): Observable<{id: string, title: string}[]> {
        return this.http.get<{id: string, title: string}[]>(`${this.apiUrl}/conversations`);
    }
}
