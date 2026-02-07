import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { AdminService } from '../../../core/services/admin.service';
import { AdminJobSeeker, addAuditLog } from '../../admin-data';
import { mapJobSeekerProjection } from '../../admin-api-mappers';

@Component({
  standalone: true,
  selector: 'app-job-seeker-list',
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './job-seeker-list.component.html',
  styleUrls: ['./job-seeker-list.component.scss']
})
export class JobSeekerListComponent implements OnInit {
  jobSeekers: AdminJobSeeker[] = [];
  filtered: AdminJobSeeker[] = [];
  paged: AdminJobSeeker[] = [];

  searchTerm = '';
  statusFilter = 'all';
  cityFilter = 'all';
  createdFrom = '';
  createdTo = '';
  lastActiveFrom = '';
  lastActiveTo = '';
  sortKey = 'lastActiveAt';
  sortDir: 'asc' | 'desc' = 'desc';
  page = 1;
  pageSize = 5;
  totalPages = 1;

  constructor(private adminService: AdminService) {}

  ngOnInit(): void {
    this.adminService
      .getAdminJobSeekers()
      .pipe(catchError(() => of([])))
      .subscribe(jobSeekers => {
        this.jobSeekers = jobSeekers.map(mapJobSeekerProjection);
        this.applyFilters();
      });
  }

  get cityOptions(): string[] {
    return Array.from(new Set(this.jobSeekers.map(user => user.city))).sort();
  }

  applyFilters(): void {
    const search = this.searchTerm.trim().toLowerCase();
    const createdFrom = this.createdFrom ? new Date(this.createdFrom) : null;
    const createdTo = this.createdTo ? new Date(this.createdTo) : null;
    const lastActiveFrom = this.lastActiveFrom ? new Date(this.lastActiveFrom) : null;
    const lastActiveTo = this.lastActiveTo ? new Date(this.lastActiveTo) : null;

    let data = this.jobSeekers.filter(user => {
      const matchesSearch =
        !search ||
        user.fullName.toLowerCase().includes(search) ||
        user.email.toLowerCase().includes(search);
      const matchesStatus = this.statusFilter === 'all' || user.status === this.statusFilter;
      const matchesCity = this.cityFilter === 'all' || user.city === this.cityFilter;
      const createdAt = new Date(user.createdAt);
      const lastActive = new Date(user.lastActiveAt);
      const matchesCreated = (!createdFrom || createdAt >= createdFrom) && (!createdTo || createdAt <= createdTo);
      const matchesLastActive = (!lastActiveFrom || lastActive >= lastActiveFrom) && (!lastActiveTo || lastActive <= lastActiveTo);

      return matchesSearch && matchesStatus && matchesCity && matchesCreated && matchesLastActive;
    });

    data = data.sort((a, b) => {
      const dir = this.sortDir === 'asc' ? 1 : -1;
      switch (this.sortKey) {
        case 'fullName':
          return a.fullName.localeCompare(b.fullName) * dir;
        case 'createdAt':
          return (new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime()) * dir;
        case 'status':
          return a.status.localeCompare(b.status) * dir;
        default:
          return (new Date(a.lastActiveAt).getTime() - new Date(b.lastActiveAt).getTime()) * dir;
      }
    });

    this.filtered = data;
    this.page = 1;
    this.updatePage();
  }

  updatePage(): void {
    this.totalPages = Math.max(Math.ceil(this.filtered.length / this.pageSize), 1);
    const start = (this.page - 1) * this.pageSize;
    this.paged = this.filtered.slice(start, start + this.pageSize);
  }

  changePage(delta: number): void {
    const next = this.page + delta;
    if (next < 1 || next > this.totalPages) return;
    this.page = next;
    this.updatePage();
  }

  toggleSuspend(user: AdminJobSeeker): void {
    if (user.status === 'Blocked') return;
    const oldStatus = user.status;
    user.status = user.status === 'Suspended' ? 'Active' : 'Suspended';
    addAuditLog({
      adminUserId: 'admin-01',
      action: user.status === 'Suspended' ? 'Suspend User' : 'Unsuspend User',
      entityType: 'JobSeeker',
      entityId: user.id,
      oldValue: oldStatus,
      newValue: user.status,
      reason: 'Admin action'
    });
    this.applyFilters();
  }

  toggleBlock(user: AdminJobSeeker): void {
    const oldStatus = user.status;
    user.status = user.status === 'Blocked' ? 'Active' : 'Blocked';
    addAuditLog({
      adminUserId: 'admin-01',
      action: user.status === 'Blocked' ? 'Block User' : 'Unblock User',
      entityType: 'JobSeeker',
      entityId: user.id,
      oldValue: oldStatus,
      newValue: user.status,
      reason: 'Admin action'
    });
    this.applyFilters();
  }

  resetPassword(user: AdminJobSeeker): void {
    user.notes.unshift(`Password reset requested on ${new Date().toISOString().slice(0, 10)}`);
    addAuditLog({
      adminUserId: 'admin-01',
      action: 'Reset Password',
      entityType: 'JobSeeker',
      entityId: user.id,
      oldValue: 'PasswordActive',
      newValue: 'ResetRequested',
      reason: 'Admin initiated reset'
    });
  }
}
