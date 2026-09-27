import { Component, EventEmitter, Input, Output, forwardRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ControlValueAccessor, FormsModule, NG_VALUE_ACCESSOR } from '@angular/forms';

@Component({
  selector: 'app-input',
  standalone: true,
  imports: [CommonModule, FormsModule],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => AppInputComponent),
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
      <input
        [attr.type]="type"
        [attr.name]="name"
        [attr.placeholder]="placeholder"
        [attr.autocomplete]="autocomplete"
        [attr.min]="min"
        [attr.step]="step"
        [disabled]="disabled"
        [ngModel]="value"
        (ngModelChange)="onChange($event)"
        (blur)="onTouched()"
        (keydown)="keydown.emit($event)"
      />
      @if (hint && !error) {
        <span class="hint">{{ hint }}</span>
      }
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
        margin-left: 0.15rem;
      }
      input {
        width: 100%;
        height: var(--control-height);
        padding: 0 0.7rem;
        border: 1px solid var(--color-border);
        border-radius: var(--radius-md);
        background: var(--color-surface);
        color: var(--color-text);
      }
      input:disabled {
        background: var(--color-bg);
        color: var(--color-text-secondary);
      }
      .invalid input {
        border-color: var(--color-danger);
      }
      .hint {
        font-size: 0.75rem;
        color: var(--color-text-secondary);
      }
      .error {
        font-size: 0.75rem;
        color: var(--color-danger);
      }
    `,
  ],
})
export class AppInputComponent implements ControlValueAccessor {
  @Input() label = '';
  @Input() type = 'text';
  @Input() name = '';
  @Input() placeholder = '';
  @Input() autocomplete = '';
  @Input() hint = '';
  @Input() error = '';
  @Input() required = false;
  @Input() disabled = false;
  @Input() min: string | number | null = null;
  @Input() step: string | number | null = null;
  @Output() readonly keydown = new EventEmitter<KeyboardEvent>();

  value = '';
  private changed: (v: string) => void = () => undefined;
  private touched: () => void = () => undefined;

  writeValue(v: string | number | null): void {
    this.value = v == null ? '' : String(v);
  }
  registerOnChange(fn: (v: string) => void): void {
    this.changed = fn;
  }
  registerOnTouched(fn: () => void): void {
    this.touched = fn;
  }
  setDisabledState(isDisabled: boolean): void {
    this.disabled = isDisabled;
  }
  onChange(v: string): void {
    this.value = v;
    this.changed(v);
  }
  onTouched(): void {
    this.touched();
  }
}
