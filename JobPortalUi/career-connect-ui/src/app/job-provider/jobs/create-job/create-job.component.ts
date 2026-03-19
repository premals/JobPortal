import { Component, EventEmitter, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  FormGroup,
  Validators,
  ReactiveFormsModule
} from '@angular/forms';
import { Router } from '@angular/router';

import { CreateJobRequest } from '../../../core/models/job-provider/create-job-request.model';
import { JobProviderService } from '../../../core/services/job-provider.service';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-create-job',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './create-job.component.html',
  styleUrls: ['./create-job.component.scss']
})
export class CreateJobComponent {

  @Output() closed = new EventEmitter<void>();

  jobForm: FormGroup;
  step = 1;
  private readonly stepFields: Record<number, string[]> = {
    1: ['title', 'description', 'employmentType', 'workMode'],
    2: [
      'minExperience',
      'maxExperience',
      'city',
      'state',
      'country',
      'keySkills',
      'education',
      'industry'
    ],
    3: [
      'minSalary',
      'maxSalary',
      'currency',
      'salaryFrequency',
      'openings',
      'expiryDate'
    ]
  };

  constructor(
    private fb: FormBuilder,
    private jobService: JobProviderService,
    private router: Router,
    private toastService: ToastService
  ) {
    this.jobForm = this.fb.group({

      /* ======================
         STEP 1 – BASIC INFO
      ====================== */
      title: ['', Validators.required],
      description: ['', [Validators.required, Validators.minLength(20)]],
      employmentType: ['PartTime', Validators.required],
      workMode: ['Onsite', Validators.required],

      /* ======================
         STEP 2 – EXPERIENCE & LOCATION
      ====================== */
      minExperience: [0, [Validators.required, Validators.min(0)]],
      maxExperience: [0, [Validators.required, Validators.min(0)]],

      city: ['', Validators.required],
      state: ['', Validators.required],
      country: ['', Validators.required],

      keySkills: ['', Validators.required],
      education: ['', Validators.required],
      industry: ['', Validators.required],

      /* ======================
         STEP 3 – SALARY & META
      ====================== */
      minSalary: [0, Validators.min(0)],
      maxSalary: [0, Validators.min(0)],
      currency: ['INR', Validators.required],
      salaryFrequency: ['Monthly', Validators.required],

      openings: [1, [Validators.required, Validators.min(1)]],
      expiryDate: ['', Validators.required]
    });
  }

  nextStep(): void {
    if (this.step >= 3) {
      return;
    }

    if (!this.isStepValid(this.step)) {
      this.markStepTouched(this.step);
      return;
    }

    this.step++;
  }

  prevStep(): void {
    if (this.step > 1) {
      this.step--;
    }
  }

  close(): void {
    this.closed.emit();
  }

  submit(): void {
    if (this.jobForm.invalid) {
      this.jobForm.markAllAsTouched();
      return;
    }

    const v = this.jobForm.value;

    const payload: CreateJobRequest = {
      title: v.title!,
      description: v.description!,
      employmentType: v.employmentType!,
      workMode: v.workMode!,

      minExperience: Number(v.minExperience),
      maxExperience: Number(v.maxExperience),

      city: v.city!,
      state: v.state!,
      country: v.country!,

      minSalary: Number(v.minSalary),
      maxSalary: Number(v.maxSalary),
      currency: v.currency!,
      salaryFrequency: v.salaryFrequency!,

      keySkills: v.keySkills
        .split(',')
        .map((s: string) => s.trim()),

      education: v.education!,
      industry: v.industry!,

      openings: Number(v.openings),
      expiryDate: new Date(v.expiryDate!).toISOString()
    };

    this.jobService.createJob(payload).subscribe({
      next: () => {
        // ✅ Show toast
        this.toastService.show('Job successfully created');

        // ✅ Close modal
        this.close();

        // ✅ Redirect to job list
        this.router.navigate(['/job-provider/jobs'], {
          queryParams: { refresh: true }
        });
      },
      error: () => {
        this.toastService.show('Failed to create job');
      }
    });
  }

  isFieldInvalid(controlName: string): boolean {
    const control = this.jobForm.get(controlName);
    return !!control && control.invalid && (control.dirty || control.touched);
  }

  getFieldError(controlName: string, label: string): string | null {
    const control = this.jobForm.get(controlName);
    if (!control || !this.isFieldInvalid(controlName)) {
      return null;
    }

    if (control.hasError('required')) {
      return `${label} is required`;
    }

    if (control.hasError('minlength')) {
      const requiredLength = control.getError('minlength')?.requiredLength ?? 0;
      return `${label} must be at least ${requiredLength} characters`;
    }

    if (control.hasError('min')) {
      const minValue = control.getError('min')?.min ?? 0;
      return `${label} must be at least ${minValue}`;
    }

    return 'Invalid value';
  }

  private isStepValid(step: number): boolean {
    return this.stepFields[step].every((field) => {
      const control = this.jobForm.get(field);
      return !!control && control.valid;
    });
  }

  private markStepTouched(step: number): void {
    this.stepFields[step].forEach((field) => {
      this.jobForm.get(field)?.markAsTouched();
    });
  }
}
