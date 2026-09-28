import {
  Component,
  ElementRef,
  EventEmitter,
  Input,
  Output,
  ViewChild,
  forwardRef,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

export interface AppSelectOption {
  value: string | number;
  label: string;
}

@Component({
  selector: 'app-select',
  standalone: true,
  imports: [CommonModule],
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
        #control
        [disabled]="disabled"
        (change)="onSelectChange($event)"
        (blur)="onTouched()"
      >
        @if (placeholder) {
          <option value="" disabled>{{ placeholder }}</option>
        }
        @for (opt of options; track opt.value) {
          <option [value]="stringify(opt.value)">{{ opt.label }}</option>
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
  @ViewChild('control', { static: true })
  private readonly control?: ElementRef<HTMLSelectElement>;

  @Input() label = '';
  @Input() placeholder = '';
  @Input() error = '';
  @Input() required = false;
  @Input() disabled = false;
  @Input() options: AppSelectOption[] = [];
  @Output() readonly selectionChange = new EventEmitter<string | number>();

  private value: string | number = '';
  private changed: (v: string | number) => void = () => undefined;
  private touched: () => void = () => undefined;

  writeValue(v: string | number | null): void {
    this.value = v ?? '';
    const el = this.control?.nativeElement;
    const next = this.stringify(this.value);
    if (el && el.value !== next) {
      el.value = next;
    }
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

  onSelectChange(event: Event): void {
    const raw = (event.target as HTMLSelectElement).value;
    const matched = this.options.find((o) => this.stringify(o.value) === raw);
    const next = matched ? matched.value : raw;
    this.value = next;
    this.changed(next);
    this.selectionChange.emit(next);
  }

  onTouched(): void {
    this.touched();
  }

  stringify(v: string | number): string {
    return v == null ? '' : String(v);
  }
}
