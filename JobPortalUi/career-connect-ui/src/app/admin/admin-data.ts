export type AdminUserStatus = 'Active' | 'Suspended' | 'Blocked';

export interface AdminJobSeeker {
  id: string;
  fullName: string;
  email: string;
  city: string;
  status: AdminUserStatus;
  createdAt: string;
  lastActiveAt: string;
  verifiedEmail: boolean;
  verifiedPhone: boolean;
  phone?: string;
  headline?: string;
  skills?: string[];
  notes: string[];
}

export interface AdminCandidate {
  id: string;
  fullName: string;
  email: string;
  city: string;
  status: 'Active' | 'Flagged' | 'Hidden';
  profileCompleteness: number;
  resume: {
    present: boolean;
    fileName?: string;
    updatedAt?: string;
  };
  skills: string[];
  experienceRange: string;
  lastUpdated: string;
  education: string[];
  experience: Array<{ role: string; company: string; start: string; end: string }>;
  applications: Array<{ jobTitle: string; status: string; appliedAt: string }>;
  atsScore?: number;
  adminRequests: Array<{ id: string; type: string; reason: string; createdAt: string }>;
}

export interface AdminHiringTimeline {
  stage: string;
  date: string;
  note?: string;
}

export interface AdminHiringApplication {
  id: string;
  candidateId: string;
  candidateName: string;
  jobTitle: string;
  jobProvider: string;
  city: string;
  stage: string;
  recruiter: string;
  appliedAt: string;
  lastUpdated: string;
  timeline: AdminHiringTimeline[];
  interviewNotes: string[];
  offerDetails?: string;
  decisionReason?: string;
  internalNotes: string[];
}

export interface AdminJobProvider {
  id: string;
  companyName: string;
  contactName: string;
  email: string;
  city: string;
  status: AdminUserStatus;
  createdAt: string;
  postedJobs: Array<{ id: string; title: string; status: string }>;
  hires: number;
  complaints: number;
  notes: string[];
}

export interface AdminModerationJob {
  id: string;
  title: string;
  provider: string;
  city: string;
  status: 'Active' | 'Expired' | 'Reported' | 'Hidden' | 'Scam' | 'ChangesRequested';
  postedAt: string;
  expiresAt: string;
  reports: number;
  flaggedAsScam: boolean;
  hidden: boolean;
}

export interface AdminReport {
  id: string;
  type: 'Fake Job' | 'Abuse' | 'Spam' | 'Harassment';
  targetType: 'Job' | 'Profile';
  targetName: string;
  reporter: string;
  createdAt: string;
  status: 'Open' | 'Resolved';
  recommendation: string;
}

export interface AdminAuditLog {
  id: string;
  adminUserId: string;
  action: string;
  entityType: string;
  entityId: string;
  oldValue?: string;
  newValue?: string;
  reason?: string;
  timestamp: string;
  ipAddress?: string;
}

