import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { forkJoin, of } from 'rxjs';
import { catchError, finalize } from 'rxjs/operators';
import { JobSeekerProfileService } from '../../../core/services/job-seeker-profile.service';
import { JobSeekerService } from '../../../core/services/job-seeker.service';

@Component({
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './job-list.component.html'
})
export class JobListComponent implements OnInit {

  jobs: any[] = [];
  listTitle = 'Recommended Jobs';
  searchTerm = '';
  loading = false;
  appliedJobIds = new Set<string>();
  applyingJobIds = new Set<string>();

  constructor(
    private service: JobSeekerService,
    private profileService: JobSeekerProfileService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadRecommendedJobs();
  }

  view(job: any): void {
    const jobId = this.getJobId(job);
    if (!jobId) return;
    this.router.navigate(['/job-seeker/jobs', jobId]);
  }

  apply(job: any): void {
    const jobId = this.getJobId(job);
    if (!jobId || this.appliedJobIds.has(jobId) || this.applyingJobIds.has(jobId))
      return;

    this.applyingJobIds.add(jobId);
    this.service.applyJob(jobId)
      .pipe(finalize(() => this.applyingJobIds.delete(jobId)))
      .subscribe({
        next: () => this.appliedJobIds.add(jobId)
      });
  }

  onSearch(): void {
    const term = this.searchTerm.trim();
    if (!term) {
      this.loadRecommendedJobs();
      return;
    }

    this.searchJobs(term, 'Search Results', false);
  }

  clearSearch(): void {
    this.searchTerm = '';
    this.loadRecommendedJobs();
  }

  isApplied(job: any): boolean {
    const jobId = this.getJobId(job);
    return !!jobId && this.appliedJobIds.has(jobId);
  }

  isApplying(job: any): boolean {
    const jobId = this.getJobId(job);
    return !!jobId && this.applyingJobIds.has(jobId);
  }

  private loadRecommendedJobs(): void {
    this.loading = true;
    this.profileService.getProfile()
      .pipe(
        catchError(() => of(null))
      )
      .subscribe(profile => {
        const skills = (profile?.skills ?? [])
          .map((skill: string) => skill?.trim())
          .filter((skill: string) => !!skill);
        const field = (profile?.headline ?? '').trim()
          || (profile?.education ?? '').trim();

        if (skills.length > 0) {
          this.listTitle = 'Jobs matching your skills';
          this.fetchJobsBySkills(skills.slice(0, 3));
          return;
        }

        if (field) {
          this.searchJobs(field, 'Jobs related to your field', true);
          return;
        }

        this.listTitle = 'Latest Jobs';
        this.fetchLatestJobs();
      });
  }

  private fetchLatestJobs(): void {
    this.loading = true;
    this.service.getJobs()
      .pipe(finalize(() => this.loading = false))
      .subscribe({
        next: res => this.jobs = (res as any[]) ?? [],
        error: () => this.jobs = []
      });
  }

  private fetchJobsBySkills(skills: string[]): void {
    if (skills.length === 0) {
      this.fetchLatestJobs();
      return;
    }

    this.loading = true;
    const requests = skills.map(skill =>
      this.service.getJobsBySkill(skill).pipe(catchError(() => of([])))
    );

    forkJoin(requests)
      .pipe(finalize(() => this.loading = false))
      .subscribe({
        next: results => {
          const merged: Record<string, any> = {};
          results.forEach(list => {
            (list as any[]).forEach(job => {
              const jobId = this.getJobId(job);
              if (jobId && !merged[jobId]) {
                merged[jobId] = job;
              }
            });
          });

          const mergedJobs = Object.values(merged);
          if (mergedJobs.length === 0) {
            this.listTitle = 'Latest Jobs';
            this.fetchLatestJobs();
            return;
          }

          this.jobs = mergedJobs;
        },
        error: () => this.fetchLatestJobs()
      });
  }

  private searchJobs(term: string, title: string, fallbackToLatest: boolean): void {
    this.loading = true;
    this.listTitle = title;
    this.service.searchJobs({ keyword: term })
      .pipe(finalize(() => this.loading = false))
      .subscribe({
        next: res => {
          const jobs = (res as any[]) ?? [];
          if (jobs.length === 0 && fallbackToLatest) {
            this.listTitle = 'Latest Jobs';
            this.fetchLatestJobs();
            return;
          }
          this.jobs = jobs;
        },
        error: () => {
          if (fallbackToLatest) {
            this.listTitle = 'Latest Jobs';
            this.fetchLatestJobs();
          } else {
            this.jobs = [];
          }
        }
      });
  }

  private getJobId(job: any): string | null {
    return job?.jobId ?? job?.id ?? null;
  }
}
