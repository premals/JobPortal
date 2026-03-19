import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import {
  Invitation,
  InvitationResponse,
  Interview,
  AcceptInvitationRequest,
  RejectInvitationRequest,
  SubmitVideoResponseRequest,
  InterviewAnalysis,
  PublicInterviewInvite,
  PublicInterviewSession
} from '../models/job-seeker/interview.model';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

@Injectable({ providedIn: 'root' })
export class InterviewService {
  private readonly baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  /**
   * Get all invitations for the current job-seeker
   */
  getInvitations(): Observable<Invitation[]> {
    return this.http.get<InvitationResponse[]>(`${this.baseUrl}/jobprovider/jobs/interviews/invites`)
      .pipe(
        map(invitations => invitations.map(inv => this.mapInvitation(inv)))
      );
  }

  /**
   * Get a public interview invite by token
   */
  getPublicInvite(token: string): Observable<PublicInterviewInvite> {
    return this.http.get<any>(
      `${this.baseUrl}/jobprovider/jobs/interviews/public/${encodeURIComponent(token)}`
    ).pipe(
      map(invite => ({
        inviteId: invite.inviteId,
        jobTitle: invite.jobTitle,
        candidateName: invite.candidateName,
        candidateEmail: invite.candidateEmail,
        status: invite.status,
        difficulty: invite.difficulty,
        questionsCount: Number(invite.questionsCount ?? 0),
        proposedSlots: (invite.proposedSlots ?? []).map((slot: any) => ({
          start: new Date(slot.start),
          end: new Date(slot.end)
        })),
        selectedSlot: invite.selectedSlot ? new Date(invite.selectedSlot) : undefined,
        tokenExpiresAt: invite.tokenExpiresAt ? new Date(invite.tokenExpiresAt) : undefined,
        sessionId: invite.sessionId ?? undefined
      }))
    );
  }

  /**
   * Accept a public invite by token
   */
  acceptPublicInvite(token: string, selectedSlot: Date): Observable<PublicInterviewSession> {
    return this.http.post<any>(
      `${this.baseUrl}/jobprovider/jobs/interviews/public/${encodeURIComponent(token)}/accept`,
      { selectedSlot }
    ).pipe(
      map(session => this.mapPublicSession(session))
    );
  }

  /**
   * Get public interview session by token
   */
  getPublicSession(token: string): Observable<PublicInterviewSession> {
    return this.http.get<any>(
      `${this.baseUrl}/jobprovider/jobs/interviews/public/${encodeURIComponent(token)}/session`
    ).pipe(
      map(session => this.mapPublicSession(session))
    );
  }

  /**
   * Start public interview session by token
   */
  startPublicSession(token: string): Observable<any> {
    return this.http.post<any>(
      `${this.baseUrl}/jobprovider/jobs/interviews/public/${encodeURIComponent(token)}/start`,
      {}
    );
  }

  /**
   * Upload video response for public interview
   */
  uploadPublicResponse(
    token: string,
    questionIndex: number,
    answerText: string,
    videoBlob: Blob,
    duration: number
  ): Observable<any> {
    const formData = new FormData();
    formData.append('questionIndex', questionIndex.toString());
    formData.append('answerText', answerText);
    formData.append('durationSeconds', duration.toString());
    formData.append('video', videoBlob, 'response.webm');

    return this.http.post(
      `${this.baseUrl}/jobprovider/jobs/interviews/public/${encodeURIComponent(token)}/upload`,
      formData
    );
  }

  /**
   * Get a single invitation by ID
   */
  getInvitation(invitationId: string): Observable<Invitation> {
    return this.http.get<InvitationResponse>(
      `${this.baseUrl}/jobseeker/invitations/${invitationId}`
    ).pipe(
      map(inv => this.mapInvitation(inv))
    );
  }

