export interface AdminUser {
  userId: string;
  fullName: string;
  email: string;
  role: string;
  isActive: boolean;
  forcePasswordReset: boolean;
}
