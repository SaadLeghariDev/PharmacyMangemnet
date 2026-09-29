import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss',
})
export class LoginComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  username = 'admin';
  /** Prefill for local Development seed only. */
  password = environment.production ? '' : 'Admin@12345';
  readonly busy = signal(false);
  readonly error = signal('');

  submit(event?: Event): void {
    event?.preventDefault();
    if (this.busy()) return;
    this.error.set('');
    if (!this.username.trim() || !this.password) {
      this.error.set('Username and password are required.');
      return;
    }
    this.busy.set(true);
    this.auth.login({ username: this.username.trim(), password: this.password }).subscribe({
      next: () => {
        this.busy.set(false);
        void this.router.navigateByUrl('/dashboard', { replaceUrl: true }).then((ok) => {
          if (!ok) {
            void this.router.navigate(['/dashboard'], { replaceUrl: true });
          }
        });
      },
      error: (err: unknown) => {
        this.busy.set(false);
        this.error.set(this.readError(err));
      },
    });
  }

  private readError(err: unknown): string {
    if (err && typeof err === 'object' && 'error' in err) {
      const body = (err as { error?: { message?: string; errors?: string[] } }).error;
      if (body?.errors?.length) return body.errors.join('; ');
      if (body?.message) return body.message;
    }
    if (err instanceof Error && err.message) return err.message;
    return 'Unable to sign in. Check your credentials and try again.';
  }
}
