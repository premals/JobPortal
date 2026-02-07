import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { AdminService } from '../../../core/services/admin.service';
import { AdminHiringApplication, addAuditLog } from '../../admin-data';
import { mapApplicationProjection } from '../../admin-api-mappers';

@Component({
  standalone: true,
  selector: 'app-hiring-detail',
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './hiring-detail.component.html',
  styleUrls: ['./hiring-detail.component.scss']
})
export class HiringDetailComponent implements OnInit {
  application?: AdminHiringApplication;
  moveStageValue = '';
  moveReason = '';
  overrideStageValue = '';
  overrideReason = '';
  newRecruiter = '';
  noteText = '';

  stages = ['Applied', 'Screening', 'Interview', 'Offer', 'Hired', 'Rejected', 'Withdrawn'];
  finalStages = ['Hired', 'Rejected', 'Withdrawn'];

  constructor(private route: ActivatedRoute, private adminService: AdminService) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) return;

    this.adminService
      .getAdminApplicationById(id)
      .pipe(catchError(() => of(null)))
      .subscribe(application => {
        if (!application) return;

        const job$ = application.JobId
          ? this.adminService.getAdminJobById(application.JobId).pipe(catchError(() => of(null)))
          : of(null);
        const provider$ = application.JobProviderId
          ? this.adminService.getAdminJobProviderById(application.JobProviderId).pipe(catchError(() => of(null)))
          : of(null);

        forkJoin({ job: job$, provider: provider$ }).subscribe(({ job, provider }) => {
          const jobLookup = new Map<string, any>();
          const providerLookup = new Map<string, any>();
          if (job) jobLookup.set(job.JobId, job);
          if (provider) providerLookup.set(provider.JobProviderId, provider);
          this.application = mapApplicationProjection(application, jobLookup, providerLookup);
        });
      });
  }

  applyStageMove(): void {
    if (!this.application || !this.moveStageValue) return;
    const oldStage = this.application.stage;
    this.application.stage = this.moveStageValue;
    this.application.timeline.push({
      stage: this.moveStageValue,
      date: new Date().toISOString(),
      note: this.moveReason || 'Stage updated'
    });
    this.application.lastUpdated = new Date().toISOString();
    addAuditLog({
      adminUserId: 'admin-01',
      action: 'Move Stage',
      entityType: 'Application',
      entityId: this.application.id,
      oldValue: oldStage,
      newValue: this.moveStageValue,
      reason: this.moveReason || 'Stage updated'
    });
    this.moveStageValue = '';
    this.moveReason = '';
  }

  applyOverride(): void {
    if (!this.application || !this.overrideStageValue) return;
    const oldStage = this.application.stage;
    this.application.stage = this.overrideStageValue;
    this.application.decisionReason = this.overrideReason || 'Override applied';
    this.application.timeline.push({
      stage: this.overrideStageValue,
      date: new Date().toISOString(),
      note: this.overrideReason || 'Override applied'
    });
    this.application.lastUpdated = new Date().toISOString();
    addAuditLog({
      adminUserId: 'admin-01',
      action: 'Override Status',
      entityType: 'Application',
      entityId: this.application.id,
      oldValue: oldStage,
      newValue: this.overrideStageValue,
      reason: this.overrideReason || 'Override applied'
    });
    this.overrideStageValue = '';
    this.overrideReason = '';
  }

  assignRecruiter(): void {
    if (!this.application || !this.newRecruiter.trim()) return;
    const oldRecruiter = this.application.recruiter;
    this.application.recruiter = this.newRecruiter.trim();
    addAuditLog({
      adminUserId: 'admin-01',
      action: 'Assign Recruiter',
      entityType: 'Application',
      entityId: this.application.id,
      oldValue: oldRecruiter,
      newValue: this.application.recruiter,
      reason: 'Admin assignment'
    });
    this.newRecruiter = '';
  }

  addInternalNote(): void {
    if (!this.application || !this.noteText.trim()) return;
    this.application.internalNotes.unshift(this.noteText.trim());
    addAuditLog({
      adminUserId: 'admin-01',
      action: 'Add Internal Note',
      entityType: 'Application',
      entityId: this.application.id,
      newValue: this.noteText.trim(),
      reason: 'Admin note'
    });
    this.noteText = '';
  }
}
