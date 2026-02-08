import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { JobSeekerProfileService } from '../../../core/services/job-seeker-profile.service';
import { ToastService } from '../../../core/services/toast.service';
import { ResumeParseResult } from '../../../core/models/job-seeker/resume-parse-result.model';
import { ResumeDraft, ResumeDraftRequest } from '../../../core/models/job-seeker/resume-draft.model';
import { JobSeekerProfile } from '../../../core/models/job-seeker/job-seeker-profile.model';

type ResumeTemplateId = 'modern' | 'classic' | 'creative';

interface ResumeTemplateOption {
  id: ResumeTemplateId;
  name: string;
  description: string;
  gradient: string;
  recommended?: boolean;
}

@Component({
  selector: 'app-ai-resume',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './ai-resume.component.html',
  styleUrl: './ai-resume.component.scss'
})
export class AiResumeComponent implements OnInit {
  templates: ResumeTemplateOption[] = [
    {
      id: 'modern',
      name: 'Modern Edge',
      description: 'Two-column layout with bold highlights.',
      gradient: 'linear-gradient(135deg, #0ea5e9, #22c55e)',
      recommended: true
    },
    {
      id: 'classic',
      name: 'Classic Serif',
      description: 'Single-column, ATS-friendly structure.',
      gradient: 'linear-gradient(135deg, #f97316, #f59e0b)'
    },
    {
      id: 'creative',
      name: 'Studio Pop',
      description: 'Sidebar layout for creative roles.',
      gradient: 'linear-gradient(135deg, #0f766e, #38bdf8)'
    }
  ];

