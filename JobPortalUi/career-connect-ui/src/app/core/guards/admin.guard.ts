import { CanActivateFn, Router } from '@angular/router';
import { inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

const decodeJwtPayload = (token: string): any | null => {
  try {
    const payload = token.split('.')[1];
    if (!payload) {
      return null;
    }
    const normalized = payload.replace(/-/g, '+').replace(/_/g, '/');
    const padded = normalized.padEnd(normalized.length + (4 - (normalized.length % 4)) % 4, '=');
    const json = atob(padded);
    return JSON.parse(json);
  } catch {
    return null;
  }
};

const hasAdminClaim = (payload: any): boolean => {
  if (!payload) {
    return false;
  }

  const roles: string[] = [];
  const claimKeys = [
    'role',
    'roles',
    'userType',
    'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'
  ];

  claimKeys.forEach(key => {
    const value = payload[key];
    if (!value) {
      return;
    }
    if (Array.isArray(value)) {
      roles.push(...value.map(String));
    } else {
      roles.push(String(value));
    }
  });

  return roles.some(role => role.toLowerCase() === 'admin');
};

export const adminGuard: CanActivateFn = () => {
  const router = inject(Router);
  const platformId = inject(PLATFORM_ID);

  if (!isPlatformBrowser(platformId)) {
    return false;
  }

  const refreshToken = localStorage.getItem('refreshToken');
  const accessToken = localStorage.getItem('accessToken');

  if (!refreshToken || !accessToken) {
    router.navigate(['/login']);
    return false;
  }

  const payload = decodeJwtPayload(accessToken);
  if (hasAdminClaim(payload)) {
    return true;
  }

  router.navigate(['/login']);
  return false;
};

