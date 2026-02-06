import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { JobProviderService } from '../../../core/services/job-provider.service';
import { Router, ActivatedRoute  } from '@angular/router';
import { CreateJobComponent } from '../create-job/create-job.component';
import { EditJobComponent } from '../edit-job/edit-job.component';

@Component({
  standalone: true,
  imports: [
    CommonModule,
    CreateJobComponent,
    EditJobComponent,
    FormsModule
  ],
  templateUrl: './jobs-list.component.html',
  styleUrls: ['./jobs-list.component.scss']
})
export class JobProviderJobsListComponent implements OnInit {

  jobs: any[] = [];
  filteredJobs: any[] = [];
  searchTerm = '';
  statusFilter = 'all';
  constructor(private jobService: JobProviderService, 
    private router: Router,
  private route: ActivatedRoute) 
    { }

 ngOnInit(): void {
    // Initial load
    this.loadJobs();

    // Reload after create job redirect
    this.route.queryParams.subscribe(params => {
      if (params['refresh']) {
        this.loadJobs();
      }
    });
  }

   loadJobs(): void {
    this.jobService.getMyJobs().subscribe(res => {
      this.jobs = res as any[] ?? [];
      this.applyFilters();
    });
  }

  viewApplications(job: any): void {
    const jobId = this.resolveJobId(job);
    if (!jobId) return;
    this.router.navigate([
      '/job-provider/jobs',
      jobId,
      'applications'
    ]);
  }

showEditJob = false;
selectedJobId: string | null = null;
selectedJob: any | null = null;
  editJob(job: any): void {
  const jobId = this.resolveJobId(job);
  if (!jobId) return;
  this.selectedJobId = jobId;
  this.selectedJob = job;
  this.showEditJob = true;
  }

  applyFilters(): void {
    const term = this.searchTerm.trim().toLowerCase();
    const status = this.statusFilter;

    this.filteredJobs = this.jobs.filter(job => {
      const title = (job.title ?? '').toLowerCase();
      const city = (job.city ?? '').toLowerCase();
      const country = (job.country ?? '').toLowerCase();
      const matchesTerm = !term || title.includes(term) || city.includes(term) || country.includes(term);
      const matchesStatus = status === 'all' || job.status === status;
      return matchesTerm && matchesStatus;
    });
  }

  get totalJobs(): number {
    return this.jobs.length;
  }

  get activeJobs(): number {
    return this.jobs.filter(job => job.status === 'Active').length;
  }

  get closedJobs(): number {
    return this.jobs.filter(job => job.status === 'Closed').length;
  }

  showCreateJob = false;
  

  openCreateJob(): void {
    this.showCreateJob = true;
  }

  closeCreateJob(): void {
    this.showCreateJob = false;
  }

closeEditJob() {
  this.showEditJob = false;
  this.selectedJob = null;
  this.loadJobs();
}

private resolveJobId(job: any): string | null {
  if (!job) return null;
  return job.id ?? job.jobId ?? job._id ?? null;
}

}
