import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { AdminService } from '../../core/services/admin.service';
import { AdminModerationJob, addAuditLog } from '../admin-data';
import { mapJobProjection } from '../admin-api-mappers';

@Component({
  standalone: true,
  selector: 'app-jobs-moderation',
  imports: [CommonModule, FormsModule],
  templateUrl: './jobs-moderation.component.html',
  styleUrls: ['./jobs-moderation.component.scss']
})
export class JobsModerationComponent implements OnInit {
  jobs: AdminModerationJob[] = [];
  filtered: AdminModerationJob[] = [];

  statusFilter = 'all';
  searchTerm = '';

  constructor(private adminService: AdminService) {}

  ngOnInit(): void {
    forkJoin({
      jobs: this.adminService.getAdminJobs().pipe(catchError(() => of([]))),
      providers: this.adminService.getAdminJobProviders().pipe(catchError(() => of([])))
    }).subscribe(({ jobs, providers }) => {
      const providerLookup = new Map(providers.map(provider => [provider.JobProviderId, provider]));
      this.jobs = jobs.map(job => mapJobProjection(job, providerLookup));
      this.applyFilters();
    });
  }

  applyFilters(): void {
    const search = this.searchTerm.trim().toLowerCase();
    this.filtered = this.jobs.filter(job => {
      const matchesSearch =
        !search ||
        job.title.toLowerCase().includes(search) ||
        job.provider.toLowerCase().includes(search);
      const matchesStatus =
        this.statusFilter === 'all' ||
        job.status.toLowerCase() === this.statusFilter;
      return matchesSearch && matchesStatus;
    });
  }

  toggleHide(job: AdminModerationJob): void {
    const oldStatus = job.status;
    job.hidden = !job.hidden;
    job.status = job.hidden ? 'Hidden' : 'Active';
    addAuditLog({
      adminUserId: 'admin-01',
      action: job.hidden ? 'Hide Job' : 'Unhide Job',
      entityType: 'Job',
      entityId: job.id,
      oldValue: oldStatus,
      newValue: job.status,
      reason: 'Moderation action'
    });
    this.applyFilters();
  }

  markScam(job: AdminModerationJob): void {
    const oldStatus = job.status;
    job.flaggedAsScam = true;
    job.status = 'Scam';
    addAuditLog({
      adminUserId: 'admin-01',
      action: 'Mark Job Scam',
      entityType: 'Job',
      entityId: job.id,
      oldValue: oldStatus,
      newValue: job.status,
      reason: 'Confirmed reports'
    });
    this.applyFilters();
  }

  requestChanges(job: AdminModerationJob): void {
    const oldStatus = job.status;
    job.status = 'ChangesRequested';
    addAuditLog({
      adminUserId: 'admin-01',
      action: 'Request Job Changes',
      entityType: 'Job',
      entityId: job.id,
      oldValue: oldStatus,
      newValue: job.status,
      reason: 'Content revision requested'
    });
    this.applyFilters();
  }
}
