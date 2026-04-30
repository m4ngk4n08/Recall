import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { ItemService } from '../../core/services/item.service';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-url-ingestor',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './url-ingestor.html',
  styleUrl: './url-ingestor.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UrlIngestor {
    private readonly itemService = inject(ItemService);
    private readonly fb = inject(FormBuilder);

    readonly isLoading = signal(false);
    readonly successMessage = signal<string | null>(null);
    readonly errorMessage = signal<string | null>(null);

    readonly ingestForm = this.fb.group({
      url: ['', [Validators.required, Validators.pattern('https?://.+')]],
    });

    onSubmit() {
      if(this.ingestForm.invalid) return;

      const url = this.ingestForm.value.url!;
      this.isLoading.set(true);
      this.successMessage.set(null);
      this.errorMessage.set(null);

      this.itemService.ingestUrl(url).subscribe({
        next: (response) => {
          this.successMessage.set("Ingestion started. Job ID: ${response.jobId.slice(0, 8)} ..");
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
