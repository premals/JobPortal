import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { AdminService } from '../../../core/services/admin.service';
import { AdminJobProvider, addAuditLog } from '../../admin-data';
import { mapJobProviderProjection } from '../../admin-api-mappers';

@Component({
  standalone: true,
  selector: 'app-job-provider-list',
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './job-provider-list.component.html',
  styleUrls: ['./job-provider-list.component.scss']
})
export class JobProviderListComponent implements OnInit {
  providers: AdminJobProvider[] = [];
  filtered: AdminJobProvider[] = [];
  paged: AdminJobProvider[] = [];

  searchTerm = '';
  statusFilter = 'all';
  createdFrom = '';
  createdTo = '';
  page = 1;
  pageSize = 5;
  totalPages = 1;

  constructor(private adminService: AdminService) {}

  ngOnInit(): void {
    this.adminService
      .getAdminJobProviders()
      .pipe(catchError(() => of([])))
      .subscribe(providers => {
        this.providers = providers.map(mapJobProviderProjection);
        this.applyFilters();
      });
  }

  applyFilters(): void {
    const search = this.searchTerm.trim().toLowerCase();
    const from = this.createdFrom ? new Date(this.createdFrom) : null;
    const to = this.createdTo ? new Date(this.createdTo) : null;

    let data = this.providers.filter(provider => {
      const matchesSearch =
        !search ||
        provider.companyName.toLowerCase().includes(search) ||
        provider.contactName.toLowerCase().includes(search) ||
        provider.email.toLowerCase().includes(search);
      const matchesStatus = this.statusFilter === 'all' || provider.status === this.statusFilter;
      const createdAt = new Date(provider.createdAt);
      const matchesDate = (!from || createdAt >= from) && (!to || createdAt <= to);
      return matchesSearch && matchesStatus && matchesDate;
    });

    data = data.sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());
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

  toggleSuspend(provider: AdminJobProvider): void {
    if (provider.status === 'Blocked') return;
    const oldStatus = provider.status;
    provider.status = provider.status === 'Suspended' ? 'Active' : 'Suspended';
    addAuditLog({
      adminUserId: 'admin-01',
      action: provider.status === 'Suspended' ? 'Suspend Provider' : 'Unsuspend Provider',
      entityType: 'JobProvider',
      entityId: provider.id,
      oldValue: oldStatus,
      newValue: provider.status,
      reason: 'Admin action'
    });
    this.applyFilters();
  }

  toggleBlock(provider: AdminJobProvider): void {
    const oldStatus = provider.status;
    provider.status = provider.status === 'Blocked' ? 'Active' : 'Blocked';
    addAuditLog({
      adminUserId: 'admin-01',
      action: provider.status === 'Blocked' ? 'Block Provider' : 'Unblock Provider',
      entityType: 'JobProvider',
      entityId: provider.id,
      oldValue: oldStatus,
      newValue: provider.status,
      reason: 'Admin action'
    });
    this.applyFilters();
  }
}
