import { CanActivateFn, Router } from '@angular/router';
import { inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

export const jobProviderGuard: CanActivateFn = () => {
  const router = inject(Router);
  const platformId = inject(PLATFORM_ID);

  // Prevent SSR / Vite crash
  if (!isPlatformBrowser(platformId)) {
    return false;
  }

  const refreshToken = localStorage.getItem('refreshToken');
  const role = localStorage.getItem('userType'); // if role-based

  console.log('refresh', localStorage.getItem('refreshToken'));

  // ✅ Allow if session exists
  if (refreshToken  && role === 'JobProvider') {
    return true;
  }

  router.navigate(['/login']);
  return false;
};
