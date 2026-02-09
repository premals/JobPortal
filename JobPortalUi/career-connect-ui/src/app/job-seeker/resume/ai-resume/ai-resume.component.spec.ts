import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { AiResumeComponent } from './ai-resume.component';
import { JobSeekerProfileService } from '../../../core/services/job-seeker-profile.service';
import { ToastService } from '../../../core/services/toast.service';
import { JobSeekerProfile } from '../../../core/models/job-seeker/job-seeker-profile.model';
import { ResumeDraft } from '../../../core/models/job-seeker/resume-draft.model';

describe('AiResumeComponent', () => {
  let component: AiResumeComponent;
  let fixture: ComponentFixture<AiResumeComponent>;
  let profileServiceStub: Partial<JobSeekerProfileService>;
  let toastServiceStub: Partial<ToastService>;

  beforeEach(async () => {
    const mockProfile: JobSeekerProfile = {
      fullName: 'Test User',
      email: 'test@example.com',
      skills: [],
      experienceYears: 0,
      education: '',
      workHistory: [],
      educationHistory: [],
      projects: [],
      certifications: [],
      languages: [],
      resumeSettings: { atsFriendly: true, template: 'modern' }
    };

    const mockDraft: ResumeDraft = {
      template: 'modern',
      atsFriendly: true,
      fullName: 'Test User',
      headline: 'Engineer',
      email: 'test@example.com',
      phone: '',
      location: '',
      summary: '',
      skills: [],
      workHistory: [],
      educationHistory: [],
      projects: [],
      certifications: [],
      languages: []
    };

    profileServiceStub = {
      getProfile: () => of(mockProfile),
      getResumeDraft: () => of(mockDraft),
      saveResumeDraft: () => of(mockDraft),
      generateResumePdf: () => of(new Blob()),
      parseResume: () => of({}),
      parseResumeFile: () => of({})
    } as Partial<JobSeekerProfileService>;

    toastServiceStub = {
      show: () => undefined
    };

    await TestBed.configureTestingModule({
      imports: [AiResumeComponent],
      providers: [
        { provide: JobSeekerProfileService, useValue: profileServiceStub },
        { provide: ToastService, useValue: toastServiceStub }
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AiResumeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
