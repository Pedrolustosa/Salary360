import { AppNotification } from '../interfaces/app-notification.interface';

export type IncomingNotification = Omit<AppNotification, 'read'>;
