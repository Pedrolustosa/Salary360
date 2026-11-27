export interface AppNotification {
  id: string;
  type: string;
  title: string;
  message: string;
  createdAtUtc: string;
  read: boolean;
}
