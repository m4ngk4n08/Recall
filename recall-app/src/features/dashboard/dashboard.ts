import { ChangeDetectionStrategy, Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { ItemService } from '../../core/services/item.service';
import { Item } from '../../app/models/item.model';
import { DatePipe, SlicePipe } from '@angular/common';

@Component({
  selector: 'app-dashboard',
  imports: [ReactiveFormsModule, FormsModule, SlicePipe, DatePipe],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Dashboard implements OnInit {
  private readonly itemService = inject(ItemService);
  private readonly fb = inject(FormBuilder);

  // Signals for state
  private readonly itemsSignal = signal<Item[]>([]);
  readonly items = this.itemsSignal.asReadonly();
  readonly isLoading = signal(false);
  readonly error = signal<string | null>(null);
  readonly editingId = signal<string | null>(null); // Track which item is being edited


  // Derive state: count of items
  readonly itemCount = computed(() => this.items().length);

  // Reactive form for new items
  readonly itemForm = this.fb.group({
    title: ['', [Validators.required, Validators.minLength(3)]],
    content: ['', Validators.required],
    sourceType: ['note'],
    tags: [[] as string[]] // will be handled with comma-separated input
  });

  editForm = this.fb.group({
    title: ['', [Validators.required, Validators.minLength(3)]],
    content: ['', Validators.required],
    sourceType: ['note'],
    tags: [[] as string[]]
  })

  // Helper for tag input (string -> array)
  tagInputValue = '';
  editTagInputValue = '';

  ngOnInit() {
    this.loadItems();
  }

  loadItems(): void {
    this.isLoading.set(true);
    this.error.set(null);
    this.itemService.getAll().subscribe({
      next: (items) => {
        this.itemsSignal.set(items);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.error.set('Failed to load items: ' + err.message);
        this.isLoading.set(false);
      }
    });
  }

  addItem(): void {
    if(this.itemForm.invalid) return;

    const formValue = this.itemForm.value;
    const newItem = {
      title: formValue.title!,
      content: formValue.content!,
      sourceType: formValue.sourceType!,
      tags: this.tagInputValue ? this.tagInputValue.split(',').map(tag => tag.trim()) : []
    };

    this.isLoading.set(true);
    this.itemService.create(newItem).subscribe({
      next: (created) => {
        // Update signal immutably
        this.itemsSignal.update(items => [created, ...items]);
        this.itemForm.reset({ sourceType: 'note', tags: [] });
        this.tagInputValue = '';
        this.isLoading.set(false);
      },
      error: (err) => {
        this.error.set('Failed to create item: ' + err.message);
        this.isLoading.set(false);
      }
    });
  };

  startEdit(item: Item): void {
    this.editingId.set(item.id);
    this.editForm.setValue({
      title: item.title,
      content: item.content,
      sourceType: item.sourceType,
      tags: item.tags
    });

    this.editTagInputValue = item.tags.join(', ');
  };
  
  cancelEdit(): void {
    this.editingId.set(null);
    this.editForm.reset();
    this.editTagInputValue = '';
  };

  updateItem(id: string): void {
    if(this.editForm.invalid) return;

    const formValue = this.editForm.value;
    const updatedTags = this.editTagInputValue ? this.editTagInputValue.split(',').map(tag => tag.trim()) : [];
    const updatedItem = {
      title: formValue.title!,
      content: formValue.content!,
      sourceType: formValue.sourceType!,
      tags: updatedTags
    };

    this.isLoading.set(true);
    this.itemService.update(id, updatedItem).subscribe({
      next: (updated) => {
        // update signal immutably
        this.itemsSignal.update(items =>
          items.map(item => item.id === id ? updated : item)
        );
        this.cancelEdit();
        this.isLoading.set(false);
      },
      error: (err) => {
        this.error.set('Failed to update item: ' + err.message);
        this.isLoading.set(false);
      }
    });
  }

  deleteItem(id: string, title: string): void {
    if(confirm(`Are you sure you want to delete "${title}"?`)){
      this.isLoading.set(true);
      this.itemService.delete(id).subscribe({
        next: () => {
          this.itemsSignal.update(items => items.filter(item => item.id !== id));
          this.isLoading.set(false);
        },
        error: (err) => {
          this.error.set('Failed to delete item: ' + err.message);
          this.isLoading.set(false);
        }
      });
    }
  }
}
