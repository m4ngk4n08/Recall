import { ChangeDetectionStrategy, Component, computed, inject, OnInit, signal, effect } from '@angular/core';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { ItemService } from '../../core/services/item.service';
import { Item, SearchResult, Topic } from '../../app/models/item.model';
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
  
  private readonly topicsSignal = signal<Topic[]>([]);
  readonly topics = this.topicsSignal.asReadonly();
  readonly selectedTopic = signal<string | null>(null);

  readonly isLoading = signal(false);
  readonly error = signal<string | null>(null);
  readonly editingId = signal<string | null>(null); // Track which item is being edited

  // Search signals
  readonly searchQuery = signal('');
  readonly searchResults = signal<SearchResult[]>([]);
  readonly isSearching = signal(false);
  readonly selectedItem = signal<Item | SearchResult | null>(null);

  // Add a signal to trach which titles are expanded
  readonly expandedTitles = signal<Set<string>>(new Set());
  
  // Create a computed signal to group items by title
  readonly groupedItems = computed(() => {
    const items = this.items();
    const groups = new Map<string, Item[]>();

    // Grouping chunks by title
    items.forEach(item => {
      const displayTitle = item.title.replace(/ - Chunk \d+$/, ''); // Remove chunk suffix for grouping
      if(!groups.has(displayTitle)){
        groups.set(displayTitle, []);
      }
      groups.get(displayTitle)!.push(item);
    });

    // Transform the map into an array of group objects
    return Array.from(groups.entries()).map(([title, chunks]) => {
      const sortedChunks = chunks.sort((a, b) => a.chunkIndex - b.chunkIndex);

      return {
        title,
        chunks: sortedChunks,
        // Collect unique tags across all chunks
        tags: Array.from(new Set(chunks.flatMap(chunk => chunk.tags))),
        sourceType: chunks[0].sourceType,
        isExpanded: this.expandedTitles().has(title)
      };
    });
  });

  // Derive state: count of groups (unique documents)
  readonly itemCount = computed(() => this.groupedItems().length);
  
  constructor() {
    // Basic search debouncing effect
    effect(() => {
      const query = this.searchQuery();
      if (query.length >= 3) {
        this.performSearch(query);
      } else {
        this.searchResults.set([]);
      }
    });

    // Effect to reload items when selectedTopic changes
    effect(() => {
      const topic = this.selectedTopic();
      this.loadItems(topic);
    }, { allowSignalWrites: true });
  }

  ngOnInit() {
    this.loadTopics();
  }

  loadTopics(): void {
    this.itemService.getTopics().subscribe({
      next: (topics) => this.topicsSignal.set(topics),
      error: (err) => console.error('Failed to load topics', err)
    });
  }

  selectTopic(topicName: string | null): void {
    this.selectedTopic.set(topicName);
  }

  performSearch(query: string): void {
    this.isSearching.set(true);
    this.itemService.search(query).subscribe({
      next: (results) => {
        // Map distance to relevance (assuming distance is 0-1 and lower is better)
        const mappedResults: SearchResult[] = results.map(r => ({
          ...r,
          relevance: Math.max(0, 1 - r.distance)
        }));
        this.searchResults.set(mappedResults);
        this.isSearching.set(false);
      },
      error: (err) => {
        this.error.set('Search failed: ' + err.message);
        this.isSearching.set(false);
      }
    });
  }

  selectSearchResult(result: SearchResult): void {
    this.selectedItem.set(result);
    this.searchQuery.set('');
    this.searchResults.set([]);
  }

  closeDetail(): void {
    this.selectedItem.set(null);
  }

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

  loadItems(tag: string | null = null): void {
    this.isLoading.set(true);
    this.error.set(null);
    
    const request = tag ? this.itemService.getByTag(tag) : this.itemService.getAll();
    
    request.subscribe({
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
      tags: this.tagInputValue ? this.tagInputValue.split(',').map(tag => tag.trim()) : [],
      chunkIndex: 0 // default value, backend will set actual index
    };

    this.isLoading.set(true);
    this.itemService.create(newItem).subscribe({
      next: (created) => {
        // Update signal immutably
        this.itemsSignal.update(items => [created, ...items]);
        this.itemForm.reset({ sourceType: 'note', tags: [] });
        this.tagInputValue = '';
        this.isLoading.set(false);
        this.loadTopics();
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
      tags: updatedTags,
      chunkIndex: 0 // default value, backend will set actual index

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
        this.loadTopics();
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
          this.loadTopics();
        },
        error: (err) => {
          this.error.set('Failed to delete item: ' + err.message);
          this.isLoading.set(false);
        }
      });
    }
  }

  // Method to toggle expansion
  toggleExpand(title: string): void{
    this.expandedTitles.update(prev => {
      const next = new Set(prev);
      if(next.has(title)){
        next.delete(title);
      } else {
        next.add(title);
      }
      return next;
    })
  }
}
