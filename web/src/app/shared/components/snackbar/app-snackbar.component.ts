import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SnackbarService } from '../../services/snackbar.service';

@Component({
  selector: 'app-snackbar',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="stack" aria-live="polite">
      @for (msg of snackbar.messages(); track msg.id) {
        <div class="toast" [attr.data-tone]="msg.tone" role="status">
          <span class="icon" aria-hidden="true">{{ icon(msg.tone) }}</span>
          <span class="text">{{ msg.text }}</span>
          <button type="button" class="dismiss" aria-label="Dismiss" (click)="snackbar.dismiss(msg.id)">
            ×
          </button>
        </div>
      }
    </div>
  `,
  styles: [
    `
      .stack {
        position: fixed;
        right: 1rem;
        bottom: 1rem;
        z-index: var(--z-snackbar);
        display: grid;
        gap: 0.5rem;
        width: min(360px, calc(100vw - 2rem));
      }
      .toast {
        display: grid;
        grid-template-columns: auto 1fr auto;
        gap: 0.55rem;
        align-items: start;
        padding: 0.75rem 0.85rem;
        border-radius: var(--radius-md);
        border: 1px solid var(--color-border);
        background: var(--color-surface);
        box-shadow: var(--shadow-md);
        animation: rise 0.15s ease;
      }
      .toast[data-tone='success'] {
        border-color: #bbf7d0;
        background: #f0fdf4;
      }
      .toast[data-tone='warning'] {
        border-color: #fde68a;
        background: #fffbeb;
      }
      .toast[data-tone='error'] {
        border-color: #fecaca;
        background: #fef2f2;
      }
      .toast[data-tone='info'] {
        border-color: #bfdbfe;
        background: #eff6ff;
      }
      .icon {
        font-weight: 700;
        color: var(--color-text-secondary);
      }
      .text {
        font-size: 0.875rem;
        color: var(--color-text);
      }
      .dismiss {
        border: 0;
        background: transparent;
        color: var(--color-text-secondary);
        cursor: pointer;
        font-size: 1.1rem;
        line-height: 1;
      }
      @keyframes rise {
        from {
          opacity: 0;
          transform: translateY(6px);
        }
        to {
          opacity: 1;
          transform: translateY(0);
        }
      }
    `,
  ],
})
export class AppSnackbarComponent {
  readonly snackbar = inject(SnackbarService);

  icon(tone: string): string {
    switch (tone) {
      case 'success':
        return '✓';
      case 'warning':
        return '!';
      case 'error':
        return '✕';
      default:
        return 'i';
    }
  }
}
