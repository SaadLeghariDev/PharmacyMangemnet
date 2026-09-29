import {
  Component,
  ElementRef,
  EventEmitter,
  HostListener,
  Input,
  OnDestroy,
  Output,
  ViewChild,
  forwardRef,
  signal,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import {
  Observable,
  Subject,
  Subscription,
  catchError,
  debounceTime,
  distinctUntilChanged,
  of,
  switchMap,
} from 'rxjs';

export interface AppTypeaheadItem<T = unknown> {
  id: string | number;
  label: string;
  detail?: string;
  trailing?: string;
  badge?: string;
  badgeTone?: 'ok' | 'warn' | 'muted';
  data?: T;
}

@Component({
  selector: 'app-typeahead',
  standalone: true,
  imports: [CommonModule],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => AppTypeaheadComponent),
      multi: true,
    },
  ],
  template: `
    <div class="ta" [class.open]="panelOpen()">
      @if (label) {
        <label class="ta-label" [attr.for]="inputId">{{ label }}</label>
      }
      <div class="ta-field">
        <input
          #inputEl
          [id]="inputId"
          class="ta-input"
          type="text"
          [value]="query"
          [placeholder]="placeholder"
          [disabled]="disabled"
          autocomplete="off"
          role="combobox"
          [attr.aria-expanded]="panelOpen()"
          aria-autocomplete="list"
          [attr.aria-controls]="listId"
          (input)="onInput($event)"
          (keydown)="onKey($event)"
          (focus)="onFocus()"
        />
        <ng-content select="[ta-addon]" />
      </div>
      @if (panelOpen()) {
        <div class="ta-panel" [id]="listId" role="listbox">
          @if (loading()) {
            <p class="ta-status">Searching…</p>
          } @else if (empty()) {
            <p class="ta-status">{{ emptyText }}</p>
          } @else {
            <ul class="ta-list">
              @for (item of items(); track item.id; let i = $index) {
                <li>
                  <button
                    type="button"
                    class="ta-item"
                    role="option"
                    [class.active]="highlightIndex() === i"
                    [attr.aria-selected]="highlightIndex() === i"
                    (mousedown)="$event.preventDefault()"
                    (click)="select(item)"
                    (mouseenter)="highlightIndex.set(i)"
                  >
                    <span class="ta-main">
                      <span class="ta-name" [innerHTML]="highlightLabel(item.label)"></span>
                      @if (item.detail) {
                        <small>{{ item.detail }}</small>
                      }
                    </span>
                    <span class="ta-meta">
                      @if (item.badge) {
                        <span class="ta-badge" [class.warn]="item.badgeTone === 'warn'" [class.muted]="item.badgeTone === 'muted'">{{
                          item.badge
                        }}</span>
                      }
                      @if (item.trailing) {
                        <strong>{{ item.trailing }}</strong>
                      }
                    </span>
                  </button>
                </li>
              }
            </ul>
          }
        </div>
      }
    </div>
  `,
  styles: [
    `
      :host {
        display: block;
        min-width: 0;
      }
      .ta {
        position: relative;
      }
      .ta-label {
        display: block;
        font-size: 0.8125rem;
        font-weight: 600;
        color: var(--color-text-secondary);
        margin-bottom: 0.35rem;
      }
      .ta-field {
        display: grid;
        grid-template-columns: 1fr auto;
        gap: 0.45rem;
        align-items: stretch;
      }
      .ta-input {
        width: 100%;
        height: 40px;
        padding: 0 0.75rem;
        border: 1px solid var(--color-border);
        border-radius: var(--radius-md);
        background: #fff;
        color: var(--color-text);
        font-size: 0.9375rem;
      }
      .ta-input:focus {
        outline: 2px solid var(--color-primary);
        outline-offset: 1px;
      }
      .ta-input:disabled {
        opacity: 0.65;
        cursor: not-allowed;
      }
      .ta-panel {
        position: absolute;
        left: 0;
        right: 0;
        top: calc(100% + 2px);
        z-index: 50;
        border: 1px solid var(--color-border);
        border-radius: var(--radius-md);
        background: var(--color-surface);
        box-shadow: var(--shadow-md);
        max-height: 280px;
        overflow: auto;
      }
      .ta-status {
        margin: 0;
        padding: 0.7rem 0.8rem;
        color: var(--color-text-secondary);
        font-size: 0.85rem;
      }
      .ta-list {
        list-style: none;
        margin: 0;
        padding: 0;
      }
      .ta-item {
        width: 100%;
        display: flex;
        justify-content: space-between;
        align-items: flex-start;
        gap: 0.65rem;
        border: 0;
        border-bottom: 1px solid var(--color-border);
        background: transparent;
        padding: 0.55rem 0.75rem;
        text-align: left;
        cursor: pointer;
      }
      .ta-list li:last-child .ta-item {
        border-bottom: 0;
      }
      .ta-item:hover,
      .ta-item.active {
        background: var(--color-primary-light);
      }
      .ta-main {
        min-width: 0;
        flex: 1;
      }
      .ta-name {
        display: block;
        font-weight: 600;
        color: var(--color-text);
        font-size: 0.875rem;
      }
      .ta-name mark {
        background: #fde68a;
        color: inherit;
        padding: 0 0.1rem;
        border-radius: 2px;
      }
      .ta-main small {
        display: block;
        color: var(--color-text-secondary);
        font-size: 0.75rem;
        margin-top: 0.1rem;
      }
      .ta-meta {
        display: grid;
        gap: 0.15rem;
        justify-items: end;
        flex-shrink: 0;
        font-size: 0.75rem;
      }
      .ta-meta strong {
        font-variant-numeric: tabular-nums;
        color: var(--color-text);
      }
      .ta-badge {
        color: var(--color-success);
        font-size: 0.7rem;
      }
      .ta-badge.warn {
        color: var(--color-danger);
      }
      .ta-badge.muted {
        color: var(--color-text-secondary);
      }
    `,
  ],
})
export class AppTypeaheadComponent<T = unknown> implements ControlValueAccessor, OnDestroy {
  @ViewChild('inputEl') inputEl?: ElementRef<HTMLInputElement>;