export const adminJobSeekers: AdminJobSeeker[] = [
  {
    id: 'JS-1001',
    fullName: 'Ava Patel',
    email: 'ava.patel@example.com',
    city: 'Austin',
    status: 'Active',
    createdAt: '2025-11-12T10:20:00Z',
    lastActiveAt: '2026-02-05T16:40:00Z',
    verifiedEmail: true,
    verifiedPhone: true,
    phone: '+1-512-555-0142',
    headline: 'Product Designer',
    notes: ['Verified portfolio on 2026-01-12', 'Requested salary update on 2026-01-30']
  },
  {
    id: 'JS-1002',
    fullName: 'Mateo Chen',
    email: 'mateo.chen@example.com',
    city: 'Seattle',
    status: 'Suspended',
    createdAt: '2025-09-20T09:00:00Z',
    lastActiveAt: '2026-01-11T13:05:00Z',
    verifiedEmail: true,
    verifiedPhone: false,
    phone: '+1-206-555-0194',
    headline: 'Full Stack Engineer',
    notes: ['Suspended pending verification review.']
  },
  {
    id: 'JS-1003',
    fullName: 'Priya Singh',
    email: 'priya.singh@example.com',
    city: 'New York',
    status: 'Active',
    createdAt: '2025-12-02T08:45:00Z',
    lastActiveAt: '2026-02-06T11:20:00Z',
    verifiedEmail: true,
    verifiedPhone: true,
    phone: '+1-212-555-0103',
    headline: 'Data Analyst',
    notes: ['Requested relocation assistance.']
  },
  {
    id: 'JS-1004',
    fullName: 'Omar Farouk',
    email: 'omar.farouk@example.com',
    city: 'Chicago',
    status: 'Blocked',
    createdAt: '2025-08-14T12:30:00Z',
    lastActiveAt: '2025-12-22T09:10:00Z',
    verifiedEmail: false,
    verifiedPhone: false,
    phone: '+1-312-555-0159',
    headline: 'Sales Manager',
    notes: ['Blocked for repeated spam complaints.']
  },
  {
    id: 'JS-1005',
    fullName: 'Sofia Garcia',
    email: 'sofia.garcia@example.com',
    city: 'Miami',
    status: 'Active',
    createdAt: '2025-10-05T15:10:00Z',
    lastActiveAt: '2026-02-04T18:15:00Z',
    verifiedEmail: true,
    verifiedPhone: true,
    phone: '+1-305-555-0170',
    headline: 'Marketing Specialist',
    notes: ['Profile completeness improved to 88%.']
  },
  {
    id: 'JS-1006',
    fullName: 'Liam Johnson',
    email: 'liam.johnson@example.com',
    city: 'Denver',
    status: 'Active',
    createdAt: '2025-11-28T10:00:00Z',
    lastActiveAt: '2026-02-01T09:50:00Z',
    verifiedEmail: true,
    verifiedPhone: false,
    phone: '+1-303-555-0128',
    headline: 'Project Coordinator',
    notes: []
  }
];

export const adminCandidates: AdminCandidate[] = [
  {
    id: 'C-2101',
    fullName: 'Ava Patel',
    email: 'ava.patel@example.com',
    city: 'Austin',
    status: 'Active',
    profileCompleteness: 92,
    resume: { present: true, fileName: 'ava_patel_resume.pdf', updatedAt: '2026-01-30' },
    skills: ['Product Design', 'Figma', 'UX Research', 'Design Systems'],
    experienceRange: '5-7 yrs',
    lastUpdated: '2026-02-02',
    education: ['BFA Design - UT Austin'],
    experience: [
      { role: 'Senior Product Designer', company: 'Nimbus Labs', start: '2021-03', end: 'Present' },
      { role: 'UX Designer', company: 'Lineage', start: '2018-06', end: '2021-02' }
    ],
    applications: [
      { jobTitle: 'Lead UX Designer', status: 'Interview', appliedAt: '2026-01-18' }
    ],
    atsScore: 78,
    adminRequests: []
  },
  {
    id: 'C-2102',
    fullName: 'Mateo Chen',
    email: 'mateo.chen@example.com',
    city: 'Seattle',
    status: 'Flagged',
    profileCompleteness: 64,
    resume: { present: true, fileName: 'mateo_chen_resume.docx', updatedAt: '2025-12-20' },
    skills: ['Node.js', 'Angular', 'MongoDB'],
    experienceRange: '3-5 yrs',
    lastUpdated: '2026-01-15',
    education: ['BS Computer Science - UW'],
    experience: [
      { role: 'Full Stack Engineer', company: 'Harbor Tech', start: '2022-01', end: 'Present' }
    ],
    applications: [
      { jobTitle: 'Platform Engineer', status: 'Screening', appliedAt: '2026-01-08' }
    ],
    atsScore: 62,
    adminRequests: [{ id: 'REQ-9001', type: 'Profile Update', reason: 'Missing certifications', createdAt: '2026-01-20' }]
  },
  {
    id: 'C-2103',
    fullName: 'Sofia Garcia',
    email: 'sofia.garcia@example.com',
    city: 'Miami',
    status: 'Active',
    profileCompleteness: 88,
    resume: { present: true, fileName: 'sofia_garcia_resume.pdf', updatedAt: '2026-01-28' },
    skills: ['Brand Marketing', 'Content Strategy', 'Analytics'],
    experienceRange: '4-6 yrs',
    lastUpdated: '2026-02-03',
    education: ['MBA Marketing - FIU'],
    experience: [
      { role: 'Marketing Specialist', company: 'BluePeak Retail', start: '2020-02', end: 'Present' }
    ],
    applications: [
      { jobTitle: 'Growth Marketing Lead', status: 'Offer', appliedAt: '2026-01-12' }
    ],
    atsScore: 81,
    adminRequests: []
  },
  {
    id: 'C-2104',
    fullName: 'Liam Johnson',
    email: 'liam.johnson@example.com',
    city: 'Denver',
    status: 'Hidden',
    profileCompleteness: 41,
    resume: { present: false },
    skills: ['Project Coordination', 'Stakeholder Updates'],
    experienceRange: '2-4 yrs',
    lastUpdated: '2025-12-22',
    education: ['BA Communications - CSU'],
    experience: [
      { role: 'Project Coordinator', company: 'VectorWorks', start: '2023-05', end: 'Present' }
    ],
    applications: [],
    atsScore: 45,
    adminRequests: []
  },
  {
    id: 'C-2105',
    fullName: 'Priya Singh',
    email: 'priya.singh@example.com',
    city: 'New York',
    status: 'Active',
    profileCompleteness: 90,
    resume: { present: true, fileName: 'priya_singh_resume.pdf', updatedAt: '2026-01-26' },
    skills: ['SQL', 'Tableau', 'Python'],
    experienceRange: '3-5 yrs',
    lastUpdated: '2026-02-04',
    education: ['MS Analytics - NYU'],
    experience: [
      { role: 'Data Analyst', company: 'BrightPath Health', start: '2021-07', end: 'Present' }
    ],
    applications: [
      { jobTitle: 'Senior Data Analyst', status: 'Hired', appliedAt: '2025-12-18' }
    ],
    atsScore: 86,
    adminRequests: []
  }
];

