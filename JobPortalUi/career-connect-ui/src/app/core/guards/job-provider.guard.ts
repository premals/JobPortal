import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';

export const jobProviderGuard: CanActivateFn = () => {
  const router = inject(Router);

  const userType = localStorage.getItem('userType');

  if (userType !== 'JobProvider') {
    router.navigate(['/profile']);
    return false;
  }

  return true;
};
