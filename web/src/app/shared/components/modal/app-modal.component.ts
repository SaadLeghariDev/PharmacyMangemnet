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
        padding-bottom: max(1rem, env(safe-area-inset-bottom));
        background: rgba(15, 23, 42, 0.45);
        animation: fade 0.12s ease;
        overflow: auto;
        -webkit-overflow-scrolling: touch;
      }
      .dialog {
        display: flex;
        flex-direction: column;
        width: min(520px, 100%);
        max-height: min(90vh, 900px);
        background: var(--color-surface);
        border: 1px solid var(--color-border);
        border-radius: var(--radius-lg);
        box-shadow: var(--shadow-md);
        animation: rise 0.15s ease;
        overflow: hidden;
        margin: auto;
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
        flex-shrink: 0;
        background: var(--color-surface);
      }
      h2 {
        margin: 0;
        font-size: 1.05rem;
        font-weight: 650;
        color: var(--color-text);
        line-height: 1.3;
      }
      .close {
        display: grid;
        place-items: center;
        width: 2.25rem;
        height: 2.25rem;
        border: 0;
        border-radius: var(--radius-md);
        background: transparent;
        font-size: 1.45rem;
        line-height: 1;
        color: var(--color-text-secondary);
        cursor: pointer;
        flex-shrink: 0;
      }
      .close:hover {
        background: var(--color-bg);
        color: var(--color-text);
      }
      .close:focus-visible {
        outline: 2px solid var(--color-primary);
        outline-offset: 1px;
      }
      .body {
        padding: 1rem 1.1rem;
        overflow-x: hidden;
        overflow-y: auto;
        flex: 1 1 auto;
        min-height: 0;
        -webkit-overflow-scrolling: touch;
      }
      footer {
        display: flex;
        justify-content: flex-end;
        flex-wrap: wrap;
        gap: 0.5rem;
        padding: 0.75rem 1rem;
        border-top: 1px solid var(--color-border);
        flex-shrink: 0;
        background: var(--color-surface);
      }
      footer:not(:has(*)) {
        display: none;
        padding: 0;
        border: 0;
      }
      @media (max-width: 640px) {
        .overlay {
          padding: 0.5rem;
          padding-bottom: max(0.5rem, env(safe-area-inset-bottom));
          align-items: end;
          place-items: end center;
        }
        .dialog,
        .dialog.sm,
        .dialog.lg {
          width: 100%;
          max-height: min(92vh, 100%);
          border-radius: var(--radius-lg) var(--radius-lg) 0 0;
          margin: 0;
        }
        header {
          padding: 0.85rem 0.9rem;
        }
        .body {
          padding: 0.9rem;
        }
        footer {
          padding: 0.75rem 0.9rem max(0.75rem, env(safe-area-inset-bottom));
        }
        .close {
          width: 2.5rem;
          height: 2.5rem;
        }
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
          transform: translateY(8px);
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
