export interface WorkExperience {
  company: string;
  role: string;
  startDate: string;
  endDate: string;
  description: string;
  skills: string[];
}

export interface EducationRecord {
  school: string;
  degree: string;
  field: string;
  graduationYear: string;
}

export interface ProjectRecord {
  name: string;
  role: string;
  description: string;
  link?: string;
}

export interface CertificationRecord {
  name: string;
  issuer: string;
  year: string;
}

export interface LanguageRecord {
  name: string;
  proficiency: string;
}

export interface ResumeSettings {
  atsFriendly: boolean;
  template: string;
}

export interface JobSeekerProfile {
  fullName: string;
  email: string;
  phone?: string;
  gender?: string;
  headline?: string;
  summary?: string;
  skills: string[];
  experienceYears: number;
  education: string;
  location?: string;
  workHistory: WorkExperience[];
  educationHistory: EducationRecord[];
  projects: ProjectRecord[];
  certifications: CertificationRecord[];
  languages: LanguageRecord[];
  resumeSettings: ResumeSettings;
}