export const adminHiringApplications: AdminHiringApplication[] = [
  {
    id: 'A-5001',
    candidateId: 'C-2101',
    candidateName: 'Ava Patel',
    jobTitle: 'Lead UX Designer',
    jobProvider: 'Nimbus Labs',
    city: 'Austin',
    stage: 'Interview',
    recruiter: 'Nina Carter',
    appliedAt: '2026-01-18T14:20:00Z',
    lastUpdated: '2026-02-02T10:00:00Z',
    timeline: [
      { stage: 'Applied', date: '2026-01-18T14:20:00Z' },
      { stage: 'Screening', date: '2026-01-20T09:00:00Z', note: 'Portfolio review complete' },
      { stage: 'Interview', date: '2026-02-02T10:00:00Z', note: 'Panel scheduled' }
    ],
    interviewNotes: ['Panel scheduled for 2026-02-10.'],
    offerDetails: '',
    decisionReason: '',
    internalNotes: ['Candidate prefers remote-first team.']
  },
  {
    id: 'A-5002',
    candidateId: 'C-2105',
    candidateName: 'Priya Singh',
    jobTitle: 'Senior Data Analyst',
    jobProvider: 'BrightPath Health',
    city: 'New York',
    stage: 'Hired',
    recruiter: 'Amir Grant',
    appliedAt: '2025-12-18T11:05:00Z',
    lastUpdated: '2026-01-14T15:30:00Z',
    timeline: [
      { stage: 'Applied', date: '2025-12-18T11:05:00Z' },
      { stage: 'Screening', date: '2025-12-22T09:00:00Z' },
      { stage: 'Interview', date: '2026-01-04T13:30:00Z' },
      { stage: 'Offer', date: '2026-01-10T10:15:00Z' },
      { stage: 'Hired', date: '2026-01-14T15:30:00Z', note: 'Offer accepted' }
    ],
    interviewNotes: ['Strong analytics presentation.'],
    offerDetails: 'Base 110k, start date 2026-02-15',
    decisionReason: 'Accepted offer',
    internalNotes: ['Follow up on relocation stipend.']
  },
  {
    id: 'A-5003',
    candidateId: 'C-2102',
    candidateName: 'Mateo Chen',
    jobTitle: 'Platform Engineer',
    jobProvider: 'VectorWorks',
    city: 'Seattle',
    stage: 'Screening',
    recruiter: 'Sasha Reed',
    appliedAt: '2026-01-08T09:45:00Z',
    lastUpdated: '2026-01-15T12:30:00Z',
    timeline: [
      { stage: 'Applied', date: '2026-01-08T09:45:00Z' },
      { stage: 'Screening', date: '2026-01-15T12:30:00Z', note: 'Awaiting tech assessment' }
    ],
    interviewNotes: ['Assessment sent on 2026-01-16.'],
    offerDetails: '',
    decisionReason: '',
    internalNotes: ['Flagged for missing certification.']
  },
  {
    id: 'A-5004',
    candidateId: 'C-2103',
    candidateName: 'Sofia Garcia',
    jobTitle: 'Growth Marketing Lead',
    jobProvider: 'BluePeak Retail',
    city: 'Miami',
    stage: 'Offer',
    recruiter: 'Tara Brooks',
    appliedAt: '2026-01-12T15:20:00Z',
    lastUpdated: '2026-02-01T10:10:00Z',
    timeline: [
      { stage: 'Applied', date: '2026-01-12T15:20:00Z' },
      { stage: 'Screening', date: '2026-01-16T10:00:00Z' },
      { stage: 'Interview', date: '2026-01-23T14:00:00Z' },
      { stage: 'Offer', date: '2026-02-01T10:10:00Z', note: 'Awaiting signature' }
    ],
    interviewNotes: ['Leadership interview completed.'],
    offerDetails: 'Base 95k, bonus 12%, hybrid',
    decisionReason: '',
    internalNotes: ['Awaiting offer signature.']
  },
  {
    id: 'A-5005',
    candidateId: 'C-2104',
    candidateName: 'Liam Johnson',
    jobTitle: 'Project Coordinator',
    jobProvider: 'Nimbus Labs',
    city: 'Denver',
    stage: 'Rejected',
    recruiter: 'Nina Carter',
    appliedAt: '2025-11-25T08:15:00Z',
    lastUpdated: '2025-12-05T16:20:00Z',
    timeline: [
      { stage: 'Applied', date: '2025-11-25T08:15:00Z' },
      { stage: 'Screening', date: '2025-11-29T11:00:00Z' },
      { stage: 'Rejected', date: '2025-12-05T16:20:00Z', note: 'Role closed' }
    ],
    interviewNotes: [],
    offerDetails: '',
    decisionReason: 'Role closed',
    internalNotes: ['Send feedback email.']
  },
  {
    id: 'A-5006',
    candidateId: 'C-2102',
    candidateName: 'Mateo Chen',
    jobTitle: 'Frontend Engineer',
    jobProvider: 'Harbor Tech',
    city: 'Seattle',
    stage: 'Withdrawn',
    recruiter: 'Sasha Reed',
    appliedAt: '2025-12-10T10:10:00Z',
    lastUpdated: '2025-12-20T09:40:00Z',
    timeline: [
      { stage: 'Applied', date: '2025-12-10T10:10:00Z' },
      { stage: 'Withdrawn', date: '2025-12-20T09:40:00Z', note: 'Candidate withdrew' }
    ],
    interviewNotes: [],
    offerDetails: '',
    decisionReason: 'Candidate withdrew',
    internalNotes: []
  }
];

