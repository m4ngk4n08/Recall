import { Component, ElementRef, inject, signal, ViewChild, AfterViewChecked } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ChatService } from '../../../core/services/chat.service';
import { ChatResponse, SearchResult } from '../../models/item.model';

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

  async sendMessage() {
    const query = this.userInput().trim();
    if (!query || this.isLoading()) return;

    // Add user message
    this.messages.update(m => [...m, { text: query, sender: 'user' }]);
    this.userInput.set('');
    this.isLoading.set(true);

    this.chatService.sendChat({ query }).subscribe({
      next: (res: ChatResponse) => {
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
