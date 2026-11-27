export interface AuthResponse {
  userId: string;
  email: string;
  displayName: string;
  dateOfBirth: string | null;
  profilePictureUrl: string | null;
  accessToken: string;
  expiresAtUtc: string;
  roles: string[];
  permissions: string[];
}
