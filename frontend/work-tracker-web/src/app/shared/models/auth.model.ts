export interface AuthUser {
  id: string;
  email: string;
  username: string;
}

export interface AuthResponse extends AuthUser {
  accessToken: string;
  refreshToken: string;
}
