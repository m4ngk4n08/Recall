import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { IngestService } from '../../core/services/ingest.service';

@Component({
  selector: 'app-url-ingestor',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './url-ingestor.html',
  styleUrl: './url-ingestor.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UrlIngestor {
    private readonly ingestService = inject(IngestService);
    private readonly fb = inject(FormBuilder);

    readonly isLoading = signal(false);
    readonly successMessage = signal<string | null>(null);
    readonly errorMessage = signal<string | null>(null);

    readonly ingestForm = this.fb.group({
      url: ['', [Validators.required, Validators.pattern('https?://.+')]],
      tags: [''],
    });

    onSubmit() {
      if(this.ingestForm.invalid) return;

      const { url, tags } = this.ingestForm.value;
      const tagList = tags ? tags.split(',').map(t => t.trim()).filter(t => t.length > 0) : [];

      this.isLoading.set(true);
      this.successMessage.set(null);
      this.errorMessage.set(null);

      this.ingestService.ingestUrl(url!, tagList).subscribe({
        next: (response) => {
          this.successMessage.set(`Ingestion started. Job ID: ${response.jobId.slice(0, 8)} ..`);
          this.ingestForm.reset();
          this.isLoading.set(false);
        },
        error: (err) => {
          this.errorMessage.set("Failed to start ingestion: " + (err.error?.message || err.message || "Unknown error"));
          this.isLoading.set(false);
        },
      });
    }
}