export const adminJobProviders: AdminJobProvider[] = [
  {
    id: 'JP-3001',
    companyName: 'Nimbus Labs',
    contactName: 'Erin Cole',
    email: 'erin.cole@nimbuslabs.com',
    city: 'Austin',
    status: 'Active',
    createdAt: '2025-06-10T09:00:00Z',
    postedJobs: [
      { id: 'J-7001', title: 'Lead UX Designer', status: 'Active' },
      { id: 'J-7002', title: 'Project Coordinator', status: 'Closed' }
    ],
    hires: 5,
    complaints: 1,
    notes: ['Premium subscription tier.']
  },
  {
    id: 'JP-3002',
    companyName: 'BrightPath Health',
    contactName: 'Dana Ortiz',
    email: 'dana.ortiz@brightpath.com',
    city: 'New York',
    status: 'Active',
    createdAt: '2025-07-14T11:30:00Z',
    postedJobs: [
      { id: 'J-7003', title: 'Senior Data Analyst', status: 'Active' }
    ],
    hires: 3,
    complaints: 0,
    notes: []
  },
  {
    id: 'JP-3003',
    companyName: 'VectorWorks',
    contactName: 'Chris Ng',
    email: 'chris.ng@vectorworks.io',
    city: 'Seattle',
    status: 'Suspended',
    createdAt: '2025-05-04T14:40:00Z',
    postedJobs: [
      { id: 'J-7004', title: 'Platform Engineer', status: 'Reported' }
    ],
    hires: 1,
    complaints: 2,
    notes: ['Suspended pending review of report queue.']
  },
  {
    id: 'JP-3004',
    companyName: 'BluePeak Retail',
    contactName: 'Maya Lopez',
    email: 'maya.lopez@bluepeak.com',
    city: 'Miami',
    status: 'Blocked',
    createdAt: '2024-12-01T10:10:00Z',
    postedJobs: [
      { id: 'J-7005', title: 'Growth Marketing Lead', status: 'Active' }
    ],
    hires: 0,
    complaints: 4,
    notes: ['Blocked for repeated fake job reports.']
  }
];

