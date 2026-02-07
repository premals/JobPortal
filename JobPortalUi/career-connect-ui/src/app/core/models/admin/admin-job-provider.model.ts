export interface AdminJobProviderProjection {
  Id: string;
  JobProviderId: string;
  CompanyName: string;
  BrandName: string;
  Industry: string;
  CompanySize: string;
  Website: string;
  Phone: string;
  Location: string;
  About: string;
  LogoUrl?: string;
  LinkedInUrl?: string;
  TwitterUrl?: string;
  ContactName: string;
  ContactEmail: string;
  CreatedAt: string;
  UpdatedAt: string;
  IsActive: boolean;
}
