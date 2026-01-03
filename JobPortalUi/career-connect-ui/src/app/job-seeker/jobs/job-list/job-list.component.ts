import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { JobSeekerService } from '../../../core/services/job-seeker.service';

@Component({
  standalone: true,
  imports: [CommonModule],
  templateUrl: './job-list.component.html'
})
export class JobListComponent implements OnInit {

  jobs: any[] = [];

  constructor(
    private service: JobSeekerService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.service.getJobs().subscribe(res => {
      this.jobs = res as any[];
    });
  }

  view(jobId: string): void {
    this.router.navigate(['/job-seeker/jobs', jobId]);
  }
}