  /**
   * Accept an invitation
   */
  acceptInvitation(invitationId: string, selectedSlot?: Date): Observable<any> {
    const request: AcceptInvitationRequest = {
      invitationId,
      selectedSlot: selectedSlot ? selectedSlot.toISOString() : undefined
    };
    return this.http.post(
      `${this.baseUrl}/jobprovider/jobs/interviews/invites/${invitationId}/accept`,
      request
    );
  }

  /**
   * Reject an invitation
   */
  rejectInvitation(invitationId: string, reason?: string): Observable<void> {
    const request: RejectInvitationRequest = { invitationId, reason };
    return this.http.post<void>(
      `${this.baseUrl}/jobseeker/invitations/${invitationId}/reject`,
      request
    );
  }

  /**
   * Get interview details
   */
  getInterview(interviewId: string): Observable<Interview> {
    return this.http.get<Interview>(
      `${this.baseUrl}/jobseeker/interviews/${interviewId}`
    );
  }

  /**
   * Submit a video response to an interview question
   */
  submitVideoResponse(request: SubmitVideoResponseRequest): Observable<Interview> {
    return this.http.post<Interview>(
      `${this.baseUrl}/jobseeker/interviews/submit-response`,
      request
    );
  }

  /**
   * Complete the interview and trigger analysis
   */
  completeInterview(interviewId: string): Observable<Interview> {
    return this.http.post<Interview>(
      `${this.baseUrl}/jobseeker/interviews/${interviewId}/complete`,
      {}
    );
  }

  /**
   * Get interview analysis/report
   */
  getInterviewAnalysis(interviewId: string): Observable<InterviewAnalysis> {
    return this.http.get<InterviewAnalysis>(
      `${this.baseUrl}/jobseeker/interviews/${interviewId}/analysis`
    );
  }

  /**
   * Get all interviews for the current job-seeker
   */
  getMyInterviews(): Observable<Interview[]> {
    return this.http.get<Interview[]>(
      `${this.baseUrl}/jobseeker/interviews`
    );
  }

  /**
   * Get pending interviews (scheduled but not completed)
   */
  getPendingInterviews(): Observable<Interview[]> {
    return this.getMyInterviews().pipe(
      map(interviews => interviews.filter(i => i.status !== 'completed'))
    );
  }

  /**
   * Upload video for analysis (supports base64 or URL)
   */
  uploadVideoResponse(
    interviewId: string,
    questionId: string,
    videoBlob: Blob,
    duration: number
  ): Observable<any> {
    const formData = new FormData();
    formData.append('interviewId', interviewId);
    formData.append('questionId', questionId);
    formData.append('video', videoBlob, 'response.webm');
    formData.append('duration', duration.toString());

    return this.http.post(
      `${this.baseUrl}/jobseeker/interviews/upload-video`,
      formData
    );
  }

  /**
   * Map response to interview model with date conversions
   */
  private mapInvitation(response: InvitationResponse): Invitation {
    const statusValue = String(response.status || '').toLowerCase();
    const normalizedStatus = ['pending', 'accepted', 'rejected', 'expired'].includes(statusValue)
      ? statusValue
      : 'pending';
    return {
      ...response,
      companyName: response.companyName,
      message: response.message,
      status: normalizedStatus as Invitation['status'],
      proposedSlots: response.proposedSlots?.map(slot => ({
        start: new Date(slot.start),
        end: new Date(slot.end)
      })),
      selectedSlot: response.selectedSlot ? new Date(response.selectedSlot) : undefined,
      expiresAt: response.expiresAt ? new Date(response.expiresAt) : undefined,
      createdAt: new Date(response.createdAt),
      respondedAt: response.respondedAt ? new Date(response.respondedAt) : undefined
    };
  }

  private mapPublicSession(session: any): PublicInterviewSession {
    return {
      sessionId: session.sessionId,
      status: session.status,
      scheduledStart: session.scheduledStart ? new Date(session.scheduledStart) : undefined,
      scheduledEnd: session.scheduledEnd ? new Date(session.scheduledEnd) : undefined,
      questions: Array.isArray(session.questions) ? session.questions : [],
      avatarProvider: session.avatarProvider ?? undefined
    };
  }
}
