import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap, map, catchError, of } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  ApiResponse,
  LoginRequest,
  LoginResponse,
  UserProfile,
} from '../models/api.models';

const TOKEN_KEY = 'pms.accessToken';
const USER_KEY = 'pms.user';
const EXPIRES_KEY = 'pms.expiresAtUtc';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly userSignal = signal<UserProfile | null>(this.readUser());
  readonly user = this.userSignal.asReadonly();
  readonly isAuthenticated = computed(() => !!this.getToken() && !!this.userSignal());

  constructor(
    private readonly http: HttpClient,
    private readonly router: Router,
  ) {}

  login(request: LoginRequest): Observable<LoginResponse> {
    return this.http
      .post<ApiResponse<LoginResponse>>(`${environment.apiBaseUrl}/api/v1/auth/login`, request)
      .pipe(
        map((res) => {
          if (!res.success || !res.data) {
            throw new Error(res.message || 'Login failed');
          }
          return res.data;
        }),
        tap((data) => this.persistSession(data)),
      );
  }

  me(): Observable<UserProfile | null> {
    return this.http
      .get<ApiResponse<UserProfile>>(`${environment.apiBaseUrl}/api/v1/auth/me`)
      .pipe(
        map((res) => (res.success ? res.data : null)),
        tap((profile) => {
          if (profile) {
            this.userSignal.set(profile);
            localStorage.setItem(USER_KEY, JSON.stringify(profile));
          }
        }),
        catchError(() => {
          this.logout(false);
          return of(null);
        }),
      );
  }

  logout(navigate = true): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    localStorage.removeItem(EXPIRES_KEY);
    this.userSignal.set(null);
    if (navigate) {
      void this.router.navigate(['/login']);
    }
  }

  getToken(): string | null {
    const token = localStorage.getItem(TOKEN_KEY);
    const expires = localStorage.getItem(EXPIRES_KEY);
    if (!token) return null;
    if (expires && new Date(expires).getTime() <= Date.now()) {
      this.logout(false);
      return null;
    }
    return token;
  }

  private persistSession(data: LoginResponse): void {
    localStorage.setItem(TOKEN_KEY, data.accessToken);
    localStorage.setItem(EXPIRES_KEY, data.expiresAtUtc);
    localStorage.setItem(USER_KEY, JSON.stringify(data.user));
    this.userSignal.set(data.user);
  }

  private readUser(): UserProfile | null {
    try {
      const raw = localStorage.getItem(USER_KEY);
      return raw ? (JSON.parse(raw) as UserProfile) : null;
    } catch {
      return null;
    }
  }
}
