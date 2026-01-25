import { CanActivateFn, Router } from '@angular/router';
import { inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

export const authGuard: CanActivateFn = () => {
  const router = inject(Router);
  const platformId = inject(PLATFORM_ID);

  // Prevent SSR / Vite crash
  if (!isPlatformBrowser(platformId)) {
    return false;
  }

  const refreshToken = localStorage.getItem('refreshToken');

  // ✅ Allow route if refresh token exists
  // Access token may be expired — interceptor will refresh it
  if (refreshToken) {
    return true;
  }

  // ❌ No refresh token → force logout
  router.navigate(['/login']);
  return false;
};
