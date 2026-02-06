import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Notification, NotificationResponse, SendNotificationRequest } from '../models/notification/notification.model';
import { BehaviorSubject, Observable } from 'rxjs';
import { map } from 'rxjs/operators';

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private readonly baseUrl = environment.apiUrl;
  private unreadCountSubject = new BehaviorSubject<number>(0);
  public unreadCount$ = this.unreadCountSubject.asObservable();

  private notificationsSubject = new BehaviorSubject<Notification[]>([]);
  public notifications$ = this.notificationsSubject.asObservable();

  constructor(private http: HttpClient) {}

  /**
   * Get all notifications for the current user
   */
  getNotifications(): Observable<Notification[]> {
    return this.http.get<NotificationResponse[]>(`${this.baseUrl}/jobseeker/notifications`)
      .pipe(
        map(notifications => notifications.map(n => this.mapNotification(n)))
      );
  }

  /**
   * Get unread notification count
   */
  getUnreadCount(): Observable<number> {
    return this.http.get<{ count: number }>(`${this.baseUrl}/jobseeker/notifications/unread-count`)
      .pipe(
        map(res => res.count)
      );
  }

  /**
   * Mark notification as read
   */
  markAsRead(notificationId: string): Observable<void> {
    return this.http.put<void>(
      `${this.baseUrl}/jobseeker/notifications/${notificationId}/read`,
      {}
    );
  }

  /**
   * Mark all notifications as read
   */
  markAllAsRead(): Observable<void> {
    return this.http.put<void>(
      `${this.baseUrl}/jobseeker/notifications/read-all`,
      {}
    );
  }

  /**
   * Delete a notification
   */
  deleteNotification(notificationId: string): Observable<void> {
    return this.http.delete<void>(
      `${this.baseUrl}/jobseeker/notifications/${notificationId}`
    );
  }

  /**
   * Send notification (admin/system use)
   */
  sendNotification(request: SendNotificationRequest): Observable<Notification> {
    return this.http.post<NotificationResponse>(
      `${this.baseUrl}/jobseeker/notifications/send`,
      request
    ).pipe(
      map(n => this.mapNotification(n))
    );
  }

  /**
   * Load and cache notifications
   */
  loadNotifications(): void {
    this.getNotifications().subscribe({
      next: (notifications) => {
        this.notificationsSubject.next(notifications);
        const unreadCount = notifications.filter(n => !n.read).length;
        this.unreadCountSubject.next(unreadCount);
      }
    });
  }

  /**
   * Get unread notifications
   */
  getUnreadNotifications(): Observable<Notification[]> {
    return this.notifications$.pipe(
      map(notifications => notifications.filter(n => !n.read))
    );
  }

  /**
   * Map response to notification model
   */
  private mapNotification(response: NotificationResponse): Notification {
    return {
      ...response,
      type: this.normalizeType(response.type),
      createdAt: new Date(response.createdAt)
    };
  }

  private normalizeType(type: string): Notification['type'] {
    const value = String(type || '').toLowerCase();
    if (value.includes('shortlist')) return 'shortlist';
    if (value.includes('invite')) return 'invitation';
    if (value.includes('interview')) return 'interview';
    if (value.includes('status')) return 'application-status';
    return 'message';
  }
}
