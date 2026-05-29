import { HttpClient } from "@angular/common/http";
import { inject, Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { ChatRequest, ChatResponse } from "../../app/models/item.model";
import { API_ENDPOINTS } from "../../app/api.config";

@Injectable({ providedIn: "root" })
export class ChatService {
        private readonly http = inject(HttpClient);
        private readonly apiUrl = API_ENDPOINTS.chat;

    sendChat(request: ChatRequest): Observable<ChatResponse> {
        return this.http.post<ChatResponse>(`${this.apiUrl}/chat`, request);
    }
}
