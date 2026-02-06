import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { JobProviderService } from '../../../core/services/job-provider.service';

@Component({
  standalone: true,
  selector: 'app-job-provider-dashboard',
  imports: [CommonModule, RouterLink],
  templateUrl: './job-provider-dashboard.component.html',
  styleUrls: ['./job-provider-dashboard.component.scss']
})
export class JobProviderDashboardComponent implements OnInit {
  jobs: any[] = [];
  isLoading = true;
  isLoadingApplications = true;
  applicationsSummary: any = {
    totalApplicants: 0,
    appliedCount: 0,
    shortlistedCount: 0,
    rejectedCount: 0,
    hiredCount: 0,
    recent: []
  };
  calendarMonth = new Date();
  calendarDays: any[] = [];
  calendarWeekdays = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];
  scheduledInvites: any[] = [];
  invitesByDate = new Map<string, any[]>();
  selectedDate: Date | null = null;
  selectedInvites: any[] = [];
  calendarLoading = true;
  calendarError = '';

  constructor(private jobService: JobProviderService) {}

  ngOnInit(): void {
    this.jobService.getMyJobs().subscribe(res => {
      this.jobs = res as any[] ?? [];
      this.isLoading = false;
    });

    this.jobService.getApplicationsSummary(6).subscribe({
      next: res => {
        this.applicationsSummary = res ?? this.applicationsSummary;
      },
      complete: () => {
        this.isLoadingApplications = false;
      },
      error: () => {
        this.isLoadingApplications = false;
      }
    });

    this.loadCalendar();
  }

  get totalJobs(): number {
    return this.jobs.length;
  }

  get activeJobs(): number {
    return this.jobs.filter(job => job.status === 'Active').length;
  }

  get closedJobs(): number {
    return this.jobs.filter(job => job.status === 'Closed').length;
  }

  get draftJobs(): number {
    return this.jobs.filter(job => job.status === 'Draft').length;
  }

  get totalOpenings(): number {
    return this.jobs.reduce((sum, job) => sum + Number(job.openings ?? 0), 0);
  }

  get expiringSoonCount(): number {
    const now = new Date().getTime();
    const twoWeeks = 14 * 24 * 60 * 60 * 1000;
    return this.jobs.filter(job => {
      if (!job.expiryDate) return false;
      const expiry = new Date(job.expiryDate).getTime();
      return expiry >= now && expiry <= now + twoWeeks;
    }).length;
  }

  get activeRatio(): number {
    if (this.totalJobs === 0) return 0;
    return Math.round((this.activeJobs / this.totalJobs) * 100);
  }

  get recentJobs(): any[] {
    return [...this.jobs]
      .sort((a, b) => new Date(b.postedAt).getTime() - new Date(a.postedAt).getTime())
      .slice(0, 3);
  }

  get recentApplicants(): any[] {
    return this.applicationsSummary?.recent ?? [];
  }

  get applicantTotals(): number {
    return Number(this.applicationsSummary?.totalApplicants ?? 0);
  }

  get applicantApplied(): number {
    return Number(this.applicationsSummary?.appliedCount ?? 0);
  }

  get applicantShortlisted(): number {
    return Number(this.applicationsSummary?.shortlistedCount ?? 0);
  }

  get applicantRejected(): number {
    return Number(this.applicationsSummary?.rejectedCount ?? 0);
  }

  get applicantHired(): number {
    return Number(this.applicationsSummary?.hiredCount ?? 0);
  }

  getInitials(name: string): string {
    if (!name) return '?';
    const parts = name.trim().split(' ').filter(Boolean);
    const first = parts[0]?.[0] ?? '';
    const last = parts.length > 1 ? parts[parts.length - 1]?.[0] : '';
    return (first + last).toUpperCase();
  }

  loadCalendar(): void {
    this.calendarLoading = true;
    this.calendarError = '';

    const start = new Date(this.calendarMonth.getFullYear(), this.calendarMonth.getMonth(), 1);
    const end = new Date(this.calendarMonth.getFullYear(), this.calendarMonth.getMonth() + 1, 0);
    const from = new Date(start);
    from.setHours(0, 0, 0, 0);
    const to = new Date(end);
    to.setHours(23, 59, 59, 999);

    this.jobService.getScheduledInvites(from.toISOString(), to.toISOString()).subscribe({
      next: res => {
        this.scheduledInvites = res ?? [];
        this.buildCalendar();
      },
      error: () => {
        this.calendarError = 'Unable to load interview calendar.';
        this.scheduledInvites = [];
        this.buildCalendar();
      },
      complete: () => {
        this.calendarLoading = false;
      }
    });
  }

  buildCalendar(): void {
    const year = this.calendarMonth.getFullYear();
    const month = this.calendarMonth.getMonth();
    const firstDay = new Date(year, month, 1);
    const startWeekday = firstDay.getDay();
    const daysInMonth = new Date(year, month + 1, 0).getDate();
    const daysInPrevMonth = new Date(year, month, 0).getDate();

    this.invitesByDate = new Map<string, any[]>();
    this.scheduledInvites.forEach(invite => {
      if (!invite?.selectedSlot) return;
      const slot = new Date(invite.selectedSlot);
      const key = this.dateKey(slot);
      const list = this.invitesByDate.get(key) ?? [];
      list.push(invite);
      this.invitesByDate.set(key, list);
    });

    const days: any[] = [];
    for (let i = 0; i < 42; i++) {
      const dayNumber = i - startWeekday + 1;
      let date: Date;
      let inMonth = true;

      if (dayNumber < 1) {
        date = new Date(year, month - 1, daysInPrevMonth + dayNumber);
        inMonth = false;
      } else if (dayNumber > daysInMonth) {
        date = new Date(year, month + 1, dayNumber - daysInMonth);
        inMonth = false;
      } else {
        date = new Date(year, month, dayNumber);
      }

      const key = this.dateKey(date);
      days.push({
        date,
        key,
        label: date.getDate(),
        inMonth,
        count: (this.invitesByDate.get(key) ?? []).length
      });
    }

    this.calendarDays = days;

    const todayKey = this.dateKey(new Date());
    const todayInMonth = this.calendarDays.find(day => day.key === todayKey && day.inMonth);
    if (todayInMonth) {
      this.selectDay(todayInMonth);
    } else if (this.calendarDays.length) {
      this.selectDay(this.calendarDays.find(day => day.inMonth) ?? this.calendarDays[0]);
    }
  }

  selectDay(day: any): void {
    this.selectedDate = day?.date ?? null;
    const key = day?.key ?? '';
    this.selectedInvites = this.invitesByDate.get(key) ?? [];
  }

  isSelected(day: any): boolean {
    return !!this.selectedDate && this.dateKey(this.selectedDate) === day?.key;
  }

  isToday(day: any): boolean {
    return this.dateKey(new Date()) === day?.key;
  }

  prevMonth(): void {
    this.calendarMonth = new Date(this.calendarMonth.getFullYear(), this.calendarMonth.getMonth() - 1, 1);
    this.loadCalendar();
  }

  nextMonth(): void {
    this.calendarMonth = new Date(this.calendarMonth.getFullYear(), this.calendarMonth.getMonth() + 1, 1);
    this.loadCalendar();
  }

  private dateKey(date: Date): string {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }
}