  selectedTemplate: ResumeTemplateId = 'modern';
  resumeText = '';
  skillDraft = '';
  parseError = '';
  isLoadingDraft = false;
  isParsing = false;
  isSavingDraft = false;
  isGeneratingPdf = false;
  profile: JobSeekerProfile | null = null;
  draft: ResumeDraft = {
    template: 'modern',
    atsFriendly: true,
    fullName: '',
    headline: '',
    email: '',
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

  constructor(
    private profileService: JobSeekerProfileService,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.loadDraft();
  }

  get hasContent(): boolean {
    return Boolean(
      this.draft.fullName ||
      this.draft.summary ||
      this.draft.skills.length ||
      this.draft.workHistory.length ||
      this.draft.educationHistory.length ||
      this.draft.projects.length
    );
  }

  get contactItems(): string[] {
    const items = [this.draft.email, this.draft.phone, this.draft.location]
      .filter((item): item is string => Boolean(item));
    return items.length ? items : ['you@email.com', 'Phone', 'Location'];
  }

  loadDraft(): void {
    this.isLoadingDraft = true;
    this.profileService.getResumeDraft().subscribe({
      next: (draft) => {
        this.draft = this.normalizeDraft(draft);
        this.selectedTemplate = (draft.template as ResumeTemplateId) || 'modern';
        this.isLoadingDraft = false;
      },
      error: () => {
        this.isLoadingDraft = false;
        this.toastService.show('Unable to load resume draft.');
      }
    });
  }

  useProfileData(): void {
    if (!this.profile) {
      this.profileService.getProfile().subscribe({
        next: (profile) => {
          this.profile = profile;
          this.draft = this.buildDraftFromProfile(profile);
          this.syncTemplate();
          this.saveDraft(true);
        },
        error: () => {
          this.toastService.show('Unable to load profile data.');
        }
      });
      return;
    }
    this.draft = this.buildDraftFromProfile(this.profile);
    this.syncTemplate();
    this.saveDraft(true);
  }

  clearInputs(): void {
    this.resumeText = '';
    this.parseError = '';
    this.skillDraft = '';
  }

  selectTemplate(id: ResumeTemplateId): void {
    this.selectedTemplate = id;
    this.syncTemplate();
    this.saveDraft();
  }

  parseResumeText(): void {
    const text = this.resumeText.trim();
    if (!text) return;
    this.parseError = '';
    this.isParsing = true;
    this.profileService.parseResume(text).subscribe({
      next: (result) => {
        this.applyParseResult(result);
        this.toastService.show('Resume text parsed successfully.');
        this.saveDraft();
        this.isParsing = false;
      },
      error: () => {
        this.parseError = 'Unable to parse the resume text.';
        this.isParsing = false;
      }
    });
  }

  onFileSelected(event: Event): void {
    const target = event.target as HTMLInputElement;
    const file = target.files?.[0];
    if (!file) return;
    this.parseError = '';
    this.isParsing = true;
    this.profileService.parseResumeFile(file).subscribe({
      next: (result) => {
        this.applyParseResult(result);
        this.toastService.show('Resume file parsed successfully.');
        this.saveDraft();
        this.isParsing = false;
      },
      error: () => {
        this.parseError = 'Unable to parse the resume file.';
        this.isParsing = false;
      }
    });
  }

  addSkill(): void {
    const value = this.skillDraft.trim();
    if (!value) return;
    if (!this.draft.skills.includes(value)) {
      this.draft.skills = [...this.draft.skills, value];
      this.saveDraft();
    }
    this.skillDraft = '';
  }

  addSkillFromInput(event: Event): void {
    if (!(event instanceof KeyboardEvent)) return;
    if (event.key !== 'Enter') return;
    event.preventDefault();
    this.addSkill();
  }

  removeSkill(index: number): void {
    this.draft.skills = this.draft.skills.filter((_, i) => i !== index);
    this.saveDraft();
  }

  downloadResume(): void {
    if (typeof window === 'undefined') return;
    this.isGeneratingPdf = true;
    const payload = this.buildDraftRequest();
    this.profileService.generateResumePdf(payload).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `resume_${Date.now()}.pdf`;
        link.click();
        window.URL.revokeObjectURL(url);
        this.isGeneratingPdf = false;
      },
      error: () => {
        this.toastService.show('Unable to generate PDF.');
        this.isGeneratingPdf = false;
      }
    });
  }

  formatDateRange(start?: string, end?: string): string {
    const startText = start || 'Start';
    const endText = end || 'Present';
    return `${startText} - ${endText}`;
  }

  private buildDraftFromProfile(profile: JobSeekerProfile): ResumeDraft {
    return {
      template: this.selectedTemplate,
      atsFriendly: profile.resumeSettings?.atsFriendly ?? true,
      fullName: profile.fullName || '',
      headline: profile.headline || '',
      email: profile.email || '',
      phone: profile.phone || '',
      location: profile.location || '',
      summary: profile.summary || '',
      skills: profile.skills || [],
      workHistory: profile.workHistory || [],
      educationHistory: profile.educationHistory || [],
      projects: profile.projects || [],
      certifications: profile.certifications || [],
      languages: profile.languages || []
    };
  }

  private applyParseResult(result: ResumeParseResult): void {
    const base = this.draft;
    this.draft = {
      ...base,
      fullName: result.fullName ?? base.fullName,
      headline: result.headline ?? base.headline,
      email: result.email ?? base.email,
      phone: result.phone ?? base.phone,
      summary: result.summary ?? base.summary,
      skills: this.mergeList(result.skills, base.skills),
      workHistory: this.mergeList(result.workHistory, base.workHistory),
      educationHistory: this.mergeList(result.educationHistory, base.educationHistory),
      projects: this.mergeList(result.projects, base.projects),
      certifications: this.mergeList(result.certifications, base.certifications),
      languages: this.mergeList(result.languages, base.languages),
      location: base.location
    };
    this.syncTemplate();
  }

  saveDraft(showToast = false): void {
    if (this.isSavingDraft) return;
    this.isSavingDraft = true;
    const payload = this.buildDraftRequest();
    this.profileService.saveResumeDraft(payload).subscribe({
      next: (saved) => {
        this.draft = this.normalizeDraft(saved);
        this.selectedTemplate = (saved.template as ResumeTemplateId) || this.selectedTemplate;
        if (showToast) {
          this.toastService.show('Resume draft saved.');
        }
        this.isSavingDraft = false;
      },
      error: () => {
        this.isSavingDraft = false;
        if (showToast) {
          this.toastService.show('Unable to save resume draft.');
        }
      }
    });
  }

  private buildDraftRequest(): ResumeDraftRequest {
    return {
      ...this.draft,
      template: this.selectedTemplate,
      atsFriendly: this.draft.atsFriendly ?? true
    };
  }

  private syncTemplate(): void {
    this.draft.template = this.selectedTemplate;
  }

  private normalizeDraft(draft: ResumeDraft): ResumeDraft {
    return {
      template: draft.template || 'modern',
      atsFriendly: draft.atsFriendly ?? true,
      fullName: draft.fullName || '',
      headline: draft.headline || '',
      email: draft.email || '',
      phone: draft.phone || '',
      location: draft.location || '',
      summary: draft.summary || '',
      skills: draft.skills || [],
      workHistory: draft.workHistory || [],
      educationHistory: draft.educationHistory || [],
      projects: draft.projects || [],
      certifications: draft.certifications || [],
      languages: draft.languages || [],
      aiGeneratedText: draft.aiGeneratedText,
      lastParsedAt: draft.lastParsedAt,
      lastGeneratedAt: draft.lastGeneratedAt,
      createdAt: draft.createdAt,
      updatedAt: draft.updatedAt
    };
  }

  private mergeList<T>(incoming: T[] | undefined, fallback: T[]): T[] {
    return Array.isArray(incoming) && incoming.length ? incoming : fallback;
  }
}
