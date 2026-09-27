import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AppButtonComponent } from '../button/app-button.component';

@Component({
  selector: 'app-empty-state',
  standalone: true,
  imports: [CommonModule, AppButtonComponent],
  template: `
    <div class="empty">
      <h3>{{ title }}</h3>
      <p>{{ message }}</p>
      @if (actionLabel) {
        <app-button variant="secondary" size="sm" (clicked)="action.emit()">{{
          actionLabel
        }}</app-button>
      }
    </div>
  `,
  styles: [
    `
      .empty {
        display: grid;
        justify-items: center;
        gap: 0.4rem;
        text-align: center;
        padding: 2.5rem 1rem;
        border: 1px dashed var(--color-border);
        border-radius: var(--radius-lg);
        background: var(--color-surface);
      }
      h3 {
        font-size: 0.95rem;
      }
      p {
        max-width: 28rem;
        color: var(--color-text-secondary);
        font-size: 0.875rem;
        margin-bottom: 0.35rem;
      }
    `,
  ],
})
export class AppEmptyStateComponent {
  @Input() title = 'Nothing here yet';
  @Input() message = 'Try adjusting filters or add a new record.';
  @Input() actionLabel = '';
  @Output() readonly action = new EventEmitter<void>();
}
