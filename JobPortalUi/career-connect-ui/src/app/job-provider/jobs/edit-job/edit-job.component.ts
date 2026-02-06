import { Component, EventEmitter, Input, Output, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  FormGroup,
  Validators,
  ReactiveFormsModule
} from '@angular/forms';
import { Router } from '@angular/router';

import { JobProviderService } from '../../../core/services/job-provider.service';
import { ToastService } from '../../../core/services/toast.service';
import { CreateJobRequest } from '../../../core/models/job-provider/create-job-request.model';

@Component({
  selector: 'app-edit-job',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './edit-job.component.html',
  styleUrls: ['./edit-job.component.scss']
})
export class EditJobComponent implements OnInit {

  @Input() jobId: string | null = null;
  @Input() job?: any;
  @Output() closed = new EventEmitter<void>();

  jobForm!: FormGroup;
  step = 1;

  constructor(
    private fb: FormBuilder,
    private jobService: JobProviderService,
    private router: Router,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.initForm();
    if (this.job) {
      this.patchFromJob(this.job);
    } else {
      this.loadJob();
    }
  }

  private initForm(): void {
    this.jobForm = this.fb.group({

      /* STEP 1 – BASIC INFO */
      title: ['', Validators.required],
      description: ['', [Validators.required, Validators.minLength(20)]],
      employmentType: ['PartTime', Validators.required],
      workMode: ['Onsite', Validators.required],

      /* STEP 2 – EXPERIENCE & LOCATION */
      minExperience: [0, [Validators.required, Validators.min(0)]],
      maxExperience: [0, [Validators.required, Validators.min(0)]],

      city: ['', Validators.required],
      state: ['', Validators.required],
      country: ['', Validators.required],

      keySkills: ['', Validators.required],
      education: ['', Validators.required],
      industry: ['', Validators.required],

      /* STEP 3 – SALARY & META */
      minSalary: [0, Validators.min(0)],
      maxSalary: [0, Validators.min(0)],
      currency: ['INR', Validators.required],
      salaryFrequency: ['Monthly', Validators.required],

      openings: [1, [Validators.required, Validators.min(1)]],
      expiryDate: ['', Validators.required]
    });
  }

  private loadJob(): void {
    if (!this.jobId) return;
    this.jobService.getJobById(this.jobId).subscribe(job => {
      this.patchFromJob(job);
    });
  }

  private patchFromJob(job: any): void {
    const keySkills = Array.isArray(job?.keySkills)
      ? job.keySkills.join(', ')
      : (job?.keySkills ?? '');

    const expiryDate = job?.expiryDate
      ? job.expiryDate.split('T')[0]
      : '';

    this.jobForm.patchValue({
      ...job,
      keySkills,
      expiryDate
    });
  }

  nextStep(): void {
    if (this.step < 3) this.step++;
  }

  prevStep(): void {
    if (this.step > 1) this.step--;
  }

  close(): void {
    this.closed.emit();
  }

  submit(): void {
    if (this.jobForm.invalid) {
      this.jobForm.markAllAsTouched();
      return;
    }
    if (!this.jobId) {
      this.toastService.show('Job ID missing');
      return;
    }

    const v = this.jobForm.value;

    const payload: CreateJobRequest = {
      title: v.title,
      description: v.description,
      employmentType: v.employmentType,
      workMode: v.workMode,

      minExperience: Number(v.minExperience),
      maxExperience: Number(v.maxExperience),

      city: v.city,
      state: v.state,
      country: v.country,

      minSalary: Number(v.minSalary),
      maxSalary: Number(v.maxSalary),
      currency: v.currency,
      salaryFrequency: v.salaryFrequency,

      keySkills: v.keySkills.split(',').map((s: string) => s.trim()),
      education: v.education,
      industry: v.industry,

      openings: Number(v.openings),
      expiryDate: new Date(v.expiryDate).toISOString()
    };

    this.jobService.updateJob(this.jobId, payload).subscribe({
      next: () => {
        this.toastService.show('Job updated successfully');
        this.close();
        this.router.navigate(['/job-provider/jobs'], {
          queryParams: { refresh: true }
        });
      },
      error: () => {
        this.toastService.show('Failed to update job');
      }
    });
  }
}
