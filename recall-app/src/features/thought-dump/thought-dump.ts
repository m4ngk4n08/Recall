import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { IngestService } from '../../core/services/ingest.service';

@Component({
  selector: 'app-thought-dump',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './thought-dump.html',
  styleUrl: './thought-dump.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ThoughtDump {
  private readonly ingestService = inject(IngestService);
  private readonly fb = inject(FormBuilder);

  readonly isLoading = signal(false);
  readonly successMessage = signal<string | null>(null);
  readonly errorMessage = signal<string | null>(null);
  
  readonly thoughtForm = this.fb.group({
    title: [''],
    content: [''],
    tags: [''],
  })

  onSubmit() {
    if(this.thoughtForm.invalid || this.isLoading()) return;

    const { title, content, tags } = this.thoughtForm.value;
    const tagList = tags ? tags.split(',').map(t => t.trim()).filter(t => t.length > 0) : [];

    this.isLoading.set(true);
    this.successMessage.set(null);
    this.errorMessage.set(null);

    this.ingestService.ingestThought(title || '', content!, tagList).subscribe({
      next: (response) => {
        this.successMessage.set("Thought saved and indexed.");
        this.thoughtForm.reset();
        this.isLoading.set(false);
      },
      error: (err) => {
        this.errorMessage.set("Failed to save thought. Please try again.");
        this.isLoading.set(false);
      }
    })
  }
  
}
