export interface Topic {
    name: string;
    count: number;
}

export interface Item {
    id: string;
    title: string;
    content: string;
    sourceType: string;
    sourceUrl?: string;
    saveAt: Date;
    tags: string[];
    parentId?: string | null;
    chunkIndex: number;
}

export interface SearchResult extends Item {
    distance: number;
    relevance?: number;
}

export type CreateItemDto = Omit<Item, 'id' | 'saveAt'>;

export type UpdateItemDto = Partial<CreateItemDto>;
