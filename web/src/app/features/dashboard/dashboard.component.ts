import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AppButtonComponent, AppPageHeaderComponent } from '../../shared';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [RouterLink, AppPageHeaderComponent, AppButtonComponent],
  template: `
    <app-page-header
      title="Dashboard"
      subtitle="Actionable pharmacy overview. Open POS to ring sales."
    >
      <a routerLink="/pos"><app-button>Open POS</app-button></a>
    </app-page-header>

    <div class="grid">
      <article>
        <h2>Today’s focus</h2>
        <p>
          Use <strong>Sales / POS</strong> for barcode sales, holds, and receipts. List modules follow
          the shared table pattern and will deepen in later slices.
        </p>
      </article>
      <article>
        <h2>Alerts</h2>
        <p>Low stock, expiry, and pending payment widgets will appear here once reporting APIs are wired.</p>
      </article>
    </div>
  `,
  styles: [
    `
      .grid {
        display: grid;
        grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
        gap: 0.75rem;
      }
      article {
        padding: 1rem;
        border: 1px solid var(--color-border);
        border-radius: var(--radius-lg);
        background: var(--color-surface);
      }
      h2 {
        font-size: 0.95rem;
        margin-bottom: 0.35rem;
      }
      p {
        color: var(--color-text-secondary);
        font-size: 0.875rem;
        line-height: 1.5;
      }
      a {
        text-decoration: none;
      }
    `,
  ],
})
export class DashboardComponent {}
