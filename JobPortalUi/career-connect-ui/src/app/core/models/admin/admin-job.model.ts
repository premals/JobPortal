export interface AdminJobProjection {
  JobId: string;
  JobProviderId: string;
  Title: string;
  Description: string;
  EmploymentType: string;
  WorkMode: string;
  MinExperience: number;
  MaxExperience: number;
  City: string;
  State: string;
  Country: string;
  MinSalary: number;
  MaxSalary: number;
  Currency: string;
  SalaryFrequency: string;
  KeySkills: string[];
  Education: string;
  Industry: string;
  Openings: number;
  PostedAt: string;
  ExpiryDate?: string;
  Status: string;
  ClosedAt?: string;
}
