import { Component, EventEmitter, Input, Output, forwardRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ControlValueAccessor, FormsModule, NG_VALUE_ACCESSOR } from '@angular/forms';

export interface AppSelectOption {
  value: string | number;
  label: string;
}

@Component({
  selector: 'app-select',
  standalone: true,
  imports: [CommonModule, FormsModule],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => AppSelectComponent),
      multi: true,
    },
  ],
  template: `
    <label class="field" [class.invalid]="!!error">
      @if (label) {
        <span class="label">
          {{ label }}
          @if (required) {
            <abbr title="required">*</abbr>
          }
        </span>
      }
      <select
        [disabled]="disabled"
        [ngModel]="value"
        (ngModelChange)="onChange($event)"
        (blur)="onTouched()"
      >
        @if (placeholder) {
          <option [ngValue]="''" disabled>{{ placeholder }}</option>
        }
        @for (opt of options; track opt.value) {
          <option [ngValue]="opt.value">{{ opt.label }}</option>
        }
      </select>
      @if (error) {
        <span class="error" role="alert">{{ error }}</span>
      }
    </label>
  `,
  styles: [
    `
      .field {
        display: grid;
        gap: 0.3rem;
      }
      .label {
        font-size: 0.8125rem;
        font-weight: 500;
        color: var(--color-text-secondary);
      }
      .label abbr {
        color: var(--color-danger);
        text-decoration: none;
      }
      select {
        width: 100%;
        height: var(--control-height);
        padding: 0 0.7rem;
        border: 1px solid var(--color-border);
        border-radius: var(--radius-md);
        background: var(--color-surface);
        color: var(--color-text);
      }
      .invalid select {
        border-color: var(--color-danger);
      }
      .error {
        font-size: 0.75rem;
        color: var(--color-danger);
      }
    `,
  ],
})
export class AppSelectComponent implements ControlValueAccessor {
  @Input() label = '';
  @Input() placeholder = '';
  @Input() error = '';
  @Input() required = false;
  @Input() disabled = false;
  @Input() options: AppSelectOption[] = [];
  @Output() readonly selectionChange = new EventEmitter<string | number>();

  value: string | number = '';
  private changed: (v: string | number) => void = () => undefined;
  private touched: () => void = () => undefined;

  writeValue(v: string | number | null): void {
    this.value = v ?? '';
  }
  registerOnChange(fn: (v: string | number) => void): void {
    this.changed = fn;
  }
  registerOnTouched(fn: () => void): void {
    this.touched = fn;
  }
  setDisabledState(isDisabled: boolean): void {
    this.disabled = isDisabled;
  }
  onChange(v: string | number): void {
    this.value = v;
    this.changed(v);
    this.selectionChange.emit(v);
  }
  onTouched(): void {
    this.touched();
  }
}
