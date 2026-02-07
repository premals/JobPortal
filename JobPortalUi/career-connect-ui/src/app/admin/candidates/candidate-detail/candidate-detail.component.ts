import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AdminCandidate, adminCandidates, addAuditLog } from '../../admin-data';

@Component({
  standalone: true,
  selector: 'app-candidate-detail',
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './candidate-detail.component.html',
  styleUrls: ['./candidate-detail.component.scss']
})
export class CandidateDetailComponent implements OnInit {
  candidate?: AdminCandidate;
  requestReason = '';

  constructor(private route: ActivatedRoute) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    this.candidate = adminCandidates.find(item => item.id === id);
  }

  toggleFlag(): void {
    if (!this.candidate) return;
    const oldStatus = this.candidate.status;
    this.candidate.status = this.candidate.status === 'Flagged' ? 'Active' : 'Flagged';
    addAuditLog({
      adminUserId: 'admin-01',
      action: this.candidate.status === 'Flagged' ? 'Flag Candidate' : 'Unflag Candidate',
      entityType: 'Candidate',
      entityId: this.candidate.id,
      oldValue: oldStatus,
      newValue: this.candidate.status,
      reason: 'Admin review'
    });
  }

  requestUpdate(): void {
    if (!this.candidate || !this.requestReason.trim()) return;
    const entry = {
      id: `REQ-${Date.now()}`,
      type: 'Profile Update',
      reason: this.requestReason.trim(),
      createdAt: new Date().toISOString()
    };
    this.candidate.adminRequests.unshift(entry);
    addAuditLog({
      adminUserId: 'admin-01',
      action: 'Request Candidate Update',
      entityType: 'Candidate',
      entityId: this.candidate.id,
      newValue: entry.reason,
      reason: 'Admin request'
    });
    this.requestReason = '';
  }

  hideCandidate(): void {
    if (!this.candidate) return;
    const oldStatus = this.candidate.status;
    this.candidate.status = 'Hidden';
    addAuditLog({
      adminUserId: 'admin-01',
      action: 'Hide Candidate',
      entityType: 'Candidate',
      entityId: this.candidate.id,
      oldValue: oldStatus,
      newValue: 'Hidden',
      reason: 'Admin action'
    });
  }

  exportJson(): void {
    if (!this.candidate || typeof window === 'undefined') return;
    const blob = new Blob([JSON.stringify(this.candidate, null, 2)], { type: 'application/json' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = `${this.candidate.id}-summary.json`;
    link.click();
    URL.revokeObjectURL(url);
  }
}

