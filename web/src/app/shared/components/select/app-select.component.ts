import {
  Component,
  ElementRef,
  EventEmitter,
  HostListener,
  Input,
  OnChanges,
  Output,
  SimpleChanges,
  ViewChild,
  forwardRef,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

export interface AppSelectOption {
  value: string | number;
  label: string;
}

const MAX_VISIBLE = 100;

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
    <div class="field" [class.invalid]="!!error" [class.open]="open">
      @if (label) {
        <span class="label">
          {{ label }}
          @if (required) {
            <abbr title="required">*</abbr>
          }
        </span>
      }
      <div class="control-wrap" #wrap>
        <input
          #control
          type="text"
          role="combobox"
          [disabled]="disabled"
          [placeholder]="placeholder || 'Type to search…'"
          [value]="query"
          [attr.aria-expanded]="open"
          aria-autocomplete="list"
          [attr.aria-controls]="listId"
          autocomplete="off"
          (input)="onInput($event)"
          (focus)="onFocus()"
          (keydown)="onKeydown($event)"
          (blur)="onBlur()"
        />
        <button
          type="button"
          class="chevron"
          tabindex="-1"
          aria-label="Toggle options"
          [disabled]="disabled"
          (mousedown)="$event.preventDefault(); toggleOpen()"
        >
          ▾
        </button>
        @if (open) {
          <div class="panel" [id]="listId" role="listbox">
            @if (!filtered.length) {
              <p class="status">No matches</p>
            } @else {
              <ul class="list">
                @for (opt of visibleOptions; track trackOpt($index, opt); let i = $index) {
                  <li>
                    <button
                      type="button"
                      class="item"
                      role="option"
                      [class.active]="highlightIndex === i"
                      [attr.aria-selected]="isSelected(opt)"
                      (mousedown)="$event.preventDefault(); pick(opt)"
                      (mouseenter)="highlightIndex = i"
                    >
                      <span [innerHTML]="highlightLabel(opt.label)"></span>
                    </button>
                  </li>
                }
              </ul>
              @if (filtered.length > maxVisible) {
                <p class="status hint">Showing {{ maxVisible }} of {{ filtered.length }} — type to narrow</p>
              }
            }
          </div>
        }
      </div>
      @if (error) {
        <span class="error" role="alert">{{ error }}</span>
      }
    </div>
  `,
  styles: [
    `
      .field {
        display: grid;
        gap: 0.3rem;
        position: relative;
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
      .control-wrap {
        position: relative;
      }
      input {
        width: 100%;
        height: var(--control-height);
        padding: 0 2rem 0 0.7rem;
        border: 1px solid var(--color-border);
        border-radius: var(--radius-md);
        background: var(--color-surface);
        color: var(--color-text);
        box-sizing: border-box;
      }
      input:focus {
        outline: 2px solid var(--color-primary);
        outline-offset: 0;
      }
      input:disabled {
        opacity: 0.65;
        cursor: not-allowed;
      }
      .invalid input {
        border-color: var(--color-danger);
      }
      .chevron {
        position: absolute;
        right: 0.35rem;
        top: 50%;
        transform: translateY(-50%);
        border: 0;
        background: transparent;
        color: var(--color-text-secondary);
        cursor: pointer;
        line-height: 1;
        padding: 0.25rem;
        font-size: 0.85rem;
      }
      .chevron:disabled {
        cursor: not-allowed;
        opacity: 0.5;
      }
      .panel {
        position: absolute;
        left: 0;
        right: 0;
        top: calc(100% + 0.25rem);
        z-index: var(--z-select-dropdown, 1050);
        max-height: 16rem;
        overflow: auto;
        border: 1px solid var(--color-border);
        border-radius: var(--radius-md);
        background: var(--color-surface);
        box-shadow: var(--shadow-md);
      }
      .list {
        list-style: none;
        margin: 0;
        padding: 0;
      }
      .item {
        display: block;
        width: 100%;
        text-align: left;
        border: 0;
        background: transparent;
        padding: 0.55rem 0.75rem;
        color: var(--color-text);
        cursor: pointer;
        border-bottom: 1px solid var(--color-border);
        font-size: 0.875rem;
      }
      .list li:last-child .item {
        border-bottom: 0;
      }
      .item:hover,
      .item.active {
        background: var(--color-primary-light, #eef6ff);
      }
      .item mark {
        background: transparent;
        color: var(--color-primary-dark, #0b4f8a);
        font-weight: 700;
        padding: 0;
      }
      .status {
        margin: 0;
        padding: 0.65rem 0.75rem;
        font-size: 0.8125rem;
        color: var(--color-text-secondary);
      }
      .status.hint {
        border-top: 1px solid var(--color-border);
        font-size: 0.75rem;
      }
      .error {
        font-size: 0.75rem;
        color: var(--color-danger);
      }
    `,
  ],
})
export class AppSelectComponent implements ControlValueAccessor, OnChanges {
  @ViewChild('control')
  private readonly control?: ElementRef<HTMLInputElement>;

  @ViewChild('wrap')
  private readonly wrap?: ElementRef<HTMLElement>;

  @Input() label = '';
  @Input() placeholder = '';
  @Input() error = '';
  @Input() required = false;
  @Input() disabled = false;
  @Input() options: AppSelectOption[] = [];
  @Output() readonly selectionChange = new EventEmitter<string | number>();

  readonly maxVisible = MAX_VISIBLE;
  readonly listId = `app-select-${Math.random().toString(36).slice(2, 9)}`;

  open = false;
  query = '';
  highlightIndex = 0;
  filtered: AppSelectOption[] = [];

  private value: string | number = '';
  private typing = false;
  private changed: (v: string | number) => void = () => undefined;
  private touched: () => void = () => undefined;

  get visibleOptions(): AppSelectOption[] {
    return this.filtered.slice(0, this.maxVisible);
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['options']) {
      this.recomputeFilter();
      if (!this.typing) {
        this.syncQueryFromValue();
      }
    }
  }

  writeValue(v: string | number | null): void {
    this.value = v ?? '';
    this.typing = false;
    this.syncQueryFromValue();
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

  @HostListener('document:mousedown', ['$event'])
  onDocumentMouseDown(event: MouseEvent): void {
    if (!this.open) return;
    const root = this.wrap?.nativeElement;
    if (root && !root.contains(event.target as Node)) {
      this.closeAndRestore();
    }
  }

  onFocus(): void {
    if (this.disabled) return;
    this.openPanel(true);
  }

  onBlur(): void {
    this.touched();
    // Delay so mousedown on option can fire first
    setTimeout(() => {
      if (!this.open) return;
      // If still open after blur without pick, restore label
      this.closeAndRestore();
    }, 150);
  }

  onInput(event: Event): void {
    if (this.disabled) return;
    this.typing = true;
    this.query = (event.target as HTMLInputElement).value;
    this.open = true;
    this.recomputeFilter();
    this.highlightIndex = 0;
  }

  onKeydown(event: KeyboardEvent): void {
    if (this.disabled) return;
    const visible = this.visibleOptions;

    if (event.key === 'ArrowDown') {
      event.preventDefault();
      if (!this.open) {
        this.openPanel(true);
        return;
      }
      if (!visible.length) return;
      this.highlightIndex = Math.min(this.highlightIndex + 1, visible.length - 1);
      return;
    }
    if (event.key === 'ArrowUp') {
      event.preventDefault();
      if (!this.open) {
        this.openPanel(true);
        return;
      }
      if (!visible.length) return;
      this.highlightIndex = Math.max(this.highlightIndex - 1, 0);
      return;
    }
    if (event.key === 'Enter') {
      if (this.open && visible.length) {
        event.preventDefault();
        this.pick(visible[this.highlightIndex] ?? visible[0]);
      }
      return;
    }
    if (event.key === 'Escape') {
      if (this.open) {
        event.preventDefault();
        this.closeAndRestore();
      }
      return;
    }
  }

  toggleOpen(): void {
    if (this.disabled) return;
    if (this.open) {
      this.closeAndRestore();
    } else {
      this.openPanel(true);
      this.control?.nativeElement.focus();
    }
  }

  pick(opt: AppSelectOption): void {
    this.value = opt.value;
    this.typing = false;
    this.query = opt.label;
    this.open = false;
    this.changed(opt.value);
    this.selectionChange.emit(opt.value);
    this.touched();
  }

  isSelected(opt: AppSelectOption): boolean {
    return this.stringify(opt.value) === this.stringify(this.value);
  }

  trackOpt(index: number, opt: AppSelectOption): string {
    return `${this.stringify(opt.value)}:${index}`;
  }

  highlightLabel(label: string): string {
    const q = this.query.trim();
    const escaped = this.escapeHtml(label);
    if (!q || !this.typing) return escaped;
    const idx = label.toLowerCase().indexOf(q.toLowerCase());
    if (idx < 0) return escaped;
    const before = this.escapeHtml(label.slice(0, idx));
    const match = this.escapeHtml(label.slice(idx, idx + q.length));
    const after = this.escapeHtml(label.slice(idx + q.length));
    return `${before}<mark>${match}</mark>${after}`;
  }

  stringify(v: string | number): string {
    return v == null ? '' : String(v);
  }

  private openPanel(selectAll: boolean): void {
    this.open = true;
    this.typing = false;
    this.recomputeFilter();
    const selectedIdx = this.visibleOptions.findIndex((o) => this.isSelected(o));
    this.highlightIndex = selectedIdx >= 0 ? selectedIdx : 0;
    if (selectAll) {
      setTimeout(() => this.control?.nativeElement.select(), 0);
    }
  }

  private closeAndRestore(): void {
    this.open = false;
    this.typing = false;
    this.syncQueryFromValue();
  }

  private syncQueryFromValue(): void {
    const match = this.options.find((o) => this.stringify(o.value) === this.stringify(this.value));
    this.query = match ? match.label : this.value === '' || this.value == null ? '' : String(this.value);
  }

  private recomputeFilter(): void {
    const q = this.typing ? this.query.trim().toLowerCase() : '';
    this.filtered = !q
      ? [...this.options]
      : this.options.filter((o) => o.label.toLowerCase().includes(q));
  }

  private escapeHtml(s: string): string {
    return s
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;');
  }
}
