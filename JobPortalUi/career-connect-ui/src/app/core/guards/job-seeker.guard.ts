import { CanActivateFn, Router } from '@angular/router';
import { inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

export const jobSeekerGuard: CanActivateFn = () => {
  const router = inject(Router);
  const platformId = inject(PLATFORM_ID);

  // Prevent SSR / Vite crash
  if (!isPlatformBrowser(platformId)) {
    return false;
  }

  const refreshToken = localStorage.getItem('refreshToken');
  const userType = localStorage.getItem('userType');

  // ✅ Allow if session exists and user is a JobSeeker
  if (refreshToken && userType === 'JobSeeker') {
    return true;
  }

  router.navigate(['/login']);
  return false;
};
