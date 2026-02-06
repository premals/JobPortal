import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { JobSeekerService } from '../../../core/services/job-seeker.service';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-my-applications',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './my-applications.component.html',
  styleUrl: './my-applications.component.scss'
})
export class MyApplicationsComponent implements OnInit {
  isLoading = true;
  applications: any[] = [];
  filteredApplications: any[] = [];
  searchTerm = '';
  statusFilter = 'all';
  withdrawingIds = new Set<string>();

  constructor(
    private jobSeekerService: JobSeekerService,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.loadApplications();
  }

  loadApplications(): void {
    this.isLoading = true;
    this.jobSeekerService.getMyApplications().subscribe({
      next: (apps: any) => {
        this.applications = Array.isArray(apps) ? apps : [];
        this.applyFilters();
      },
      error: () => {
        this.applications = [];
        this.toastService.show('Unable to load applications.');
        this.isLoading = false;
      },
      complete: () => {
        this.isLoading = false;
      }
    });
  }

  applyFilters(): void {
    const term = this.searchTerm.trim().toLowerCase();
    const status = this.statusFilter;
    this.filteredApplications = this.applications.filter(app => {
      const title = (app.jobTitle ?? '').toLowerCase();
      const company = (app.company ?? app.companyName ?? '').toLowerCase();
      const matchesTerm = !term || title.includes(term) || company.includes(term);
      const matchesStatus = status === 'all' || String(app.status ?? '').toLowerCase() === status;
      return matchesTerm && matchesStatus;
    });
  }

  getStatusClass(status: string): string {
    switch (String(status || '').toLowerCase()) {
      case 'shortlisted': return 'shortlisted';
      case 'rejected': return 'rejected';
      case 'accepted': return 'accepted';
      case 'pending': return 'pending';
      default: return 'applied';
    }
  }

  isWithdrawing(app: any): boolean {
    return this.withdrawingIds.has(app.jobId ?? app.id ?? app.applicationId);
  }

  withdraw(app: any): void {
    const jobId = app.jobId ?? app.id ?? app.applicationId;
    if (!jobId || this.isWithdrawing(app)) return;

    this.withdrawingIds.add(jobId);
    this.jobSeekerService.withdrawApplication(jobId).subscribe({
      next: () => {
        this.toastService.show('Application withdrawn.');
        this.applications = this.applications.filter(item => (item.jobId ?? item.id ?? item.applicationId) !== jobId);
        this.applyFilters();
      },
      error: () => {
        this.toastService.show('Unable to withdraw application.');
      },
      complete: () => {
        this.withdrawingIds.delete(jobId);
      }
    });
  }
}
