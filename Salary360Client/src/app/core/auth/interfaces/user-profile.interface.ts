import { UserStatus } from '../enums/user-status.enum';
import { UserAddress } from './user-address.interface';

export interface UserProfile {
  userId: string;
  email: string;
  displayName: string;
  dateOfBirth: string | null;
  phoneNumber: string | null;
  status: UserStatus;
  termsAcceptedAt: string | null;
  privacyPolicyAcceptedAt: string | null;
  profilePictureUrl: string | null;
  address: UserAddress | null;
  createdAtUtc: string;
  updatedAtUtc: string | null;
  roles: string[];
}
