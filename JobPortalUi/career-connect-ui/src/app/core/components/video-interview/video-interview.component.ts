import { Component, Input, Output, EventEmitter, ViewChild, ElementRef, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Interview, InterviewQuestion } from '../../models/job-seeker/interview.model';
import { InterviewService } from '../../services/interview.service';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-video-interview',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="interview-container" #interviewRoot>
      <div class="interview-header">
        <div>
          <span class="eyebrow">Video Interview</span>
          <h2>Record Your Answers</h2>
          <p class="progress">Question {{ currentQuestionIndex + 1 }} of {{ questions.length }}</p>
        </div>
        <div class="integrity-badge" [class.blocked]="isMobileDevice">
          <i class="bi" [ngClass]="isMobileDevice ? 'bi-shield-x' : 'bi-shield-check'"></i>
          {{ isMobileDevice ? 'Mobile Not Allowed' : 'Integrity Check' }}
        </div>
      </div>

      <div class="interview-content">
        <div class="question-section">
          <div class="question-card">
            <h3>{{ currentQuestion?.question }}</h3>
            <p *ngIf="currentQuestion?.description" class="description">
              {{ currentQuestion?.description }}
            </p>
            <p class="duration">
              Expected duration: <strong>{{ currentQuestion?.expectedDuration }}s</strong>
            </p>
          </div>
        </div>

        <div class="video-section">
          <div class="integrity-panel">
            <div class="integrity-item" [class.ok]="isCameraReady" [class.warn]="!isCameraReady">
              <i class="bi" [ngClass]="isCameraReady ? 'bi-check-circle-fill' : 'bi-exclamation-triangle-fill'"></i>
              <span>Camera connected</span>
            </div>
            <div class="integrity-item" [class.ok]="!isMobileDevice" [class.warn]="isMobileDevice">
              <i class="bi" [ngClass]="!isMobileDevice ? 'bi-check-circle-fill' : 'bi-x-circle-fill'"></i>
              <span>Desktop only</span>
            </div>
            <div class="integrity-item" [class.ok]="isFullscreen" [class.warn]="!isFullscreen">
              <i class="bi" [ngClass]="isFullscreen ? 'bi-check-circle-fill' : 'bi-arrows-fullscreen'"></i>
              <span>Fullscreen mode</span>
            </div>
            <div class="integrity-item" [class.ok]="isPageVisible" [class.warn]="!isPageVisible">
              <i class="bi" [ngClass]="isPageVisible ? 'bi-check-circle-fill' : 'bi-eye-slash-fill'"></i>
              <span>Keep this tab active</span>
            </div>
          </div>

          <div class="video-container">
            <video #videoElement
                   class="video-preview"
                   [muted]="isRecording"
                   autoplay
                   playsinline>
            </video>

            <div *ngIf="recordedBlob && !isRecording" class="video-recorded">
              <p class="recorded-label">Recorded</p>
            </div>
          </div>

          <div class="recording-controls">
            <div class="timer" [class.warning]="currentExpectedDuration !== null && recordingTime > currentExpectedDuration * 0.8">
              {{ formatTime(recordingTime) }}
            </div>

            <div class="button-group">
              <button class="btn btn-danger" (click)="startRecording()"
                      *ngIf="!isRecording && !recordedBlob"
                      [disabled]="isCameraLoading || !canAttemptRecording">
                {{ isCameraLoading ? 'Loading Camera...' : 'Record' }}
              </button>

              <button class="btn btn-warning" (click)="stopRecording()"
                      *ngIf="isRecording">
                Stop
              </button>

              <button class="btn btn-secondary" (click)="retakeVideo()"
                      *ngIf="recordedBlob && !isRecording">
                Retake
              </button>
            </div>
          </div>

          <div class="upload-alternative">
            <p>Or upload a video file:</p>
            <input type="file" #fileInput (change)="onFileSelected($event)"
                   accept="video/*" style="display: none">
            <button class="btn btn-secondary btn-small" (click)="fileInput.click()">
              Choose File
            </button>
          </div>

          <div class="integrity-warning" *ngIf="integrityWarnings.length > 0">
            <i class="bi bi-exclamation-diamond"></i>
            <div>
              <strong>Integrity alert</strong>
              <p>{{ integrityWarnings[integrityWarnings.length - 1] }}</p>
            </div>
          </div>
        </div>
      </div>

      <div class="interview-footer">
        <button class="btn btn-secondary" (click)="goToPreviousQuestion()"
                [disabled]="currentQuestionIndex === 0 || !recordedBlob">
          Previous
        </button>

        <div class="question-status">
          <span *ngFor="let q of questions; let i = index"
                class="status-dot"
                [class.answered]="answeredQuestions.has(i)"
                [class.current]="i === currentQuestionIndex"
                (click)="goToQuestion(i)"
                [title]="'Question ' + (i + 1)">
          </span>
        </div>

        <button class="btn btn-primary" (click)="goToNextQuestion()"
                *ngIf="currentQuestionIndex < questions.length - 1"
                [disabled]="!recordedBlob">
          Next
        </button>

        <button class="btn btn-success" (click)="submitInterview()"
                *ngIf="currentQuestionIndex === questions.length - 1"
                [disabled]="!allQuestionsAnswered || isSubmitting">
          {{ isSubmitting ? 'Submitting...' : 'Submit Interview' }}
        </button>
      </div>
    </div>
  `,
  styles: [`
    .interview-container {
      background: white;
      border-radius: 18px;
      box-shadow: 0 18px 36px rgba(15, 23, 42, 0.12);
      overflow: hidden;
      font-family: "Sora", "Segoe UI", sans-serif;
    }

    .interview-header {
      background: linear-gradient(135deg, #0f766e 0%, #0ea5e9 50%, #22c55e 100%);
      color: white;
      padding: 24px;
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: 16px;
    }

    .eyebrow {
      text-transform: uppercase;
      letter-spacing: 0.14em;
      font-size: 0.7rem;
      opacity: 0.85;
    }

    .interview-header h2 {
      margin: 6px 0 8px 0;
      font-size: 24px;
    }

    .progress {
      margin: 0;
      font-size: 13px;
      opacity: 0.9;
    }

    .integrity-badge {
      background: rgba(15, 23, 42, 0.35);
      padding: 8px 14px;
      border-radius: 999px;
      font-size: 12px;
      display: inline-flex;
      align-items: center;
      gap: 6px;
      font-weight: 600;
    }

    .integrity-badge.blocked {
      background: rgba(239, 68, 68, 0.7);
    }

    .interview-content {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 24px;
      padding: 24px;
      min-height: 500px;
    }

    .question-section {
      display: flex;
      align-items: center;
    }

    .question-card {
      width: 100%;
      padding: 20px;
      background: #f8fafc;
      border-left: 4px solid #0ea5e9;
      border-radius: 12px;
    }

    .question-card h3 {
      margin: 0 0 12px 0;
      font-size: 18px;
      font-weight: 600;
      color: #0f172a;
    }

    .description {
      margin: 0 0 12px 0;
      color: #64748b;
      font-size: 13px;
      line-height: 1.6;
    }

    .duration {
      margin: 0;
      padding: 8px 12px;
      background: #ecfeff;
      border-radius: 8px;
      color: #0ea5e9;
      font-size: 12px;
    }

    .video-section {
      display: flex;
      flex-direction: column;
      gap: 16px;
    }

    .integrity-panel {
      display: grid;
      grid-template-columns: repeat(2, minmax(0, 1fr));
      gap: 10px;
      background: #f8fafc;
      border-radius: 12px;
      padding: 12px;
      border: 1px solid #e2e8f0;
    }

    .integrity-item {
      display: flex;
      align-items: center;
      gap: 8px;
      font-size: 12px;
      color: #64748b;
      padding: 6px 8px;
      border-radius: 8px;
    }

    .integrity-item.ok {
      color: #0f766e;
      background: #ecfeff;
    }

    .integrity-item.warn {
      color: #b45309;
      background: #fffbeb;
    }

    .video-container {
      position: relative;
      background: #000;
      border-radius: 12px;
      overflow: hidden;
      aspect-ratio: 4 / 3;
    }

    .video-preview {
      width: 100%;
      height: 100%;
      object-fit: cover;
    }

    .video-recorded {
      position: absolute;
      top: 8px;
      right: 8px;
      background: #22c55e;
      color: white;
      padding: 6px 12px;
      border-radius: 999px;
      font-size: 12px;
      font-weight: 600;
    }

    .recorded-label {
      position: absolute;
      top: 50%;
      left: 50%;
      transform: translate(-50%, -50%);
      color: white;
      font-size: 16px;
      font-weight: 600;
    }

    .recording-controls {
      display: flex;
      flex-direction: column;
      gap: 12px;
      align-items: center;
    }

    .timer {
      font-size: 24px;
      font-weight: 600;
      color: #0f766e;
      font-family: monospace;
      min-width: 100px;
      text-align: center;
    }

    .timer.warning {
      color: #f59e0b;
    }

    .button-group {
      display: flex;
      gap: 8px;
      justify-content: center;
      flex-wrap: wrap;
      width: 100%;
    }

    .btn {
      padding: 10px 20px;
      border: none;
      border-radius: 999px;
      cursor: pointer;
      font-weight: 600;
      transition: transform 0.2s ease;
      white-space: nowrap;
    }

    .btn-danger {
      background: #ef4444;
      color: white;
    }

    .btn-warning {
      background: #f59e0b;
      color: white;
    }

    .btn-secondary {
      background: #e2e8f0;
      color: #1f2937;
    }

    .btn-primary {
      background: #0ea5e9;
      color: white;
    }

    .btn-success {
      background: #22c55e;
      color: white;
    }

    .btn-small {
      padding: 6px 12px;
      font-size: 12px;
    }

    .btn:disabled {
      opacity: 0.5;
      cursor: not-allowed;
    }

    .btn:hover:not(:disabled) {
      transform: translateY(-1px);
    }

    .upload-alternative {
      text-align: center;
      padding: 12px;
      background: #f1f5f9;
      border-radius: 10px;
    }

    .upload-alternative p {
      margin: 0 0 8px 0;
      font-size: 12px;
      color: #64748b;
    }

    .integrity-warning {
      display: flex;
      gap: 12px;
      align-items: flex-start;
      padding: 12px;
      border-radius: 12px;
      background: #fff7ed;
      color: #9a3412;
      border: 1px solid #fed7aa;
      font-size: 12px;
    }

    .interview-footer {
      padding: 20px 24px;
      background: #fafafa;
      border-top: 1px solid #e2e8f0;
      display: flex;
      gap: 12px;
      align-items: center;
      justify-content: space-between;
      flex-wrap: wrap;
    }

    .question-status {
      display: flex;
      gap: 8px;
      align-items: center;
    }

    .status-dot {
      width: 12px;
      height: 12px;
      border-radius: 50%;
      background: #cbd5f5;
      cursor: pointer;
      transition: transform 0.2s ease;
      border: 2px solid transparent;
    }

    .status-dot.answered {
      background: #22c55e;
    }

    .status-dot.current {
      background: #0ea5e9;
      transform: scale(1.3);
      border-color: #0ea5e9;
    }

    .status-dot:hover {
      transform: scale(1.2);
    }

    @media (max-width: 1024px) {
      .interview-content {
        grid-template-columns: 1fr;
      }
    }
  `]
})
export class VideoInterviewComponent implements OnInit {
  @Input() interview!: Interview;
  @Output() submitted = new EventEmitter<Interview>();
  @Output() closed = new EventEmitter<void>();

  @ViewChild('videoElement') videoElement!: ElementRef<HTMLVideoElement>;
  @ViewChild('interviewRoot') interviewRoot!: ElementRef<HTMLDivElement>;

  questions: InterviewQuestion[] = [];
  currentQuestionIndex = 0;
  currentQuestion: InterviewQuestion | null = null;
  currentExpectedDuration: number | null = null;

  isRecording = false;
  recordingTime = 0;
  recordedBlob: Blob | null = null;
  isCameraLoading = false;
  isSubmitting = false;
  isCameraReady = false;
  isMobileDevice = false;
  isFullscreen = false;
  isPageVisible = true;
  integrityWarnings: string[] = [];

  private mediaRecorder: MediaRecorder | null = null;
  private recordedChunks: Blob[] = [];
  private recordingInterval: any;
  private stream: MediaStream | null = null;
  answeredQuestions = new Set<number>();
  private readonly isBrowser = typeof window !== 'undefined' && typeof document !== 'undefined';
  private visibilityHandler = () => {
    if (!this.isBrowser) return;
    this.isPageVisible = document.visibilityState === 'visible';
    if (!this.isPageVisible) {
      this.raiseIntegrityWarning('Interview paused because the tab is no longer visible.');
      if (this.isRecording) {
        this.stopRecording();
      }
    }
  };
  private blurHandler = () => {
    if (!this.isBrowser) return;
    this.isPageVisible = false;
    this.raiseIntegrityWarning('Interview paused because focus left the interview window.');
    if (this.isRecording) {
      this.stopRecording();
    }
  };
  private focusHandler = () => {
    if (!this.isBrowser) return;
    this.isPageVisible = true;
  };
  private fullscreenHandler = () => {
    if (!this.isBrowser) return;
    this.isFullscreen = !!document.fullscreenElement;
    if (!this.isFullscreen && this.isRecording) {
      this.raiseIntegrityWarning('Recording stopped because fullscreen mode was exited.');
      this.stopRecording();
    }
  };

  get allQuestionsAnswered(): boolean {
    return this.answeredQuestions.size === this.questions.length;
  }

  constructor(
    private interviewService: InterviewService,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.questions = this.interview.questions || [];
    if (this.questions.length > 0) {
      this.currentQuestion = this.questions[0];
      this.currentExpectedDuration = this.currentQuestion.expectedDuration ?? null;
    }
    this.isMobileDevice = this.detectMobileDevice();
    if (this.isBrowser) {
      this.isPageVisible = document.visibilityState === 'visible';
      this.isFullscreen = !!document.fullscreenElement;
      document.addEventListener('visibilitychange', this.visibilityHandler);
      window.addEventListener('blur', this.blurHandler);
      window.addEventListener('focus', this.focusHandler);
      document.addEventListener('fullscreenchange', this.fullscreenHandler);
    } else {
      this.isPageVisible = true;
      this.isFullscreen = false;
    }
    this.initializeCamera();
  }

  private initializeCamera(): void {
    this.isCameraLoading = true;
    if (typeof navigator === 'undefined' || !navigator.mediaDevices?.getUserMedia) {
      this.isCameraLoading = false;
      this.toastService.show('Camera access is not available in this environment.');
      return;
    }

    navigator.mediaDevices.getUserMedia({
      video: { width: { ideal: 1280 }, height: { ideal: 720 } },
      audio: true
    }).then(stream => {
      this.stream = stream;
      if (this.videoElement) {
        this.videoElement.nativeElement.srcObject = stream;
      }
      this.isCameraReady = true;
      this.isCameraLoading = false;
    }).catch(err => {
      console.error('Camera access error:', err);
      this.toastService.show('Unable to access camera. Please check permissions.');
      this.isCameraReady = false;
      this.isCameraLoading = false;
    });
  }

  startRecording(): void {
    if (this.isMobileDevice) {
      this.toastService.show('Recording is only available on desktop.');
      return;
    }

    if (!this.isCameraReady || !this.stream) {
      this.toastService.show('Camera is not ready yet.');
      return;
    }

    if (!this.isPageVisible) {
      this.toastService.show('Keep the interview tab active to start recording.');
      return;
    }

    if (!this.isFullscreen) {
      this.requestFullscreen();
      this.toastService.show('Please enter fullscreen to start recording.');
      return;
    }

    if (!this.stream) return;

    this.recordedChunks = [];
    const options = { mimeType: 'video/webm' };
    this.mediaRecorder = new MediaRecorder(this.stream, options);

    this.mediaRecorder.ondataavailable = (event) => {
      this.recordedChunks.push(event.data);
    };

    this.mediaRecorder.onstop = () => {
      this.recordedBlob = new Blob(this.recordedChunks, { type: 'video/webm' });
      this.answeredQuestions.add(this.currentQuestionIndex);
    };

    this.mediaRecorder.start();
    this.isRecording = true;
    this.recordingTime = 0;

    this.recordingInterval = setInterval(() => {
      this.recordingTime++;
    }, 1000);
  }

  stopRecording(): void {
    if (this.mediaRecorder && this.isRecording) {
      this.mediaRecorder.stop();
      this.isRecording = false;
      clearInterval(this.recordingInterval);
    }
  }

  retakeVideo(): void {
    this.recordedBlob = null;
    this.recordingTime = 0;
    this.answeredQuestions.delete(this.currentQuestionIndex);
  }

  onFileSelected(event: Event): void {
    const target = event.target as HTMLInputElement;
    const files = target.files;
    if (files && files.length > 0) {
      this.recordedBlob = files[0];
      this.answeredQuestions.add(this.currentQuestionIndex);
      this.toastService.show('Video uploaded successfully');
    }
  }

  goToPreviousQuestion(): void {
    if (this.currentQuestionIndex > 0) {
      this.goToQuestion(this.currentQuestionIndex - 1);
    }
  }

  goToNextQuestion(): void {
    if (this.currentQuestionIndex < this.questions.length - 1) {
      this.goToQuestion(this.currentQuestionIndex + 1);
    }
  }

  goToQuestion(index: number): void {
    this.currentQuestionIndex = index;
    this.currentQuestion = this.questions[index];
    this.currentExpectedDuration = this.currentQuestion?.expectedDuration ?? null;
    this.recordedBlob = null;
    this.recordingTime = 0;
    this.stopRecording();
  }

  submitInterview(): void {
    if (!this.allQuestionsAnswered) {
      this.toastService.show('Please answer all questions');
      return;
    }

    this.isSubmitting = true;
    this.interviewService.completeInterview(this.interview.id)
      .subscribe({
        next: (updated) => {
          this.isSubmitting = false;
          this.toastService.show('Interview submitted successfully!');
          this.submitted.emit(updated);
        },
        error: () => {
          this.isSubmitting = false;
          this.toastService.show('Failed to submit interview');
        }
      });
  }

  formatTime(seconds: number): string {
    const hrs = Math.floor(seconds / 3600);
    const mins = Math.floor((seconds % 3600) / 60);
    const secs = seconds % 60;
    return `${String(hrs).padStart(2, '0')}:${String(mins).padStart(2, '0')}:${String(secs).padStart(2, '0')}`;
  }

  ngOnDestroy(): void {
    if (this.recordingInterval) clearInterval(this.recordingInterval);
    if (this.stream) {
      this.stream.getTracks().forEach(track => track.stop());
    }
    if (this.isBrowser) {
      document.removeEventListener('visibilitychange', this.visibilityHandler);
      window.removeEventListener('blur', this.blurHandler);
      window.removeEventListener('focus', this.focusHandler);
      document.removeEventListener('fullscreenchange', this.fullscreenHandler);
    }
  }

  get canAttemptRecording(): boolean {
    return !this.isMobileDevice && this.isCameraReady && this.isPageVisible;
  }

  private detectMobileDevice(): boolean {
    if (typeof navigator === 'undefined') return false;
    const ua = navigator.userAgent || '';
    const isMobileUA = /Android|iPhone|iPad|iPod|IEMobile|Opera Mini/i.test(ua);
    const smallViewport = typeof window !== 'undefined' && window.innerWidth < 820;
    return isMobileUA || smallViewport;
  }

  private requestFullscreen(): void {
    if (!this.interviewRoot?.nativeElement) return;
    const el = this.interviewRoot.nativeElement;
    if (el.requestFullscreen) {
      el.requestFullscreen().catch(() => {
        this.toastService.show('Fullscreen permission denied.');
      });
    }
  }

  private raiseIntegrityWarning(message: string): void {
    this.integrityWarnings.push(message);
    if (this.integrityWarnings.length > 5) {
      this.integrityWarnings.shift();
    }
  }
}
