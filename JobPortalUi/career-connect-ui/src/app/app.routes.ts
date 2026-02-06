import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { jobProviderGuard } from './core/guards/job-provider.guard';
import { jobSeekerGuard } from './core/guards/job-seeker.guard';

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
