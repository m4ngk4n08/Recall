import { HttpClient } from "@angular/common/http";
import { inject, Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { CreateItemDto, Item, SearchResult } from "../../app/models/item.model";


@Injectable({ providedIn: "root" })
export class ItemService{
    private readonly http = inject(HttpClient);
    private readonly apiUrl = "http://localhost:5073/api/items";

    getAll(): Observable<Item[]>{
        return this.http.get<Item[]>(this.apiUrl);
    }

    create(item: CreateItemDto): Observable<Item>{
        return this.http.post<Item>(this.apiUrl, item);
    }

    update(id: string, item: Partial<Item>): Observable<Item>{
        return this.http.put<Item>(`${this.apiUrl}/${id}`, item);
    }

    delete(id: string): Observable<void>{
        return this.http.delete<void>(`${this.apiUrl}/${id}`);
    }

    ingestUrl(url: string): Observable<{ jobId: string; message: string}>{
        return this.http.post<{ jobId: string; message: string }>(`${this.apiUrl}/ingest-url`, { url });
    }

    search(query: string, limit: number = 10): Observable<SearchResult[]>{
        return this.http.get<SearchResult[]>(`${this.apiUrl}/search`, { params: {q: query, limit } });
    }
}