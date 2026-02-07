import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { jobProviderGuard } from './core/guards/job-provider.guard';
import { jobSeekerGuard } from './core/guards/job-seeker.guard';
import { adminGuard } from './core/guards/admin.guard';

export const routes: Routes = [

    /* =======================
       AUTH ROUTES (PUBLIC)
       ======================= */
    {
        path: 'login',
        loadComponent: () =>
            import('./auth/login/login.component')
                .then(c => c.LoginComponent)
    },
    {
        path: 'register',
        loadComponent: () =>
            import('./auth/register/register.component')
                .then(c => c.RegisterComponent)
    },
    {
        path: 'forgot-password',
        loadComponent: () =>
            import('./auth/forgot-password/forgot-password.component')
                .then(c => c.ForgotPasswordComponent)
    },
    {
        path: 'reset-password',
        loadComponent: () =>
            import('./auth/reset-password/reset-password.component')
                .then(c => c.ResetPasswordComponent)
    },

    /* =======================
       PROFILE (AFTER LOGIN)
       ======================= */
    {
        path: 'profile',
        loadComponent: () =>
            import('./profile/view-profile/view-profile.component')
                .then(c => c.ViewProfileComponent),
        canActivate: [authGuard]
    },
    {
        path: 'profile/edit',
        loadComponent: () =>
            import('./profile/edit-profile/edit-profile.component')
                .then(c => c.EditProfileComponent),
        canActivate: [authGuard]
    },

    /* =======================
       JOB PROVIDER MODULE
       ======================= */
    {
        path: 'job-provider',
        canActivate: [authGuard, jobProviderGuard],
        loadComponent: () =>
            import('./job-provider/job-provider-layout/job-provider-layout.component')
                .then(c => c.JobProviderLayoutComponent),
        children: [
            {
                path: '',
                loadComponent: () =>
                    import('./job-provider/dashboard/job-provider-dashboard/job-provider-dashboard.component')
                        .then(c => c.JobProviderDashboardComponent)
            },
            {
                path: 'jobs',
                loadComponent: () =>
                    import('./job-provider/jobs/jobs-list/jobs-list.component')
                        .then(c => c.JobProviderJobsListComponent)
            },
            {
                path: 'jobs/create',
                loadComponent: () =>
                    import('./job-provider/jobs/create-job/create-job.component')
                        .then(c => c.CreateJobComponent)
            },
            {
                path: 'jobs/:jobId/applications',
                loadComponent: () =>
                    import('./job-provider/applications/job-applications/job-applications.component')
                        .then(c => c.JobApplicationsComponent)
            },
            {
                path: 'settings',
                loadComponent: () =>
                    import('./job-provider/settings/job-provider-settings/job-provider-settings.component')
                        .then(c => c.JobProviderSettingsComponent)
            }
        ]
    },

    
    /* =======================
       ADMIN MODULE
       ======================= */
    {
        path: 'admin',
        canActivate: [authGuard, adminGuard],
        loadComponent: () =>
            import('./admin/admin-layout/admin-layout.component')
                .then(c => c.AdminLayoutComponent),
        children: [
            {
                path: '',
                loadComponent: () =>
                    import('./admin/dashboard/admin-dashboard.component')
                        .then(c => c.AdminDashboardComponent)
            },
            {
                path: 'job-seekers',
                loadComponent: () =>
                    import('./admin/job-seekers/job-seeker-list/job-seeker-list.component')
                        .then(c => c.JobSeekerListComponent)
            },
            {
                path: 'job-seekers/:id',
                loadComponent: () =>
                    import('./admin/job-seekers/job-seeker-detail/job-seeker-detail.component')
                        .then(c => c.JobSeekerDetailComponent)
            },
            {
                path: 'candidates',
                loadComponent: () =>
                    import('./admin/candidates/candidate-list/candidate-list.component')
                        .then(c => c.CandidateListComponent)
            },
            {
                path: 'candidates/:id',
                loadComponent: () =>
                    import('./admin/candidates/candidate-detail/candidate-detail.component')
                        .then(c => c.CandidateDetailComponent)
            },
            {
                path: 'hiring',
                loadComponent: () =>
                    import('./admin/hiring/hiring-list/hiring-list.component')
                        .then(c => c.HiringListComponent)
            },
            {
                path: 'hiring/:id',
                loadComponent: () =>
                    import('./admin/hiring/hiring-detail/hiring-detail.component')
                        .then(c => c.HiringDetailComponent)
            },
            {
                path: 'job-providers',
                loadComponent: () =>
                    import('./admin/job-providers/job-provider-list/job-provider-list.component')
                        .then(c => c.JobProviderListComponent)
            },
            {
                path: 'job-providers/:id',
                loadComponent: () =>
                    import('./admin/job-providers/job-provider-detail/job-provider-detail.component')
                        .then(c => c.JobProviderDetailComponent)
            },
            {
                path: 'jobs-moderation',
                loadComponent: () =>
                    import('./admin/jobs-moderation/jobs-moderation.component')
                        .then(c => c.JobsModerationComponent)
            },
            {
                path: 'reports',
                loadComponent: () =>
                    import('./admin/reports/reports.component')
                        .then(c => c.ReportsComponent)
            },
            {
                path: 'master-data',
                loadComponent: () =>
                    import('./admin/master-data/master-data.component')
                        .then(c => c.MasterDataComponent)
            },
            {
                path: 'audit-logs',
                loadComponent: () =>
                    import('./admin/audit-logs/audit-logs.component')
                        .then(c => c.AuditLogsComponent)
            }
        ]
    },

    {
        path: 'job-seeker',
        canActivate: [authGuard, jobSeekerGuard],
        loadComponent: () =>
            import('./job-seeker/layout/layout.component')
                .then(c => c.LayoutComponent),
        children: [
            {
                path: '',
                loadComponent: () =>
                    import('./job-seeker/dashboard/dashboard.component')
                        .then(c => c.DashboardComponent)
            },
            {
                path: 'jobs',
                loadComponent: () =>
                    import('./job-seeker/jobs/job-list/job-list.component')
                        .then(c => c.JobListComponent)
            },
            {
                path: 'jobs/:id',
                loadComponent: () =>
                    import('./job-seeker/jobs/job-detail/job-detail.component')
                        .then(c => c.JobDetailComponent)
            },
            {
                path: 'applications',
                loadComponent: () =>
                    import('./job-seeker/applications/my-applications/my-applications.component')
                        .then(c => c.MyApplicationsComponent)
            },
            {
                path: 'resume',
                loadComponent: () =>
                    import('./job-seeker/resume/ai-resume/ai-resume.component')
                        .then(c => c.AiResumeComponent)
            }
        ]
    },

    /* =======================
       DEFAULT & FALLBACK
       ======================= */
    {
        path: '',
        redirectTo: 'login',
        pathMatch: 'full'
    },
    {
        path: '**',
        redirectTo: 'login'
    }
];
