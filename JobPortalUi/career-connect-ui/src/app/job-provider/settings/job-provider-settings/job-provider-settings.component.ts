import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { JobProviderService } from '../../../core/services/job-provider.service';
import { JobProviderSettings } from '../../../core/models/job-provider/job-provider-settings.model';

@Component({
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './job-provider-settings.component.html',
  styleUrls: ['./job-provider-settings.component.scss']
})
export class JobProviderSettingsComponent implements OnInit {
  settings: JobProviderSettings | null = null;
  isLoading = true;
  isSaving = false;
  errorMessage = '';
  successMessage = '';
  newDifficulty = '';

  constructor(private jobService: JobProviderService) {}

  ngOnInit(): void {
    this.loadSettings();
  }

  loadSettings(): void {
    this.isLoading = true;
    this.errorMessage = '';
    this.jobService.getJobProviderSettings().subscribe({
      next: res => {
        this.settings = this.normalizeSettings(res);
      },
      error: () => {
        this.errorMessage = 'Unable to load settings. Please try again.';
        this.isLoading = false;
      },
      complete: () => {
        this.isLoading = false;
      }
    });
  }

  addDifficulty(): void {
    if (!this.settings) return;
    const value = this.newDifficulty.trim();
    if (!value) return;

    const existing = this.settings.interview.difficultyLevels
      .map(level => level.toLowerCase());

    if (!existing.includes(value.toLowerCase())) {
      this.settings.interview.difficultyLevels.push(value);
      if (!this.settings.interview.defaultDifficulty) {
        this.settings.interview.defaultDifficulty = value;
      }
    }

    this.newDifficulty = '';
  }

  removeDifficulty(index: number): void {
    if (!this.settings) return;
    const removed = this.settings.interview.difficultyLevels.splice(index, 1)[0];
    if (!removed) return;

    if (this.settings.interview.defaultDifficulty === removed) {
      this.settings.interview.defaultDifficulty =
        this.settings.interview.difficultyLevels[0] ?? '';
    }
  }

  save(): void {
    if (!this.settings) return;

    this.errorMessage = '';
    this.successMessage = '';

    if (this.settings.interview.difficultyLevels.length === 0) {
      this.errorMessage = 'Add at least one difficulty level before saving.';
      return;
    }

    if (!this.settings.interview.defaultDifficulty) {
      this.settings.interview.defaultDifficulty =
        this.settings.interview.difficultyLevels[0];
    }

    this.isSaving = true;
    this.jobService.updateJobProviderSettings(this.settings).subscribe({
      next: res => {
        this.settings = this.normalizeSettings(res);
        this.successMessage = 'Settings updated successfully.';
      },
      error: () => {
        this.errorMessage = 'Unable to save settings. Please try again.';
        this.isSaving = false;
      },
      complete: () => {
        this.isSaving = false;
      }
    });
  }

  get difficultyOptions(): string[] {
    return this.settings?.interview?.difficultyLevels ?? [];
  }

  get isAzureProvider(): boolean {
    const provider = this.settings?.ai?.provider ?? '';
    return provider.toLowerCase().startsWith('azure');
  }

  private normalizeSettings(res: any): JobProviderSettings {
    return {
      interview: {
        difficultyLevels: Array.isArray(res?.interview?.difficultyLevels)
          ? res.interview.difficultyLevels.filter((x: string) => !!x)
          : [],
        defaultDifficulty: res?.interview?.defaultDifficulty ?? '',
        questionsCount: Number(res?.interview?.questionsCount ?? 0),
        slotDurationMinutes: Number(res?.interview?.slotDurationMinutes ?? 0),
        slotCount: Number(res?.interview?.slotCount ?? 0)
      },
      ai: {
        enableShortlistSuggestions: !!res?.ai?.enableShortlistSuggestions,
        enableInterviewAi: !!res?.ai?.enableInterviewAi,
        provider: res?.ai?.provider ?? 'OpenAI',
        endpoint: res?.ai?.endpoint ?? '',
        deployment: res?.ai?.deployment ?? '',
        apiVersion: res?.ai?.apiVersion ?? '',
        enableAvatar: !!res?.ai?.enableAvatar,
        avatarProvider: res?.ai?.avatarProvider ?? ''
      },
      email: {
        inviteSubject: res?.email?.inviteSubject ?? '',
        inviteBody: res?.email?.inviteBody ?? ''
      }
    };
  }
}
