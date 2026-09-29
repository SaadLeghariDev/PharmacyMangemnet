import { Component, EventEmitter, HostListener, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-modal',
  standalone: true,
  imports: [CommonModule],
  template: `
    @if (open) {
      <div class="overlay" (click)="onBackdrop()" role="presentation">
        <div
          class="dialog"
          [class.sm]="size === 'sm'"
          [class.lg]="size === 'lg'"
          role="dialog"
          aria-modal="true"
          [attr.aria-label]="title || 'Dialog'"
          (click)="$event.stopPropagation()"
        >
          <header>
            <h2>{{ title }}</h2>
            <button type="button" class="close" aria-label="Close" (click)="closed.emit()">×</button>
          </header>
          <div class="body">
            <ng-content />
          </div>
          @if (showFooter) {
            <footer>
              <ng-content select="[modal-footer]" />
            </footer>
          }
        </div>
      </div>
    }
  `,
  styles: [
    `
      .overlay {
        position: fixed;
        inset: 0;
        z-index: var(--z-modal);
        display: grid;
        place-items: center;
        padding: 1rem;
        background: rgba(15, 23, 42, 0.4);
        animation: fade 0.12s ease;
      }
      .dialog {
        width: min(520px, 100%);
        background: var(--color-surface);
        border: 1px solid var(--color-border);
        border-radius: var(--radius-lg);
        box-shadow: var(--shadow-md);
        animation: rise 0.15s ease;
        overflow: visible;
      }
      .dialog.sm {
        width: min(400px, 100%);
      }
      .dialog.lg {
        width: min(720px, 100%);
      }
      header {
        display: flex;
        align-items: center;
        justify-content: space-between;
        gap: 0.75rem;
        padding: 0.9rem 1rem;
        border-bottom: 1px solid var(--color-border);
      }
      h2 {
        font-size: 1rem;
      }
      .close {
        border: 0;
        background: transparent;
        font-size: 1.35rem;
        line-height: 1;
        color: var(--color-text-secondary);
        cursor: pointer;
      }
      .body {
        padding: 1rem;
        overflow: visible;
      }
      footer {
        display: flex;
        justify-content: flex-end;
        gap: 0.5rem;
        padding: 0.75rem 1rem;
        border-top: 1px solid var(--color-border);
      }
      @keyframes fade {
        from {
          opacity: 0;
        }
        to {
          opacity: 1;
        }
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
export class AppModalComponent {
  @Input() open = false;
  @Input() title = '';
  @Input() size: 'sm' | 'md' | 'lg' = 'md';
  @Input() closeOnBackdrop = true;
  @Input() showFooter = true;
  @Output() readonly closed = new EventEmitter<void>();

  @HostListener('document:keydown.escape')
  onEsc(): void {
    if (this.open) this.closed.emit();
  }

  onBackdrop(): void {
    if (this.closeOnBackdrop) this.closed.emit();
  }
}
