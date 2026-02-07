import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { AdminService } from '../../../core/services/admin.service';
import { AdminHiringApplication } from '../../admin-data';
import { mapApplicationProjection } from '../../admin-api-mappers';

@Component({
  standalone: true,
  selector: 'app-hiring-list',
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './hiring-list.component.html',
  styleUrls: ['./hiring-list.component.scss']
})
export class HiringListComponent implements OnInit {
  applications: AdminHiringApplication[] = [];
  filtered: AdminHiringApplication[] = [];
  paged: AdminHiringApplication[] = [];

  jobTitleFilter = '';
  providerFilter = '';
  stageFilter = 'all';
  recruiterFilter = '';
  dateFrom = '';
  dateTo = '';
  page = 1;
  pageSize = 6;
  totalPages = 1;

  constructor(private adminService: AdminService) {}

  ngOnInit(): void {
    forkJoin({
      applications: this.adminService.getAdminApplications().pipe(catchError(() => of([]))),
      jobs: this.adminService.getAdminJobs().pipe(catchError(() => of([]))),
      providers: this.adminService.getAdminJobProviders().pipe(catchError(() => of([])))
    }).subscribe(({ applications, jobs, providers }) => {
      const jobLookup = new Map(jobs.map(item => [item.JobId, item]));
      const providerLookup = new Map(providers.map(item => [item.JobProviderId, item]));
      this.applications = applications.map(app => mapApplicationProjection(app, jobLookup, providerLookup));
      this.applyFilters();
    });
  }

  applyFilters(): void {
    const title = this.jobTitleFilter.trim().toLowerCase();
    const provider = this.providerFilter.trim().toLowerCase();
    const recruiter = this.recruiterFilter.trim().toLowerCase();
    const from = this.dateFrom ? new Date(this.dateFrom) : null;
    const to = this.dateTo ? new Date(this.dateTo) : null;

    let data = this.applications.filter(app => {
      const matchesTitle = !title || app.jobTitle.toLowerCase().includes(title);
      const matchesProvider = !provider || app.jobProvider.toLowerCase().includes(provider);
      const matchesStage = this.stageFilter === 'all' || app.stage === this.stageFilter;
      const matchesRecruiter = !recruiter || app.recruiter.toLowerCase().includes(recruiter);
      const applied = new Date(app.appliedAt);
      const matchesDate = (!from || applied >= from) && (!to || applied <= to);
      return matchesTitle && matchesProvider && matchesStage && matchesRecruiter && matchesDate;
    });

    data = data.sort((a, b) => new Date(b.lastUpdated).getTime() - new Date(a.lastUpdated).getTime());

    this.filtered = data;
    this.page = 1;
    this.updatePage();
  }

  updatePage(): void {
    this.totalPages = Math.max(Math.ceil(this.filtered.length / this.pageSize), 1);
    const start = (this.page - 1) * this.pageSize;
    this.paged = this.filtered.slice(start, start + this.pageSize);
  }

  changePage(delta: number): void {
    const next = this.page + delta;
    if (next < 1 || next > this.totalPages) return;
    this.page = next;
    this.updatePage();
  }
}
