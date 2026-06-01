import { Component, ElementRef, inject, signal, ViewChild, AfterViewChecked } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ChatService } from '../../../core/services/chat.service';
import { SearchResult } from '../../models/item.model';
import { ChatResponse } from '../../models/chat.model';

interface Message {
  text: string;
  sender: 'user' | 'bot';
  sources?: SearchResult[];
}

@Component({
  selector: 'app-chat-widget',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './chat-widget.html',
  styleUrl: './chat-widget.css'
})
export class ChatWidgetComponent implements AfterViewChecked {
  private readonly chatService = inject(ChatService);
  
  isOpen = signal(false);
  isLoading = signal(false);
  userInput = signal('');

  // Keep track of the current conversation ID for context (optional, can be set after first response)
  currentConversationId = signal<string | undefined>(undefined);

  messages = signal<Message[]>([
    { text: 'Hello! I am your Recall assistant. Ask me anything about your documents.', sender: 'bot' }
  ]);

  @ViewChild('scrollMe') private myScrollContainer!: ElementRef;

  ngAfterViewChecked() {
    this.scrollToBottom();
  }

  toggleChat() {
    this.isOpen.update(v => !v);
  }

  startNewChat(){
    this.currentConversationId.set(undefined);
    this.messages.set([
      { text: 'Hello! I am your Recall assistant. Ask me anything about your documents.', sender: 'bot' }
    ]);
  }

  async sendMessage() {
    const query = this.userInput().trim();
    if (!query || this.isLoading()) return;

    // Add user message
    this.messages.update(m => [...m, { text: query, sender: 'user' }]);
    this.userInput.set('');
    this.isLoading.set(true);

    // Send to backend(including the converstationId if we have one)
    this.chatService.sendChat({ 
      query,
      conversationId: this.currentConversationId() 
    }).subscribe({
      next: (res: ChatResponse) => {

        // Store the ID returned by backend so follow-ups work
        this.currentConversationId.set(res.conversationId);

        // Add AI response to UI
        this.messages.update(m => [...m, { 
          text: res.answer, 
          sender: 'bot',
          sources: res.sources
        }]);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Chat error:', err);
        this.messages.update(m => [...m, { 
          text: 'Sorry, I encountered an error connecting to the local AI service. Make sure the API and Ollama are running.', 
          sender: 'bot' 
        }]);
        this.isLoading.set(false);
      }
    });
  }

  private scrollToBottom(): void {
    try {
      this.myScrollContainer.nativeElement.scrollTop = this.myScrollContainer.nativeElement.scrollHeight;
    } catch (err) { }
  }
}
