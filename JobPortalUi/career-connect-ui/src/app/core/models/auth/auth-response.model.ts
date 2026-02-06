export interface AuthProfile {
  userId: string;
  fullName: string;
  email: string;
  userType: string;
  forcePasswordReset: boolean;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  accessTokenExpiresAt: string;
  profile: AuthProfile;
}
