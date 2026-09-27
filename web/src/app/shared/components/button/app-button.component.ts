import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';

export type AppButtonVariant = 'primary' | 'secondary' | 'ghost' | 'danger';
export type AppButtonSize = 'sm' | 'md';

@Component({
  selector: 'app-button',
  standalone: true,
  imports: [CommonModule],
  template: `
    <button
      [attr.type]="type"
      class="btn"
      [class.primary]="variant === 'primary'"
      [class.secondary]="variant === 'secondary'"
      [class.ghost]="variant === 'ghost'"
      [class.danger]="variant === 'danger'"
      [class.sm]="size === 'sm'"
      [class.block]="block"
      [disabled]="disabled || loading"
      (click)="clicked.emit($event)"
    >
      @if (loading) {
        <span class="spinner" aria-hidden="true"></span>
      }
      <ng-content />
    </button>
  `,
  styles: [
    `
      .btn {
        display: inline-flex;
        align-items: center;
        justify-content: center;
        gap: 0.4rem;
        min-height: var(--control-height);
        padding: 0 0.9rem;
        border-radius: var(--radius-md);
        border: 1px solid transparent;
        font-weight: 600;
        font-size: 0.875rem;
        cursor: pointer;
        background: transparent;
        transition: background 0.12s ease, border-color 0.12s ease, color 0.12s ease;
      }
      .btn.sm {
        min-height: 30px;
        padding: 0 0.65rem;
        font-size: 0.8125rem;
      }
      .btn.block {
        width: 100%;
      }
      .btn:disabled {
        opacity: 0.55;
        cursor: not-allowed;
      }
      .primary {
        background: var(--color-primary);
        color: #fff;
      }
      .primary:hover:not(:disabled) {
        background: var(--color-primary-dark);
      }
      .secondary {
        background: var(--color-surface);
        border-color: var(--color-border);
        color: var(--color-text);
      }
      .secondary:hover:not(:disabled) {
        background: var(--color-bg);
      }
      .ghost {
        color: var(--color-primary);
      }
      .ghost:hover:not(:disabled) {
        background: var(--color-primary-light);
      }
      .danger {
        background: var(--color-danger);
        color: #fff;
      }
      .danger:hover:not(:disabled) {
        filter: brightness(0.92);
      }
      .spinner {
        width: 0.85rem;
        height: 0.85rem;
        border: 2px solid rgba(255, 255, 255, 0.35);
        border-top-color: #fff;
        border-radius: 50%;
        animation: spin 0.6s linear infinite;
      }
      .secondary .spinner,
      .ghost .spinner {
        border-color: rgba(15, 118, 110, 0.25);
        border-top-color: var(--color-primary);
      }
      @keyframes spin {
        to {
          transform: rotate(360deg);
        }
      }
    `,
  ],
})
export class AppButtonComponent {
  @Input() variant: AppButtonVariant = 'primary';
  @Input() size: AppButtonSize = 'md';
  @Input() type: 'button' | 'submit' | 'reset' = 'button';
  @Input() disabled = false;
  @Input() loading = false;
  @Input() block = false;
  @Output() readonly clicked = new EventEmitter<MouseEvent>();
}
