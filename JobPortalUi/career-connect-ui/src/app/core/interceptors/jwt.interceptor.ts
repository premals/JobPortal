import { HttpInterceptorFn } from '@angular/common/http';

/**
 * JWT Interceptor
 * Attaches Authorization header if token exists
 */
export const jwtInterceptor: HttpInterceptorFn = (req, next) => {

  // Get token from storage
  const token = localStorage.getItem('token');

  // Clone request and add Authorization header if token exists
  if (token) {
    const authReq = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });

    return next(authReq);
  }

  // If no token, continue without modification
  return next(req);
};