  @Input() label = '';
  @Input() placeholder = 'Search…';
  @Input() emptyText = 'No results found';
  @Input() minChars = 2;
  @Input() debounceMs = 250;
  @Input() disabled = false;
  @Input() searchFn: ((query: string) => Observable<AppTypeaheadItem<T>[]>) | null = null;
  @Input() clearOnSelect = false;

  @Output() readonly selected = new EventEmitter<AppTypeaheadItem<T>>();
  @Output() readonly submitted = new EventEmitter<string>();
  @Output() readonly queryChange = new EventEmitter<string>();

  readonly inputId = `ta-${Math.random().toString(36).slice(2, 9)}`;
  readonly listId = `${this.inputId}-list`;

  query = '';
  readonly items = signal<AppTypeaheadItem<T>[]>([]);
  readonly panelOpen = signal(false);
  readonly loading = signal(false);
  readonly empty = signal(false);
  readonly highlightIndex = signal(0);

  private readonly search$ = new Subject<string>();
  private searchSub?: Subscription;
  private onChange: (v: string) => void = () => undefined;
  private onTouched: () => void = () => undefined;

  constructor() {
    this.searchSub = this.search$
      .pipe(
        debounceTime(this.debounceMs),
        distinctUntilChanged(),
        switchMap((term) => {
          const q = term.trim();
          if (!this.searchFn || q.length < this.minChars) {
            this.closePanel(false);
            return of(null);
          }
          this.loading.set(true);
          this.empty.set(false);
          this.panelOpen.set(true);
          return this.searchFn(q).pipe(
            catchError(() => {
              this.loading.set(false);
              this.empty.set(true);
              this.items.set([]);
              return of([] as AppTypeaheadItem<T>[]);
            }),
          );
        }),
      )
      .subscribe((rows) => {
        if (rows == null) return;
        this.loading.set(false);
        this.items.set(rows);
        this.empty.set(rows.length === 0);
        this.highlightIndex.set(0);
        this.panelOpen.set(true);
      });
  }

  ngOnDestroy(): void {
    this.searchSub?.unsubscribe();
  }

  writeValue(value: string | null): void {
    this.query = value ?? '';
  }

  registerOnChange(fn: (v: string) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled = isDisabled;
  }

  focus(): void {
    setTimeout(() => this.inputEl?.nativeElement?.focus(), 0);
  }

  clear(): void {
    this.setQuery('');
    this.closePanel(true);
  }

  @HostListener('document:click', ['$event'])
  onDocClick(ev: MouseEvent): void {
    const host = (ev.target as HTMLElement | null)?.closest('app-typeahead');
    if (host !== (this.inputEl?.nativeElement.closest('app-typeahead') ?? null)) {
      this.closePanel(false);
    }
  }

  onInput(ev: Event): void {
    const value = (ev.target as HTMLInputElement).value;
    this.setQuery(value);
    this.search$.next(value);
  }

  onFocus(): void {
    this.onTouched();
    if (this.query.trim().length >= this.minChars) {
      this.search$.next(this.query);
    }
  }

  onKey(event: KeyboardEvent): void {
    const list = this.items();
    const open = this.panelOpen();

    if (event.key === 'ArrowDown' && open && list.length) {
      event.preventDefault();
      this.highlightIndex.set(Math.min(list.length - 1, this.highlightIndex() + 1));
      return;
    }
    if (event.key === 'ArrowUp' && open && list.length) {
      event.preventDefault();
      this.highlightIndex.set(Math.max(0, this.highlightIndex() - 1));
      return;
    }
    if (event.key === 'Escape' && open) {
      event.preventDefault();
      this.closePanel(true);
      return;
    }
    if (event.key === 'Enter') {
      event.preventDefault();
      if (open && list.length) {
        const hit = list[this.highlightIndex()] ?? list[0];
        if (hit) this.select(hit);
        return;
      }
      this.submitted.emit(this.query.trim());
      this.closePanel(true);
    }
  }

  select(item: AppTypeaheadItem<T>): void {
    if (this.clearOnSelect) {
      this.setQuery('');
    } else {
      this.setQuery(item.label);
    }
    this.closePanel(true);
    this.selected.emit(item);
  }

  highlightLabel(name: string): string {
    const q = this.query.trim();
    if (!q) return this.escapeHtml(name);
    const idx = name.toLowerCase().indexOf(q.toLowerCase());
    if (idx < 0) return this.escapeHtml(name);
    const before = this.escapeHtml(name.slice(0, idx));
    const match = this.escapeHtml(name.slice(idx, idx + q.length));
    const after = this.escapeHtml(name.slice(idx + q.length));
    return `${before}<mark>${match}</mark>${after}`;
  }

  private setQuery(value: string): void {
    this.query = value;
    this.onChange(value);
    this.queryChange.emit(value);
  }

  private closePanel(clearItems: boolean): void {
    this.panelOpen.set(false);
    this.loading.set(false);
    this.empty.set(false);
    if (clearItems) {
      this.items.set([]);
      this.highlightIndex.set(0);
    }
  }

  private escapeHtml(s: string): string {
    return s
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;');
  }
}
