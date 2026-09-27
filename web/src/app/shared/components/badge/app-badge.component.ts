import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

export type AppBadgeTone = 'success' | 'warning' | 'danger' | 'info' | 'neutral' | 'primary';

@Component({
  selector: 'app-badge',
  standalone: true,
  imports: [CommonModule],
  template: `<span class="badge" [attr.data-tone]="tone"><ng-content /></span>`,
  styles: [
    `
      .badge {
        display: inline-flex;
        align-items: center;
        gap: 0.25rem;
        padding: 0.15rem 0.5rem;
        border-radius: 999px;
        font-size: 0.75rem;
        font-weight: 600;
        line-height: 1.3;
        white-space: nowrap;
      }
      .badge[data-tone='success'] {
        background: #dcfce7;
        color: #166534;
      }
      .badge[data-tone='warning'] {
        background: #ffedd5;
        color: #9a3412;
      }
      .badge[data-tone='danger'] {
        background: #fee2e2;
        color: #991b1b;
      }
      .badge[data-tone='info'] {
        background: #dbeafe;
        color: #1e40af;
      }
      .badge[data-tone='primary'] {
        background: var(--color-primary-light);
        color: var(--color-primary-dark);
      }
      .badge[data-tone='neutral'] {
        background: #f1f5f9;
        color: var(--color-text-secondary);
      }
    `,
  ],
})
export class AppBadgeComponent {
  @Input() tone: AppBadgeTone = 'neutral';
}
