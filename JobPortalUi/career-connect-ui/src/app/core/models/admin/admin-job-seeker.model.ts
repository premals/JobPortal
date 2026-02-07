export interface AdminJobSeekerProjection {
  Id: string;
  UserId: string;
  FullName: string;
  Email: string;
  Phone?: string;
  Headline?: string;
  Summary?: string;
  Skills: string[];
  ExperienceYears: number;
  Education: string;
  Location?: string;
  CreatedAt: string;
  UpdatedAt: string;
  LastActiveAt?: string;
  IsActive: boolean;
}
