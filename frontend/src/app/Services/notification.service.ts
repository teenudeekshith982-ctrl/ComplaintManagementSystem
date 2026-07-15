import { Injectable, signal, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../config';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { AuthService } from './auth.service';
import { Observable } from 'rxjs';

export interface NotificationItem {
    notificationId: number;
    message: string;
    isRead: boolean;
    createdAt: string;
    relatedComplaintId?: number;
    relatedComplaintTitle?: string;
}

export interface PagedNotificationsResponse {
    data: NotificationItem[];
    totalRecords: number;
    unreadCount: number;
    pageNumber: number;
    pageSize: number;
}

@Injectable({ providedIn: 'root' })
export class NotificationService {
    private http = inject(HttpClient);
    private authService = inject(AuthService);
    private baseUrl = `${environment.apiBaseUrl}/api/Notifications`;
    private hubUrl = `${environment.apiBaseUrl}/hubs/notifications`;

    private hubConnection: HubConnection | null = null;

    
    notifications = signal<NotificationItem[]>([]);
    unreadCount = signal(0);
    
    
    activeToast = signal<{ message: string; complaintId?: number } | null>(null);

    constructor() {

        if (this.authService.isLoggedIn()) {
            this.loadInitialNotifications();
            this.startHubConnection();
        }
    }

    loadInitialNotifications() {
        this.getNotifications(1, 5).subscribe({
            next: (res) => {
                this.notifications.set(res.data || []);
                this.unreadCount.set(res.unreadCount);
            },
            error: (err) => console.error('Failed to load initial notifications', err)
        });
    }

    getNotifications(pageNumber = 1, pageSize = 10): Observable<PagedNotificationsResponse> {
        return this.http.get<PagedNotificationsResponse>(`${this.baseUrl}?pageNumber=${pageNumber}&pageSize=${pageSize}`);
    }

    markAsRead(notificationId: number): Observable<any> {
        return this.http.patch(`${this.baseUrl}/${notificationId}/read`, {});
    }

    markAllAsRead(): Observable<any> {
        return this.http.patch(`${this.baseUrl}/read-all`, {});
    }

    
    startHubConnection() {
        if (this.hubConnection) return;

        const token = localStorage.getItem('token');
        if (!token) return;
            
        this.hubConnection = new HubConnectionBuilder()
            .withUrl(this.hubUrl, {
                accessTokenFactory: () => token
            })
            .configureLogging(LogLevel.Warning)
            .withAutomaticReconnect()
            .build();

        this.hubConnection.start()
            .then(() => {
                console.log('SignalR connected to NotificationHub.');
                const user = this.authService.getUserInfo();
                if (user && this.hubConnection) {
                    this.hubConnection.invoke('JoinUserGroup', user.userId.toString())
                        .catch(err => console.error('Failed to join user hub group', err));
                }
            })
            .catch(err => console.error('Error establishing SignalR connection', err));

        // Listen for incoming notifications from server
        this.hubConnection.on('ReceiveNotification', (newNotif: any) => {
            console.log('SignalR notification received:', newNotif);
            
            // Map keys
            const item: NotificationItem = {
                notificationId: newNotif.notificationId,
                message: newNotif.message,
                isRead: newNotif.isRead,
                createdAt: newNotif.createdAt,
                relatedComplaintId: newNotif.relatedComplaintId
            };

            // Prepend notification
            this.notifications.update(current => [item, ...current.slice(0, 4)]);
            this.unreadCount.update(c => c + 1);

            // Display Toast Banner
            this.activeToast.set({
                message: item.message,
                complaintId: item.relatedComplaintId
            });

            // Dismiss toast automatically after 5 seconds
            setTimeout(() => {
                this.activeToast.update(current => 
                    current?.message === item.message ? null : current
                );
            }, 5000);
        });
    }

    stopHubConnection() {
        if (this.hubConnection) {
            const user = this.authService.getUserInfo();
            if (user) {
                this.hubConnection.invoke('LeaveUserGroup', user.userId.toString())
                    .catch(err => console.error(err));
            }
            this.hubConnection.stop()
                .then(() => {
                    this.hubConnection = null;
                    console.log('SignalR connection stopped.');
                });
        }
    }
}
