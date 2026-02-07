import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminAuditLog, adminAuditLogs } from '../admin-data';

@Component({
  standalone: true,
  selector: 'app-audit-logs',
  imports: [CommonModule, FormsModule],
  templateUrl: './audit-logs.component.html',
  styleUrls: ['./audit-logs.component.scss']
})
export class AuditLogsComponent implements OnInit {
  logs = adminAuditLogs;
  filtered: AdminAuditLog[] = [];

  searchTerm = '';
  actionFilter = 'all';
  entityFilter = 'all';
  dateFrom = '';
  dateTo = '';

  ngOnInit(): void {
    this.applyFilters();
  }

  applyFilters(): void {
    const search = this.searchTerm.trim().toLowerCase();
    const from = this.dateFrom ? new Date(this.dateFrom) : null;
    const to = this.dateTo ? new Date(this.dateTo) : null;

    this.filtered = this.logs.filter(log => {
      const matchesSearch =
        !search ||
        log.adminUserId.toLowerCase().includes(search) ||
        log.entityId.toLowerCase().includes(search);
      const matchesAction = this.actionFilter === 'all' || log.action === this.actionFilter;
      const matchesEntity = this.entityFilter === 'all' || log.entityType === this.entityFilter;
      const timestamp = new Date(log.timestamp);
      const matchesDate = (!from || timestamp >= from) && (!to || timestamp <= to);
      return matchesSearch && matchesAction && matchesEntity && matchesDate;
    });
  }
}

