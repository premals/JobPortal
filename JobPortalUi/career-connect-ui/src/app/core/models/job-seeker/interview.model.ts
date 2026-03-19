export type InvitationStatus = 'pending' | 'accepted' | 'rejected' | 'expired';
export type InterviewStatus = 'scheduled' | 'in-progress' | 'completed' | 'cancelled';

export interface InterviewTimeSlot {
  start: Date;
  end: Date;
}

export interface InterviewTimeSlotResponse {
  start: string;
  end: string;
}

export interface Interview {
  id: string;
  invitationId: string;
  jobSeekerId: string;
  jobId: string;
  jobProviderId: string;
  companyName: string;
  interviewType: 'video-recording' | 'live-interview';
  status: InterviewStatus;
  scheduledDate?: Date;
  recordingLink?: string;
  questions?: InterviewQuestion[];
  responses?: CandidateResponse[];
  analysisReport?: InterviewAnalysis;
  integrityEvents?: InterviewIntegrityEvent[];
  createdAt: Date;
  updatedAt: Date;
}

export interface InterviewQuestion {
  id: string;
  question: string;
  expectedDuration: number; // in seconds
  description?: string;
  order: number;
}

export interface CandidateResponse {
  questionId: string;
  videoUrl: string;
  duration: number; // in seconds
  recordedAt: Date;
}

export interface InterviewAnalysis {
  id: string;
  interviewId: string;
  overallScore: number; // 0-100
  communicationScore: number; // 0-100
  technicalScore: number; // 0-100
  confidenceScore: number; // 0-100
  keyStrengths: string[];
  areasForImprovement: string[];
  summary: string;
  recommendedQuestions?: string[];
  nextSteps?: string;
  generatedAt: Date;
}

export type InterviewIntegrityEventType = 'device' | 'visibility' | 'focus' | 'fullscreen' | 'system';

export interface InterviewIntegrityEvent {
  message: string;
  timestamp: string;
  type: InterviewIntegrityEventType;
}

export interface Invitation {
  id: string;
  jobSeekerId: string;
  jobId: string;
  jobProviderId: string;
  companyName?: string;
  jobTitle?: string;
  companyLogo?: string;
  message?: string;
  status: InvitationStatus;
  proposedSlots?: InterviewTimeSlot[];
  selectedSlot?: Date;
  expiresAt?: Date;
  interview?: Interview;
  createdAt: Date;
  respondedAt?: Date;
}

export interface InvitationResponse {
  id: string;
  jobSeekerId: string;
  jobId: string;
  jobProviderId: string;
  companyName?: string;
  jobTitle?: string;
  companyLogo?: string;
  message?: string;
  status: InvitationStatus;
  proposedSlots?: InterviewTimeSlotResponse[];
  selectedSlot?: string;
  expiresAt?: string;
  interview?: Interview;
  createdAt: string;
  respondedAt?: string;
}

export interface PublicInterviewInvite {
  inviteId: string;
  jobTitle: string;
  candidateName: string;
  candidateEmail: string;
  status: string;
  difficulty: string;
  questionsCount: number;
  proposedSlots: InterviewTimeSlot[];
  selectedSlot?: Date;
  tokenExpiresAt?: Date;
  sessionId?: string;
}

export interface PublicInterviewSession {
  sessionId: string;
  status: string;
  scheduledStart?: Date;
  scheduledEnd?: Date;
  questions: string[];
  avatarProvider?: string;
}

export interface AcceptInvitationRequest {
  invitationId: string;
  selectedSlot?: string;
}

export interface RejectInvitationRequest {
  invitationId: string;
  reason?: string;
}

export interface SubmitVideoResponseRequest {
  interviewId: string;
  questionId: string;
  videoUrl: string;
  duration: number;
}
