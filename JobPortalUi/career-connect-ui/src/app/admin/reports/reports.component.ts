import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminReport, adminReports, addAuditLog } from '../admin-data';

@Component({
  standalone: true,
  selector: 'app-reports',
  imports: [CommonModule, FormsModule],
  templateUrl: './reports.component.html',
  styleUrls: ['./reports.component.scss']
})
export class ReportsComponent implements OnInit {
  reports = adminReports;
  filtered: AdminReport[] = [];
  typeFilter = 'all';
  statusFilter = 'all';
  searchTerm = '';

  ngOnInit(): void {
    this.applyFilters();
  }

  applyFilters(): void {
    const search = this.searchTerm.trim().toLowerCase();
    this.filtered = this.reports.filter(report => {
      const matchesType = this.typeFilter === 'all' || report.type === this.typeFilter;
      const matchesStatus = this.statusFilter === 'all' || report.status === this.statusFilter;
      const matchesSearch =
        !search ||
        report.targetName.toLowerCase().includes(search) ||
        report.reporter.toLowerCase().includes(search);
      return matchesType && matchesStatus && matchesSearch;
    });
  }

  resolveReport(report: AdminReport): void {
    const oldStatus = report.status;
    report.status = 'Resolved';
    addAuditLog({
      adminUserId: 'admin-01',
      action: 'Resolve Report',
      entityType: 'Report',
      entityId: report.id,
      oldValue: oldStatus,
      newValue: report.status,
      reason: 'Resolved by admin'
    });
    this.applyFilters();
  }

  banUser(report: AdminReport): void {
    addAuditLog({
      adminUserId: 'admin-01',
      action: 'Ban User',
      entityType: report.targetType,
      entityId: report.id,
      reason: 'Moderation action'
    });
  }

  hideTarget(report: AdminReport): void {
    addAuditLog({
      adminUserId: 'admin-01',
      action: report.targetType === 'Job' ? 'Hide Job' : 'Hide Profile',
      entityType: report.targetType,
      entityId: report.id,
      reason: 'Report resolution'
    });
  }
}

