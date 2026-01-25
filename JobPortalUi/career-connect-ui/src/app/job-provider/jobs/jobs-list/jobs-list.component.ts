import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { JobProviderService } from '../../../core/services/job-provider.service';
import { Router, ActivatedRoute  } from '@angular/router';
import { CreateJobComponent } from '../create-job/create-job.component';

@Component({
  standalone: true,
  imports: [
    CommonModule,
    CreateJobComponent],
  templateUrl: './jobs-list.component.html',
  styleUrls: ['./jobs-list.component.scss']
})
export class JobListComponent implements OnInit {

  jobs: any[] = [];
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
      this.jobs = res as any[];
    });
  }

  viewApplications(jobId: string): void {
    this.router.navigate([
      '/job-provider/jobs',
      jobId,
      'applications'
    ]);
  }

  editJob(jobId: string): void {
    this.router.navigate(['/job-provider/jobs/edit', jobId]);
  }

  showCreateJob = false;

  openCreateJob(): void {
    this.showCreateJob = true;
  }

  closeCreateJob(): void {
    this.showCreateJob = false;
  }

}
