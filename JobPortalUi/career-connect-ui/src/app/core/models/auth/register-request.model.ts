export interface RegisterRequest {
  fullName?: string;
  email: string;
  password: string;
  userType: 'JobSeeker' | 'JobProvider' | string;
}