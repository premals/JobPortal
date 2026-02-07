import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { AdminCandidate, adminCandidates } from '../../admin-data';

@Component({
  standalone: true,
  selector: 'app-candidate-list',
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './candidate-list.component.html',
  styleUrls: ['./candidate-list.component.scss']
})
export class CandidateListComponent implements OnInit {
  candidates = adminCandidates;
  filtered: AdminCandidate[] = [];
  paged: AdminCandidate[] = [];

  searchTerm = '';
  statusFilter = 'all';
  resumeFilter = 'all';
  sortKey = 'lastUpdated';
  sortDir: 'asc' | 'desc' = 'desc';
  page = 1;
  pageSize = 5;
  totalPages = 1;

  ngOnInit(): void {
    this.applyFilters();
  }

  applyFilters(): void {
    const search = this.searchTerm.trim().toLowerCase();
    let data = this.candidates.filter(candidate => {
      const matchesSearch =
        !search ||
        candidate.fullName.toLowerCase().includes(search) ||
        candidate.email.toLowerCase().includes(search);
      const matchesStatus = this.statusFilter === 'all' || candidate.status === this.statusFilter;
      const matchesResume =
        this.resumeFilter === 'all' ||
        (this.resumeFilter === 'yes' ? candidate.resume.present : !candidate.resume.present);
      return matchesSearch && matchesStatus && matchesResume;
    });

    data = data.sort((a, b) => {
      const dir = this.sortDir === 'asc' ? 1 : -1;
      switch (this.sortKey) {
        case 'fullName':
          return a.fullName.localeCompare(b.fullName) * dir;
        case 'profileCompleteness':
          return (a.profileCompleteness - b.profileCompleteness) * dir;
        default:
          return (new Date(a.lastUpdated).getTime() - new Date(b.lastUpdated).getTime()) * dir;
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
}

