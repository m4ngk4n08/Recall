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
  // Signal to store the list of past conversations
  showHistory = signal(false);
  pastConversations = signal<any[]>([]);

  messages = signal<Message[]>([
    { text: 'Hello! I am your Recall assistant. Ask me anything about your documents.', sender: 'bot' }
  ]);

  @ViewChild('scrollMe') private myScrollContainer!: ElementRef;

// Toggle and load chat history when the widget is opened
toggleHistory(){
  this.showHistory.update(v => !v);
  if(this.showHistory()) {
    this.chatService.getChatConversations().subscribe(list => 
      this.pastConversations.set(list)
    )
  }
}

// Switch conversations
loadConversation(id: string){
  this.currentConversationId.set(id);
  this.chatService.getChatHistory(id).subscribe(msgs => {
    this.messages.set(msgs.map(m => ({
      text: m.content,
      sender: m.role === 'user' ? 'user' : 'bot',
    })))
  })
}

// Fetch the list when the chat is opened
loadHistoryList(){
  this.chatService.getChatConversations().subscribe(list => {
    this.pastConversations.set(list);
  });
}

// Load a specific chat's message when clicked
selectConversations(id: string){
  this.currentConversationId.set(id);
  this.chatService.getChatHistory(id).subscribe(messages => {
    this.messages.set(messages.map(m => ({
      text: m.content,
      sender: m.role === 'user' ? 'user' : 'bot'
    })))
  })
}

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
        // Check if this is a brand new conversation
        const isNewConversation = !this.currentConversationId();
        // Store the ID returned by backend so follow-ups work
        this.currentConversationId.set(res.conversationId);

        // If it was new conversation, refresh the sidebar list immidiately so user can see it
        if(isNewConversation){
          this.loadHistoryList();
        }

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
