export interface RegisterRequest {
  email: string;
  password: string;
  displayName: string;
  dateOfBirth: string;
  phoneNumber: string | null;
  acceptedTerms: boolean;
  acceptedPrivacyPolicy: boolean;
}
