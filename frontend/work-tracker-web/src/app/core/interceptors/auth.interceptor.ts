import { HttpClient, HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { AuthResponse } from '../../shared/models/auth.model';

const AUTH_PATHS = ['/api/auth/login', '/api/auth/signup', '/api/auth/refresh'];

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const http = inject(HttpClient);
  const router = inject(Router);

  const isAuthCall = AUTH_PATHS.some((path) => req.url.startsWith(path));
  const token = auth.accessToken;

  const authedReq = token && !isAuthCall ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } }) : req;

  return next(authedReq).pipe(
    catchError((err: unknown) => {
      if (!(err instanceof HttpErrorResponse) || err.status !== 401 || isAuthCall) {
        return throwError(() => err);
      }

      const refreshToken = auth.refreshToken;
      if (!refreshToken) {
        auth.clearSession();
        router.navigate(['/login']);
        return throwError(() => err);
      }

      return http.post<AuthResponse>('/api/auth/refresh', { refreshToken }).pipe(
        switchMap((res) => {
          auth.setAccessToken(res.accessToken);
          localStorage.setItem('refreshToken', res.refreshToken);
          const retryReq = req.clone({ setHeaders: { Authorization: `Bearer ${res.accessToken}` } });
          return next(retryReq);
        }),
        catchError((refreshErr) => {
          auth.clearSession();
          router.navigate(['/login']);
          return throwError(() => refreshErr);
        }),
      );
    }),
  );
};
