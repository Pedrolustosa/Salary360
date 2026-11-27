import { UserAddress } from './user-address.interface';

export interface UpdateProfileRequest {
  displayName: string;
  dateOfBirth: string;
  phoneNumber: string | null;
  profilePictureUrl: string | null;
  address: UserAddress | null;
}
