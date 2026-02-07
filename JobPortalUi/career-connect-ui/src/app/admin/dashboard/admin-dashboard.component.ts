import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { AdminService } from '../../core/services/admin.service';
import { AdminSummary } from '../../core/models/admin/admin-summary.model';
import { AdminHiringApplication, AdminJobProvider, AdminJobSeeker, AdminModerationJob } from '../admin-data';
import {
  mapApplicationProjection,
  mapJobProjection,
  mapJobProviderProjection,
  mapJobSeekerProjection
} from '../admin-api-mappers';

@Component({
  standalone: true,
  selector: 'app-admin-dashboard',
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-dashboard.component.html',
  styleUrls: ['./admin-dashboard.component.scss']
})
export class AdminDashboardComponent implements OnInit {
  summary: AdminSummary | null = null;
  jobSeekers: AdminJobSeeker[] = [];
  jobProviders: AdminJobProvider[] = [];
  jobs: AdminModerationJob[] = [];
  applications: AdminHiringApplication[] = [];

  filters = {
    from: '',
    to: '',
    status: 'all',
    city: 'all'
  };

  constructor(private adminService: AdminService) {}

  ngOnInit(): void {
    forkJoin({
      summary: this.adminService.getAdminSummary().pipe(catchError(() => of(null))),
      jobSeekers: this.adminService.getAdminJobSeekers().pipe(catchError(() => of([]))),
      jobProviders: this.adminService.getAdminJobProviders().pipe(catchError(() => of([]))),
      jobs: this.adminService.getAdminJobs().pipe(catchError(() => of([]))),
      applications: this.adminService.getAdminApplications().pipe(catchError(() => of([])))
    }).subscribe(({ summary, jobSeekers, jobProviders, jobs, applications }) => {
      this.summary = summary;
      this.jobSeekers = jobSeekers.map(mapJobSeekerProjection);
      this.jobProviders = jobProviders.map(mapJobProviderProjection);
      const providerLookup = new Map(jobProviders.map(item => [item.JobProviderId, item]));
      const jobLookup = new Map(jobs.map(item => [item.JobId, item]));
      this.jobs = jobs.map(job => mapJobProjection(job, providerLookup));
      this.applications = applications.map(app => mapApplicationProjection(app, jobLookup, providerLookup));
    });
  }

  get totalJobSeekers(): number {
    return this.summary?.totalJobSeekers ?? this.jobSeekers.length;
  }

  get totalJobProviders(): number {
    return this.summary?.totalJobProviders ?? this.jobProviders.length;
  }

  get totalJobsPosted(): number {
    return this.summary?.totalJobs ?? this.jobs.length;
  }

  get totalApplications(): number {
    return this.summary?.totalApplications ?? this.applications.length;
  }

  get totalHires(): number {
    return this.summary?.totalHires ?? this.applications.filter(app => app.stage === 'Hired').length;
  }

  get activeUsers(): number {
    if (this.summary) return this.summary.activeUsers;
    const seekerActive = this.jobSeekers.filter(user => user.status === 'Active').length;
    const providerActive = this.jobProviders.filter(user => user.status === 'Active').length;
    return seekerActive + providerActive;
  }

  get blockedUsers(): number {
    if (this.summary) return this.summary.blockedUsers;
    const seekerBlocked = this.jobSeekers.filter(user => user.status === 'Blocked').length;
    const providerBlocked = this.jobProviders.filter(user => user.status === 'Blocked').length;
    return seekerBlocked + providerBlocked;
  }

  get availableCities(): string[] {
    const cities = this.applications.map(app => app.city).filter(Boolean);
    return Array.from(new Set(cities)).sort();
  }

  get filteredApplications() {
    const from = this.filters.from ? new Date(this.filters.from) : null;
    const to = this.filters.to ? new Date(this.filters.to) : null;
    return this.applications.filter(app => {
      const applied = new Date(app.appliedAt);
      const inFrom = !from || applied >= from;
      const inTo = !to || applied <= to;
      const inStatus = this.filters.status === 'all' || app.stage === this.filters.status;
      const inCity = this.filters.city === 'all' || app.city === this.filters.city;
      return inFrom && inTo && inStatus && inCity;
    });
  }

  get applicationsByStatus() {
    const stages = ['Applied', 'Screening', 'Interview', 'Offer', 'Hired', 'Rejected', 'Withdrawn'];
    return stages.map(stage => ({
      label: stage,
      value: this.filteredApplications.filter(app => app.stage === stage).length
    }));
  }

  get statusMax(): number {
    return Math.max(...this.applicationsByStatus.map(item => item.value), 1);
  }

  get hiresByMonth() {
    const now = new Date();
    const months: { label: string; key: string; value: number }[] = [];
    for (let i = 5; i >= 0; i--) {
      const date = new Date(now.getFullYear(), now.getMonth() - i, 1);
      const key = `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}`;
      const label = date.toLocaleString('en-US', { month: 'short' });
      months.push({ label, key, value: 0 });
    }

    this.applications
      .filter(app => app.stage === 'Hired')
      .forEach(app => {
        const date = new Date(app.lastUpdated);
        const key = `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}`;
        const match = months.find(month => month.key === key);
        if (match) {
          match.value += 1;
        }
      });

    return months;
  }

  get hiresMax(): number {
    return Math.max(...this.hiresByMonth.map(item => item.value), 1);
  }

  get topCities() {
    const counts = new Map<string, number>();
    this.jobSeekers.forEach(seeker => {
      counts.set(seeker.city, (counts.get(seeker.city) ?? 0) + 1);
    });
    return Array.from(counts.entries())
      .map(([label, value]) => ({ label, value }))
      .sort((a, b) => b.value - a.value)
      .slice(0, 5);
  }

  get topSkills() {
    const counts = new Map<string, number>();
    this.jobSeekers
      .flatMap(seeker => seeker.skills ?? [])
      .forEach(skill => {
        counts.set(skill, (counts.get(skill) ?? 0) + 1);
      });
    return Array.from(counts.entries())
      .map(([label, value]) => ({ label, value }))
      .sort((a, b) => b.value - a.value)
      .slice(0, 6);
  }

  percent(value: number, max: number): number {
    if (max === 0) return 0;
    return Math.round((value / max) * 100);
  }
}
