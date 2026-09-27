import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-loading-state',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="loading" [attr.aria-busy]="true" role="status">
      @if (variant === 'table') {
        <div class="skel-row" *ngFor="let _ of rows"></div>
      } @else if (variant === 'inline') {
        <span class="spinner" aria-hidden="true"></span>
        <span>{{ label }}</span>
      } @else {
        <div class="block">
          <span class="spinner" aria-hidden="true"></span>
          <span>{{ label }}</span>
        </div>
      }
    </div>
  `,
  styles: [
    `
      .loading {
        color: var(--color-text-secondary);
        font-size: 0.875rem;
      }
      .block {
        display: flex;
        align-items: center;
        justify-content: center;
        gap: 0.6rem;
        padding: 2rem 1rem;
        border: 1px solid var(--color-border);
        border-radius: var(--radius-lg);
        background: var(--color-surface);
      }
      .skel-row {
        height: 2.5rem;
        margin-bottom: 0.5rem;
        border-radius: var(--radius-md);
        background: linear-gradient(90deg, #f1f5f9 25%, #e2e8f0 50%, #f1f5f9 75%);
        background-size: 200% 100%;
        animation: shimmer 1.2s linear infinite;
      }
      .spinner {
        width: 1rem;
        height: 1rem;
        border: 2px solid var(--color-border);
        border-top-color: var(--color-primary);
        border-radius: 50%;
        animation: spin 0.6s linear infinite;
      }
      @keyframes spin {
        to {
          transform: rotate(360deg);
        }
      }
      @keyframes shimmer {
        to {
          background-position: -200% 0;
        }
      }
    `,
  ],
})
export class AppLoadingStateComponent {
  @Input() variant: 'page' | 'table' | 'inline' = 'page';
  @Input() label = 'Loading…';
  @Input() rowCount = 5;

  get rows(): number[] {
    return Array.from({ length: this.rowCount }, (_, i) => i);
  }
}
