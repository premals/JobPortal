import { Component, Input, Output, EventEmitter, ViewChild, ElementRef, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Interview, InterviewQuestion, InterviewIntegrityEvent, InterviewIntegrityEventType } from '../../models/job-seeker/interview.model';
import { InterviewService } from '../../services/interview.service';
import { ToastService } from '../../services/toast.service';

interface CocoDetectedObject {
  class: string;
  score: number;
}

interface CocoModel {
  detect(input: HTMLVideoElement | HTMLCanvasElement | HTMLImageElement): Promise<CocoDetectedObject[]>;
}

@Component({
  selector: 'app-video-interview',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './video-interview.component.html',
  styleUrls: ['./video-interview.component.scss']
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
  integrityEvents: InterviewIntegrityEvent[] = [];

  private mediaRecorder: MediaRecorder | null = null;
  private recordedChunks: Blob[] = [];
  private recordingInterval: any;
  private stream: MediaStream | null = null;
  private deviceDetectionInterval: any;
  private deviceDetectionModel: CocoModel | null = null;
  private deviceDetectionLoading = false;
  private deviceDetectionInProgress = false;
  private deviceDetectionRequested = false;
  private lastDeviceWarningAt = 0;
  private readonly deviceWarningCooldownMs = 15000;
  private readonly deviceDetectionIntervalMs = 2000;
  private readonly deviceConfidenceThreshold = 0.6;
  private readonly deviceClasses = new Set(['cell phone']);
  private readonly integrityStorageKeyPrefix = 'interview_integrity_';
  answeredQuestions = new Set<number>();
  private readonly isBrowser = typeof window !== 'undefined' && typeof document !== 'undefined';
  private visibilityHandler = () => {
    if (!this.isBrowser) return;
    this.isPageVisible = document.visibilityState === 'visible';
    if (!this.isPageVisible) {
      this.raiseIntegrityWarning('Interview paused because the tab is no longer visible.', 'visibility');
      if (this.isRecording) {
        this.stopRecording();
      }
    }
  };
  private blurHandler = () => {
    if (!this.isBrowser) return;
    this.isPageVisible = false;
    this.raiseIntegrityWarning('Interview paused because focus left the interview window.', 'focus');
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
      this.raiseIntegrityWarning('Recording stopped because fullscreen mode was exited.', 'fullscreen');
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
    this.integrityEvents = this.loadIntegrityEvents();
    this.integrityWarnings = this.integrityEvents.map(event => event.message).slice(-5);
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
    this.startDeviceDetection();

    this.recordingInterval = setInterval(() => {
      this.recordingTime++;
    }, 1000);
  }

  stopRecording(): void {
    if (this.mediaRecorder && this.isRecording) {
      this.mediaRecorder.stop();
      this.isRecording = false;
      clearInterval(this.recordingInterval);
      this.stopDeviceDetection();
    }
  }

  retakeVideo(): void {
    this.recordedBlob = null;
    this.recordingTime = 0;
    this.answeredQuestions.delete(this.currentQuestionIndex);
    this.stopDeviceDetection();
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
    this.stopDeviceDetection();
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
          const enriched: Interview = {
            ...updated,
            integrityEvents: [...this.integrityEvents]
          };
          this.isSubmitting = false;
          this.toastService.show('Interview submitted successfully!');
          this.submitted.emit(enriched);
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
    this.stopDeviceDetection();
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

  private startDeviceDetection(): void {
    if (!this.isBrowser || this.deviceDetectionInterval) return;
    this.deviceDetectionRequested = true;
    void this.ensureDeviceDetectionModel().then((ready) => {
      if (!ready || !this.deviceDetectionRequested || !this.isRecording || !this.videoElement?.nativeElement) return;
      this.deviceDetectionInterval = setInterval(() => {
        void this.checkForDevices();
      }, this.deviceDetectionIntervalMs);
    });
  }

  private stopDeviceDetection(): void {
    this.deviceDetectionRequested = false;
    if (this.deviceDetectionInterval) {
      clearInterval(this.deviceDetectionInterval);
      this.deviceDetectionInterval = null;
    }
    this.deviceDetectionInProgress = false;
  }

  private async ensureDeviceDetectionModel(): Promise<boolean> {
    if (this.deviceDetectionModel) return true;
    if (this.deviceDetectionLoading) return false;
    this.deviceDetectionLoading = true;

    try {
      const tfReady = await this.loadScript(
        'https://cdn.jsdelivr.net/npm/@tensorflow/tfjs@4.0.0/dist/tf.min.js',
        'tfjs-lib'
      );
      const cocoReady = await this.loadScript(
        'https://cdn.jsdelivr.net/npm/@tensorflow-models/coco-ssd@2.2.2/dist/coco-ssd.min.js',
        'coco-ssd-lib'
      );

      if (!tfReady || !cocoReady) return false;
      const tf = (window as any).tf;
      const cocoSsd = (window as any).cocoSsd;
      if (!tf || !cocoSsd?.load) return false;

      if (tf.setBackend) {
        await tf.setBackend('webgl').catch(() => undefined);
        await tf.ready?.();
      }

      this.deviceDetectionModel = await cocoSsd.load();
      return true;
    } catch (err) {
      console.error('Device detection model failed to load:', err);
      return false;
    } finally {
      this.deviceDetectionLoading = false;
    }
  }

  private async checkForDevices(): Promise<void> {
    if (!this.deviceDetectionModel || this.deviceDetectionInProgress) return;
    const video = this.videoElement?.nativeElement;
    if (!video || video.readyState < 2) return;

    this.deviceDetectionInProgress = true;
    try {
      const predictions = await this.deviceDetectionModel.detect(video);
      const suspect = predictions.find(prediction =>
        this.deviceClasses.has(prediction.class) &&
        prediction.score >= this.deviceConfidenceThreshold
      );
      if (suspect) {
        this.handleDeviceDetected(suspect.class);
      }
    } catch (err) {
      console.error('Device detection error:', err);
    } finally {
      this.deviceDetectionInProgress = false;
    }
  }

  private handleDeviceDetected(label: string): void {
    const now = Date.now();
    if (now - this.lastDeviceWarningAt < this.deviceWarningCooldownMs) return;
    this.lastDeviceWarningAt = now;
    const readable = label === 'cell phone' ? 'mobile phone' : label;
    const message = `Possible device detected in camera (${readable}). Please remove it.`;
    this.toastService.show(message);
    this.raiseIntegrityWarning(message, 'device');
  }

  private raiseIntegrityWarning(message: string, type: InterviewIntegrityEventType = 'system'): void {
    const event: InterviewIntegrityEvent = {
      message,
      timestamp: new Date().toISOString(),
      type
    };
    this.integrityEvents.push(event);
    if (this.integrityEvents.length > 50) {
      this.integrityEvents.shift();
    }
    this.persistIntegrityEvents();
    this.integrityWarnings.push(message);
    if (this.integrityWarnings.length > 5) {
      this.integrityWarnings.shift();
    }
  }

  private loadIntegrityEvents(): InterviewIntegrityEvent[] {
    if (!this.isBrowser) return [];
    const key = this.getIntegrityStorageKey();
    if (!key) return [];
    try {
      const raw = localStorage.getItem(key);
      const parsed = raw ? JSON.parse(raw) : [];
      return Array.isArray(parsed) ? parsed : [];
    } catch {
      return [];
    }
  }

  private persistIntegrityEvents(): void {
    if (!this.isBrowser) return;
    const key = this.getIntegrityStorageKey();
    if (!key) return;
    try {
      localStorage.setItem(key, JSON.stringify(this.integrityEvents.slice(-50)));
    } catch {
      // Ignore storage errors
    }
  }

  private getIntegrityStorageKey(): string | null {
    if (!this.interview?.id) return null;
    return `${this.integrityStorageKeyPrefix}${this.interview.id}`;
  }

  private loadScript(src: string, id: string): Promise<boolean> {
    if (!this.isBrowser) return Promise.resolve(false);
    if (document.getElementById(id)) return Promise.resolve(true);

    return new Promise((resolve) => {
      const script = document.createElement('script');
      script.id = id;
      script.src = src;
      script.async = true;
      script.onload = () => resolve(true);
      script.onerror = () => resolve(false);
      document.head.appendChild(script);
    });
  }
}
