import {
  WorkExperience,
  EducationRecord,
  ProjectRecord,
  CertificationRecord,
  LanguageRecord
} from './job-seeker-profile.model';

export interface ResumeDraft {
  id?: string;
  template: string;
  atsFriendly: boolean;
  fullName: string;
  headline?: string;
  email: string;
  phone?: string;
  location?: string;
  summary?: string;
  skills: string[];
  experienceYears?: number;
  education?: string;
  workHistory: WorkExperience[];
  educationHistory: EducationRecord[];
  projects: ProjectRecord[];
  certifications: CertificationRecord[];
  languages: LanguageRecord[];
  aiGeneratedText?: string;
  lastParsedAt?: string;
  lastGeneratedAt?: string;
  createdAt?: string;
  updatedAt?: string;
}

export type ResumeDraftRequest = Omit<
  ResumeDraft,
  'id' | 'createdAt' | 'updatedAt' | 'lastParsedAt' | 'lastGeneratedAt'
>;
