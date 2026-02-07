import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { AdminService } from '../../../core/services/admin.service';
import { AdminJobProvider, addAuditLog } from '../../admin-data';
import { mapJobProviderProjection } from '../../admin-api-mappers';

@Component({
  standalone: true,
  selector: 'app-job-provider-detail',
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './job-provider-detail.component.html',
  styleUrls: ['./job-provider-detail.component.scss']
})
export class JobProviderDetailComponent implements OnInit {
  provider?: AdminJobProvider;
  noteText = '';

  constructor(private route: ActivatedRoute, private adminService: AdminService) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) return;

    this.adminService
      .getAdminJobProviderById(id)
      .pipe(catchError(() => of(null)))
      .subscribe(provider => {
        if (!provider) return;
        this.provider = mapJobProviderProjection(provider);
      });
  }

  toggleSuspend(): void {
    if (!this.provider || this.provider.status === 'Blocked') return;
    const oldStatus = this.provider.status;
    this.provider.status = this.provider.status === 'Suspended' ? 'Active' : 'Suspended';
    addAuditLog({
      adminUserId: 'admin-01',
      action: this.provider.status === 'Suspended' ? 'Suspend Provider' : 'Unsuspend Provider',
      entityType: 'JobProvider',
      entityId: this.provider.id,
      oldValue: oldStatus,
      newValue: this.provider.status,
      reason: 'Admin action'
    });
  }

  toggleBlock(): void {
    if (!this.provider) return;
    const oldStatus = this.provider.status;
    this.provider.status = this.provider.status === 'Blocked' ? 'Active' : 'Blocked';
    addAuditLog({
      adminUserId: 'admin-01',
      action: this.provider.status === 'Blocked' ? 'Block Provider' : 'Unblock Provider',
      entityType: 'JobProvider',
      entityId: this.provider.id,
      oldValue: oldStatus,
      newValue: this.provider.status,
      reason: 'Admin action'
    });
  }

  addNote(): void {
    if (!this.provider || !this.noteText.trim()) return;
    this.provider.notes.unshift(this.noteText.trim());
    addAuditLog({
      adminUserId: 'admin-01',
      action: 'Add Provider Note',
      entityType: 'JobProvider',
      entityId: this.provider.id,
      newValue: this.noteText.trim(),
      reason: 'Internal note'
    });
    this.noteText = '';
  }
}
