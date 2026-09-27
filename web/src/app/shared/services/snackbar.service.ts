import { Injectable, signal } from '@angular/core';

export type SnackbarTone = 'success' | 'warning' | 'error' | 'info';

export interface SnackbarMessage {
  id: number;
  tone: SnackbarTone;
  text: string;
  durationMs: number;
}

@Injectable({ providedIn: 'root' })
export class SnackbarService {
  private seq = 0;
  readonly messages = signal<SnackbarMessage[]>([]);

  show(text: string, tone: SnackbarTone = 'info', durationMs = 4000): void {
    const id = ++this.seq;
    this.messages.update((list) => [...list, { id, tone, text, durationMs }]);
    if (durationMs > 0) {
      window.setTimeout(() => this.dismiss(id), durationMs);
    }
  }

  success(text: string): void {
    this.show(text, 'success');
  }

  warning(text: string): void {
    this.show(text, 'warning');
  }

  error(text: string): void {
    this.show(text, 'error', 6000);
  }

  info(text: string): void {
    this.show(text, 'info');
  }

  dismiss(id: number): void {
    this.messages.update((list) => list.filter((m) => m.id !== id));
  }
}