export const adminModerationJobs: AdminModerationJob[] = [
  {
    id: 'J-7001',
    title: 'Lead UX Designer',
    provider: 'Nimbus Labs',
    city: 'Austin',
    status: 'Active',
    postedAt: '2026-01-05T09:00:00Z',
    expiresAt: '2026-03-01T00:00:00Z',
    reports: 0,
    flaggedAsScam: false,
    hidden: false
  },
  {
    id: 'J-7003',
    title: 'Senior Data Analyst',
    provider: 'BrightPath Health',
    city: 'New York',
    status: 'Reported',
    postedAt: '2025-12-20T10:00:00Z',
    expiresAt: '2026-02-20T00:00:00Z',
    reports: 2,
    flaggedAsScam: false,
    hidden: false
  },
  {
    id: 'J-7004',
    title: 'Platform Engineer',
    provider: 'VectorWorks',
    city: 'Seattle',
    status: 'Reported',
    postedAt: '2025-12-14T09:30:00Z',
    expiresAt: '2026-02-01T00:00:00Z',
    reports: 3,
    flaggedAsScam: true,
    hidden: true
  },
  {
    id: 'J-7005',
    title: 'Growth Marketing Lead',
    provider: 'BluePeak Retail',
    city: 'Miami',
    status: 'Active',
    postedAt: '2026-01-08T11:00:00Z',
    expiresAt: '2026-03-05T00:00:00Z',
    reports: 1,
    flaggedAsScam: false,
    hidden: false
  },
  {
    id: 'J-7006',
    title: 'Customer Success Lead',
    provider: 'Coastal Suite',
    city: 'Denver',
    status: 'Expired',
    postedAt: '2025-10-12T10:00:00Z',
    expiresAt: '2025-12-12T00:00:00Z',
    reports: 0,
    flaggedAsScam: false,
    hidden: false
  },
  {
    id: 'J-7007',
    title: 'Finance Operations Analyst',
    provider: 'Pinecrest Capital',
    city: 'Chicago',
    status: 'ChangesRequested',
    postedAt: '2026-01-15T12:00:00Z',
    expiresAt: '2026-03-12T00:00:00Z',
    reports: 1,
    flaggedAsScam: false,
    hidden: false
  }
];

export const adminReports: AdminReport[] = [
  {
    id: 'R-8001',
    type: 'Fake Job',
    targetType: 'Job',
    targetName: 'Platform Engineer - VectorWorks',
    reporter: 'jordan.lee@example.com',
    createdAt: '2026-01-22T14:40:00Z',
    status: 'Open',
    recommendation: 'Review for scam signals'
  },
  {
    id: 'R-8002',
    type: 'Abuse',
    targetType: 'Profile',
    targetName: 'Omar Farouk',
    reporter: 'reporter@careerconnect.io',
    createdAt: '2026-01-18T09:25:00Z',
    status: 'Resolved',
    recommendation: 'User already blocked'
  },
  {
    id: 'R-8003',
    type: 'Spam',
    targetType: 'Job',
    targetName: 'Growth Marketing Lead - BluePeak Retail',
    reporter: 'sofia.garcia@example.com',
    createdAt: '2026-02-01T16:10:00Z',
    status: 'Open',
    recommendation: 'Request job description updates'
  },
  {
    id: 'R-8004',
    type: 'Harassment',
    targetType: 'Profile',
    targetName: 'Mateo Chen',
    reporter: 'anonymous',
    createdAt: '2026-01-30T13:50:00Z',
    status: 'Open',
    recommendation: 'Review messaging audit'
  }
];

