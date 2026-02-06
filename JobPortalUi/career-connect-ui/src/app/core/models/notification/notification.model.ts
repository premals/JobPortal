export type NotificationType = 'shortlist' | 'invitation' | 'interview' | 'application-status' | 'message';

export interface Notification {
  id: string;
  jobSeekerId: string;
  type: NotificationType;
  title: string;
  message: string;
  relatedJobId?: string;
  relatedCompanyName?: string;
  relatedCompanyLogo?: string;
  read: boolean;
  actionLink?: string;
  actionLabel?: string;
  createdAt: Date;
  metadata?: Record<string, any>;
}

export interface NotificationResponse {
  id: string;
  jobSeekerId: string;
  type: NotificationType;
  title: string;
  message: string;
  relatedJobId?: string;
  relatedCompanyName?: string;
  relatedCompanyLogo?: string;
  read: boolean;
  actionLink?: string;
  actionLabel?: string;
  createdAt: string;
  metadata?: Record<string, any>;
}

export interface SendNotificationRequest {
  jobSeekerId: string;
  type: NotificationType;
  title: string;
  message: string;
  relatedJobId?: string;
  actionLink?: string;
  actionLabel?: string;
}
