import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AppModalComponent } from '../modal/app-modal.component';
import { AppButtonComponent } from '../button/app-button.component';

@Component({
  selector: 'app-confirm-dialog',
  standalone: true,
  imports: [CommonModule, AppModalComponent, AppButtonComponent],
  template: `
    <app-modal
      [open]="open"
      [title]="title"
      size="sm"
      [closeOnBackdrop]="!busy"
      (closed)="onCancel()"
    >
      <p class="body">{{ message }}</p>
      <div modal-footer>
        <app-button variant="secondary" [disabled]="busy" (clicked)="onCancel()">{{
          cancelLabel
        }}</app-button>
        <app-button
          [variant]="danger ? 'danger' : 'primary'"
          [loading]="busy"
          (clicked)="confirmed.emit()"
          >{{ confirmLabel }}</app-button
        >
      </div>
    </app-modal>
  `,
  styles: [
    `
      .body {
        margin: 0;
        color: var(--color-text-secondary);
        font-size: 0.9rem;
        line-height: 1.5;
      }
    `,
  ],
})
export class AppConfirmDialogComponent {
  @Input() open = false;
  @Input() title = 'Confirm';
  @Input() message = '';
  @Input() confirmLabel = 'Confirm';
  @Input() cancelLabel = 'Cancel';
  @Input() danger = false;
  @Input() busy = false;
  @Output() readonly confirmed = new EventEmitter<void>();
  @Output() readonly cancelled = new EventEmitter<void>();

  onCancel(): void {
    if (!this.busy) this.cancelled.emit();
  }
}
