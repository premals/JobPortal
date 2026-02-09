import {
  CertificationRecord,
  EducationRecord,
  LanguageRecord,
  ProjectRecord,
  WorkExperience
} from './job-seeker-profile.model';

export interface ResumeParseResult {
  fullName?: string;
  email?: string;
  phone?: string;
  headline?: string;
  summary?: string;
  skills?: string[];
  experienceYears?: number;
  education?: string;
  workHistory?: WorkExperience[];
  educationHistory?: EducationRecord[];
  projects?: ProjectRecord[];
  certifications?: CertificationRecord[];
  languages?: LanguageRecord[];
}
