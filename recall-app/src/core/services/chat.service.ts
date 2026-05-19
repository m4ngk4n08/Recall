import { HttpClient } from "@angular/common/http";
import { inject, Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { ChatRequest, ChatResponse } from "../../app/models/item.model";

@Injectable({ providedIn: "root" })
export class ChatService {
    private readonly http = inject(HttpClient);
    private readonly apiUrl = "http://localhost:5073/api/items/chat";

    sendChat(request: ChatRequest): Observable<ChatResponse> {
        return this.http.post<ChatResponse>(this.apiUrl, request);
    }
}
