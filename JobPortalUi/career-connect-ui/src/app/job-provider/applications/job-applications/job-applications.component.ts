import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { JobProviderService } from '../../../core/services/job-provider.service';

@Component({
  standalone: true,
  imports: [CommonModule],
  templateUrl: './job-applications.component.html',
  styleUrls: ['./job-applications.component.scss']
})
export class JobApplicationsComponent implements OnInit {

  applications: any[] = [];
  isLoading = true;

  constructor(
    private route: ActivatedRoute,
    private service: JobProviderService
  ) {}

  ngOnInit(): void {
    const jobId = this.route.snapshot.paramMap.get('jobId')!;
    this.service.getApplicationsByJob(jobId).subscribe(res => {
      this.applications = res;
      this.isLoading = false;
    });
  }

  getStatusClass(status: string): string {
    switch (status) {
      case 'Applied': return 'badge-applied';
      case 'Shortlisted': return 'badge-shortlisted';
      case 'Rejected': return 'badge-rejected';
      default: return 'badge-default';
    }
  }
}