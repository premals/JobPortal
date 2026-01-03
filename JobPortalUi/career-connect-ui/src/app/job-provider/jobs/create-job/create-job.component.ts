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

@Component({
  selector: 'app-create-job',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './create-job.component.html',
  styleUrls: ['./create-job.component.scss']
})
export class CreateJobComponent {

  jobs: any[] = [];
  jobForm!: FormGroup; // ✅ declare only

  @Output() closed = new EventEmitter<void>();

  step = 1;

  constructor(private fb: FormBuilder,private jobService: JobProviderService, private router: Router) {
     this.jobForm = this.fb.group({
          title: ['', Validators.required],
          description: ['', [Validators.required, Validators.minLength(20)]],
    
          employmentType: ['PartTime', Validators.required],
          workMode: ['Onsite', Validators.required],
    
          minExperience: [0, Validators.required],
          maxExperience: [0, Validators.required],
    
          city: ['', Validators.required],
          state: ['', Validators.required],
          country: ['', Validators.required],
    
          minSalary: [0],
          maxSalary: [0],
          currency: ['INR', Validators.required],
          salaryFrequency: ['Monthly', Validators.required],
    
          keySkills: ['', Validators.required],
          education: ['', Validators.required],
          industry: ['', Validators.required],
    
          openings: [1, Validators.required],
          expiryDate: ['', Validators.required]
        });
  }

  nextStep() {
    if (this.step < 3) this.step++;
  }

  prevStep() {
    if (this.step > 1) this.step--;
  }

  close() {
    this.closed.emit(); 
  }

  submit(): void {
    if (this.jobForm.invalid) {
      this.jobForm.markAllAsTouched();
      return;
    }

    const formValue = this.jobForm.value;

    const payload: CreateJobRequest = {
      title: formValue.title!,
      description: formValue.description!,
      employmentType: formValue.employmentType!,
      workMode: formValue.workMode!,

      minExperience: Number(formValue.minExperience),
      maxExperience: Number(formValue.maxExperience),

      city: formValue.city!,
      state: formValue.state!,
      country: formValue.country!,

      minSalary: Number(formValue.minSalary),
      maxSalary: Number(formValue.maxSalary),
      currency: formValue.currency!,
      salaryFrequency: formValue.salaryFrequency!,

      keySkills: formValue.keySkills!
        .split(',')
        .map((s: string) => s.trim()),

      education: formValue.education!,
      industry: formValue.industry!,

      openings: Number(formValue.openings),
      expiryDate: new Date(formValue.expiryDate!).toISOString()
    };

    this.jobService.createJob(payload).subscribe(() => {
      this.router.navigate(['/job-provider/jobs']);
    });

      this.close();
  }
}