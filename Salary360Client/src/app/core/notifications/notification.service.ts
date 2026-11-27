import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { HubConnection, HubConnectionBuilder, HubConnectionState, LogLevel } from '@microsoft/signalr';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthService } from '../auth/auth.service';
import { HubStatus } from './enums/hub-status.enum';
import { AppNotification } from './interfaces/app-notification.interface';
import { IncomingNotification } from './types/incoming-notification.type';

const RECEIVE_METHOD = 'ReceiveNotification';
const MAX_ITEMS = 50;

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private readonly auth = inject(AuthService);
  private readonly http = inject(HttpClient);
  private connection?: HubConnection;

  private readonly itemsState = signal<AppNotification[]>([]);
  private readonly statusState = signal<HubStatus>(HubStatus.Disconnected);

  readonly items = this.itemsState.asReadonly();
  readonly status = this.statusState.asReadonly();
  readonly unreadCount = computed(() => this.itemsState().filter((item) => !item.read).length);

  async connect(): Promise<void> {
    if (this.connection && this.connection.state !== HubConnectionState.Disconnected) return;

    this.connection = new HubConnectionBuilder()
      .withUrl(environment.hubUrl, { accessTokenFactory: () => this.auth.session()?.accessToken ?? '' })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.None)
      .build();

    this.connection.on(RECEIVE_METHOD, (notification: IncomingNotification) => this.receive(notification));
    this.connection.onreconnecting(() => this.statusState.set(HubStatus.Reconnecting));
    this.connection.onreconnected(() => this.statusState.set(HubStatus.Connected));
    this.connection.onclose(() => this.statusState.set(HubStatus.Disconnected));

    this.statusState.set(HubStatus.Connecting);
    try {
      await this.connection.start();
      this.statusState.set(HubStatus.Connected);
    } catch {
      this.statusState.set(HubStatus.Disconnected);
    }
  }

  async disconnect(): Promise<void> {
    const connection = this.connection;
    this.connection = undefined;
    this.itemsState.set([]);
    this.statusState.set(HubStatus.Disconnected);
    if (connection) await connection.stop();
  }

  sendTest(): Observable<void> {
    return this.http.post<void>(`${environment.apiUrl}/notifications/test`, {});
  }

  markAllAsRead(): void {
    this.itemsState.update((items) => items.map((item) => ({ ...item, read: true })));
  }

  markAsRead(id: string): void {
    this.itemsState.update((items) => items.map((item) => (item.id === id ? { ...item, read: true } : item)));
  }

  clear(): void {
    this.itemsState.set([]);
  }

  private receive(notification: IncomingNotification): void {
    this.itemsState.update((items) => [{ ...notification, read: false }, ...items].slice(0, MAX_ITEMS));
  }
}
