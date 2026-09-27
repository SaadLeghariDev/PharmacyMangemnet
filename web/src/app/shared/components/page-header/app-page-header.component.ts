import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-page-header',
  standalone: true,
  imports: [CommonModule],
  template: `
    <header class="page-header">
      <div class="titles">
        <h1>{{ title }}</h1>
        @if (subtitle) {
          <p>{{ subtitle }}</p>
        }
      </div>
      <div class="actions">
        <ng-content />
      </div>
    </header>
  `,
  styles: [
    `
      .page-header {
        display: flex;
        align-items: flex-start;
        justify-content: space-between;
        gap: 1rem;
        margin-bottom: 1rem;
      }
      h1 {
        font-size: 1.25rem;
        font-weight: 600;
      }
      p {
        margin-top: 0.2rem;
        color: var(--color-text-secondary);
        font-size: 0.875rem;
      }
      .actions {
        display: flex;
        flex-wrap: wrap;
        gap: 0.5rem;
        align-items: center;
      }
    `,
  ],
})
export class AppPageHeaderComponent {
  @Input({ required: true }) title!: string;
  @Input() subtitle = '';
}
