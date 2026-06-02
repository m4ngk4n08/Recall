import { HttpClient } from "@angular/common/http";
import { inject } from "@angular/core";
import { Observable } from "rxjs/internal/Observable";
import { API_ENDPOINTS } from "../../app/api.config";


export class IngestService{
    private readonly http = inject(HttpClient);
    private readonly apiIngestUrl = API_ENDPOINTS.ingest;

    ingestUrl(url: string, tags: string[] = []): Observable<{ jobId: string; message: string}>{
        return this.http.post<{ jobId: string; message: string }>(`${this.apiIngestUrl}/url`, { url, tags });
    }

    ingestThought(title: string, content: string, tags: string[] = []) : Observable<{ jobId: string; message: string }> {
        return this.http.post<{ jobId: string; message: string }>(`${this.apiIngestUrl}/thought`, 
            { 
                title, 
                content, 
                tags 
            });
    }
}