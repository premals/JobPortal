import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { AdminService } from '../../../core/services/admin.service';
import { AdminJobSeeker, addAuditLog } from '../../admin-data';
import { mapJobSeekerProjection } from '../../admin-api-mappers';

@Component({
  standalone: true,
  selector: 'app-job-seeker-detail',
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './job-seeker-detail.component.html',
  styleUrls: ['./job-seeker-detail.component.scss']
})
export class JobSeekerDetailComponent implements OnInit {
  jobSeeker?: AdminJobSeeker;
  editMode = false;
  editModel = {
    fullName: '',
    phone: '',
    city: '',
    headline: ''
  };
  noteText = '';

  constructor(private route: ActivatedRoute, private adminService: AdminService) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) return;

    this.adminService
      .getAdminJobSeekerById(id)
      .pipe(catchError(() => of(null)))
      .subscribe(jobSeeker => {
        if (!jobSeeker) return;
        this.jobSeeker = mapJobSeekerProjection(jobSeeker);
        this.editModel = {
          fullName: this.jobSeeker.fullName,
          phone: this.jobSeeker.phone ?? '',
          city: this.jobSeeker.city,
          headline: this.jobSeeker.headline ?? ''
        };
      });
  }

  toggleEdit(): void {
    this.editMode = !this.editMode;
  }

  save(): void {
    if (!this.jobSeeker) return;
    const oldValue = JSON.stringify({
      fullName: this.jobSeeker.fullName,
      phone: this.jobSeeker.phone,
      city: this.jobSeeker.city,
      headline: this.jobSeeker.headline
    });
    this.jobSeeker.fullName = this.editModel.fullName;
    this.jobSeeker.phone = this.editModel.phone;
    this.jobSeeker.city = this.editModel.city;
    this.jobSeeker.headline = this.editModel.headline;
    const newValue = JSON.stringify({
      fullName: this.jobSeeker.fullName,
      phone: this.jobSeeker.phone,
      city: this.jobSeeker.city,
      headline: this.jobSeeker.headline
    });
    addAuditLog({
      adminUserId: 'admin-01',
      action: 'Edit JobSeeker',
      entityType: 'JobSeeker',
      entityId: this.jobSeeker.id,
      oldValue,
      newValue,
      reason: 'Safe field update'
    });
    this.editMode = false;
  }

  addNote(): void {
    if (!this.jobSeeker || !this.noteText.trim()) return;
    this.jobSeeker.notes.unshift(this.noteText.trim());
    addAuditLog({
      adminUserId: 'admin-01',
      action: 'Add Note',
      entityType: 'JobSeeker',
      entityId: this.jobSeeker.id,
      newValue: this.noteText.trim(),
      reason: 'Internal note'
    });
    this.noteText = '';
  }

  toggleSuspend(): void {
    if (!this.jobSeeker || this.jobSeeker.status === 'Blocked') return;
    const oldStatus = this.jobSeeker.status;
    this.jobSeeker.status = this.jobSeeker.status === 'Suspended' ? 'Active' : 'Suspended';
    addAuditLog({
      adminUserId: 'admin-01',
      action: this.jobSeeker.status === 'Suspended' ? 'Suspend User' : 'Unsuspend User',
      entityType: 'JobSeeker',
      entityId: this.jobSeeker.id,
      oldValue: oldStatus,
      newValue: this.jobSeeker.status,
      reason: 'Admin action'
    });
  }

  toggleBlock(): void {
    if (!this.jobSeeker) return;
    const oldStatus = this.jobSeeker.status;
    this.jobSeeker.status = this.jobSeeker.status === 'Blocked' ? 'Active' : 'Blocked';
    addAuditLog({
      adminUserId: 'admin-01',
      action: this.jobSeeker.status === 'Blocked' ? 'Block User' : 'Unblock User',
      entityType: 'JobSeeker',
      entityId: this.jobSeeker.id,
      oldValue: oldStatus,
      newValue: this.jobSeeker.status,
      reason: 'Admin action'
    });
  }

  resetPassword(): void {
    if (!this.jobSeeker) return;
    this.jobSeeker.notes.unshift(`Password reset requested on ${new Date().toISOString().slice(0, 10)}`);
    addAuditLog({
      adminUserId: 'admin-01',
      action: 'Reset Password',
      entityType: 'JobSeeker',
      entityId: this.jobSeeker.id,
      oldValue: 'PasswordActive',
      newValue: 'ResetRequested',
      reason: 'Admin initiated reset'
    });
  }
}