export const adminMasterData = {
  skills: ['Angular', 'React', 'SQL', 'Figma', 'Python', 'Content Strategy'],
  industries: ['Healthcare', 'Fintech', 'Retail', 'SaaS', 'Education'],
  locations: ['Austin', 'New York', 'Seattle', 'Miami', 'Denver', 'Chicago'],
  employmentTypes: ['Full-time', 'Part-time', 'Contract', 'Internship'],
  workModes: ['Onsite', 'Hybrid', 'Remote'],
  emailTemplates: [
    { id: 'T-01', name: 'Application Received', status: 'Active' },
    { id: 'T-02', name: 'Interview Scheduled', status: 'Draft' },
    { id: 'T-03', name: 'Offer Sent', status: 'Active' }
  ]
};

export const adminAuditLogs: AdminAuditLog[] = [
  {
    id: 'AL-1001',
    adminUserId: 'admin-01',
    action: 'Suspend User',
    entityType: 'JobSeeker',
    entityId: 'JS-1002',
    oldValue: 'Active',
    newValue: 'Suspended',
    reason: 'Verification pending',
    timestamp: '2026-01-20T10:12:00Z',
    ipAddress: '10.2.18.44'
  },
  {
    id: 'AL-1002',
    adminUserId: 'admin-01',
    action: 'Mark Job Scam',
    entityType: 'Job',
    entityId: 'J-7004',
    oldValue: 'Reported',
    newValue: 'Scam',
    reason: 'Multiple confirmed reports',
    timestamp: '2026-01-23T09:40:00Z',
    ipAddress: '10.2.18.44'
  },
  {
    id: 'AL-1003',
    adminUserId: 'admin-02',
    action: 'Reset Password',
    entityType: 'JobProvider',
    entityId: 'JP-3003',
    oldValue: 'PasswordActive',
    newValue: 'ResetRequested',
    reason: 'Account recovery',
    timestamp: '2026-01-28T15:20:00Z',
    ipAddress: '10.2.18.19'
  },
  {
    id: 'AL-1004',
    adminUserId: 'admin-03',
    action: 'Move Stage',
    entityType: 'Application',
    entityId: 'A-5004',
    oldValue: 'Interview',
    newValue: 'Offer',
    reason: 'Offer approved',
    timestamp: '2026-02-01T10:12:00Z',
    ipAddress: '10.2.18.71'
  },
  {
    id: 'AL-1005',
    adminUserId: 'admin-02',
    action: 'Resolve Report',
    entityType: 'Report',
    entityId: 'R-8002',
    oldValue: 'Open',
    newValue: 'Resolved',
    reason: 'User blocked',
    timestamp: '2026-01-18T10:05:00Z',
    ipAddress: '10.2.18.19'
  }
];

export function addAuditLog(entry: {
  adminUserId: string;
  action: string;
  entityType: string;
  entityId: string;
  oldValue?: string;
  newValue?: string;
  reason?: string;
  timestamp?: string;
  ipAddress?: string;
}): AdminAuditLog {
  const log: AdminAuditLog = {
    id: `AL-${Date.now()}-${Math.floor(Math.random() * 1000)}`,
    adminUserId: entry.adminUserId,
    action: entry.action,
    entityType: entry.entityType,
    entityId: entry.entityId,
    oldValue: entry.oldValue,
    newValue: entry.newValue,
    reason: entry.reason,
    timestamp: entry.timestamp ?? new Date().toISOString(),
    ipAddress: entry.ipAddress ?? '10.2.18.10'
  };

  adminAuditLogs.unshift(log);
  return log;
}

