import { HttpClient } from "@angular/common/http";
import { inject, Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { CreateItemDto, Item, SearchResult, Topic } from "../../app/models/item.model";
import { API_ENDPOINTS } from "../../app/api.config";


@Injectable({ providedIn: "root" })
export class ItemService{
    private readonly http = inject(HttpClient);
    private readonly apiItemUrl = API_ENDPOINTS.items;
    private readonly apiIngestUrl = API_ENDPOINTS.ingest;

    getAll(): Observable<Item[]>{
        return this.http.get<Item[]>(`${this.apiItemUrl}/getall`);
    }

    getTopics(): Observable<Topic[]>{
        return this.http.get<Topic[]>(`${this.apiItemUrl}/topics`);
    }

    getByTag(tag: string): Observable<Item[]>{
        return this.http.get<Item[]>(`${this.apiItemUrl}/tag/${tag}`);
    }

    create(item: CreateItemDto): Observable<Item>{
        return this.http.post<Item>(this.apiItemUrl, item);
    }

    update(id: string, item: Partial<Item>): Observable<Item>{
        return this.http.put<Item>(`${this.apiItemUrl}/${id}`, item);
    }

    delete(id: string): Observable<void>{
        return this.http.delete<void>(`${this.apiItemUrl}/${id}`);
    }

    ingestUrl(url: string, tags: string[] = []): Observable<{ jobId: string; message: string}>{
        return this.http.post<{ jobId: string; message: string }>(`${this.apiIngestUrl}/url`, { url, tags });
    }

    search(query: string, limit: number = 10): Observable<SearchResult[]>{
        return this.http.get<SearchResult[]>(`${this.apiItemUrl}/search`, { params: {q: query, limit } });
    }
}