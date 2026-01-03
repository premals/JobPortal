import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';

export const jobSeekerGuard: CanActivateFn = () => {
  const router = inject(Router);

  const token = localStorage.getItem('token');
  const userType = localStorage.getItem('userType');

  if (!token || userType !== 'JobSeeker') {
    router.navigate(['/login']);
    return false;
  }

  return true;
};
