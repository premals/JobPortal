export interface CreateJobRequest {
  title: string;
  description: string;
  employmentType: 'FullTime' | 'PartTime' | 'Contract' | string;
  workMode: 'Onsite' | 'Remote' | 'Hybrid' | string;

  minExperience: number;
  maxExperience: number;

  city: string;
  state: string;
  country: string;

  minSalary: number;
  maxSalary: number;
  currency: string;
  salaryFrequency: 'Monthly' | 'Yearly' | string;

  keySkills: string[];
  education: string;
  industry: string;

  openings: number;
  expiryDate: string; // ISO string
}
