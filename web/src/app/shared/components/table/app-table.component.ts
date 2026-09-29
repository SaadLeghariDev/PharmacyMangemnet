import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AppButtonComponent } from '../button/app-button.component';

export interface AppTableColumn {
  key: string;
  label: string;
  width?: string;
  align?: 'left' | 'right' | 'center';
}

@Component({
  selector: 'app-table',
  standalone: true,
  imports: [CommonModule, AppButtonComponent],
  template: `
    <div class="table-wrap">
      <table>
        <thead>
          <tr>
            @for (col of columns; track col.key) {
              <th [style.width]="col.width || null" [style.textAlign]="col.align || 'left'">
                {{ col.label }}
              </th>
            }
            @if (hasActions) {
              <th class="actions">Actions</th>
            }
          </tr>
        </thead>
        <tbody>
          <ng-content />
        </tbody>
      </table>
    </div>
    @if (showPagination) {
      <div class="pager">
        <span class="meta">
          @if (totalCount != null) {
            {{ totalCount }} result{{ totalCount === 1 ? '' : 's' }}
          }
          · Page {{ page }}
        </span>
        <div class="pager-actions">
          <app-button
            variant="secondary"
            size="sm"
            [disabled]="page <= 1 || loading"
            (clicked)="pageChange.emit(page - 1)"
            >Previous</app-button
          >
          <app-button
            variant="secondary"
            size="sm"
            [disabled]="!hasNext || loading"
            (clicked)="pageChange.emit(page + 1)"
            >Next</app-button
          >
        </div>
      </div>
    }
  `,
  styles: [
    `
      .table-wrap {
        overflow: auto;
        border: 1px solid var(--color-border);
        border-radius: var(--radius-lg);
        background: var(--color-surface);
      }
      table {
        width: 100%;
        border-collapse: collapse;
        min-width: 640px;
      }
      thead th {
        position: sticky;
        top: 0;
        z-index: 1;
        background: #f1f5f9;
        border-bottom: 1px solid var(--color-border);
        padding: 0.65rem 0.75rem;
        text-align: left;
        font-size: 0.75rem;
        font-weight: 600;
        text-transform: uppercase;
        letter-spacing: 0.03em;
        color: var(--color-text-secondary);
        white-space: nowrap;
      }
      th.actions {
        text-align: right;
        width: 1%;
        white-space: nowrap;
      }
      :host ::ng-deep tbody td {
        padding: 0.7rem 0.75rem;
        border-bottom: 1px solid var(--color-border);
        vertical-align: middle;
        color: var(--color-text);
      }
      :host ::ng-deep tbody tr:hover td {
        background: #f8fafc;
      }
      :host ::ng-deep tbody tr:last-child td {
        border-bottom: 0;
      }
      .pager {
        display: flex;
        align-items: center;
        justify-content: space-between;
        gap: 0.75rem;
        margin-top: 0.75rem;
      }
      .meta {
        color: var(--color-text-secondary);
        font-size: 0.8125rem;
      }
      .pager-actions {
        display: flex;
        gap: 0.4rem;
      }
    `,
  ],
})
export class AppTableComponent {
  @Input() columns: AppTableColumn[] = [];
  @Input() hasActions = false;
  @Input() showPagination = true;
  @Input() page = 1;
  @Input() hasNext = false;
  @Input() totalCount: number | null = null;
  @Input() loading = false;
  @Output() readonly pageChange = new EventEmitter<number>();
}
