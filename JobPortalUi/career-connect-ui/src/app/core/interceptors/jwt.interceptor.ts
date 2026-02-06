import {
  HttpInterceptorFn,
  HttpErrorResponse
} from '@angular/common/http';
import { inject, Injector } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { catchError, switchMap, throwError } from 'rxjs';

let isRefreshing = false;


/**
 * JWT Interceptor
 * Attaches Authorization header if token exists
 */
export const jwtInterceptor: HttpInterceptorFn = (req, next) => {

  // Use Injector to lazily resolve AuthService at runtime to avoid circular
  // dependency between HttpClient <-> interceptors <-> AuthService
  const injector = inject(Injector);
  const isBrowser = typeof window !== 'undefined' && typeof localStorage !== 'undefined';
  const accessToken = isBrowser ? localStorage.getItem('accessToken') : null;

  if (!isBrowser) {
    return next(req);
  }

  // Skip interceptor for refresh token API
  if (req.url.includes('/refresh-token')) {
    return next(req);
  }

  // Attach access token
  const authReq = accessToken
    ? req.clone({
        setHeaders: {
          Authorization: `Bearer ${accessToken}`
        }
      })
    : req;

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {

      // Access token expired
      if (error.status === 401 && !isRefreshing) {
        isRefreshing = true;

        // Resolve AuthService lazily to avoid circular provider initialization
        const authService = injector.get(AuthService);

        return authService.refreshToken().pipe(
          switchMap(res => {
            isRefreshing = false;

            // Retry original request with new token
            const retryReq = req.clone({
              setHeaders: {
                Authorization: `Bearer ${res.accessToken}`
              }
            });

            return next(retryReq);
          }),
          catchError(err => {
            // Refresh token also expired
            isRefreshing = false;
            authService.logout();
            return throwError(() => err);
          })
        );
      }

      return throwError(() => error);
    })
  );
};
