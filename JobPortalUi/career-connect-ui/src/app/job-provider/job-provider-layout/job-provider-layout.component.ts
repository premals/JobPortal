import { Component, OnInit } from '@angular/core';
import { Router, RouterOutlet, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ViewProfileComponent } from '../../profile/view-profile/view-profile.component';
import { EditProfileComponent } from '../../profile/edit-profile/edit-profile.component';
import { JobProviderService } from '../../core/services/job-provider.service';

@Component({
  selector: 'app-job-provider-layout',
  standalone: true,
  imports: [CommonModule, RouterModule, RouterOutlet, ViewProfileComponent, EditProfileComponent],
  templateUrl: './job-provider-layout.component.html',
  styleUrls: ['./job-provider-layout.component.scss']
})
export class JobProviderLayoutComponent implements OnInit {
  profileModalOpen = false;
  profileMode: 'view' | 'edit' = 'view';
  showNotifications = false;
  notificationsLoading = false;
  providerNotifications: any[] = [];
  notificationCount = 0;

  constructor(
    private router: Router,
    private jobProviderService: JobProviderService
  ) {}

  ngOnInit(): void {
    this.loadNotifications();
  }

  logout(): void {
    localStorage.clear();
    this.router.navigate(['/login']);
  }

  openProfile(mode: 'view' | 'edit'): void {
    this.profileMode = mode;
    this.profileModalOpen = true;
  }

  closeProfile(): void {
    this.profileModalOpen = false;
  }

  switchToView(): void {
    this.profileMode = 'view';
  }

  switchToEdit(): void {
    this.profileMode = 'edit';
  }

  toggleNotifications(): void {
    this.showNotifications = !this.showNotifications;
    if (this.showNotifications) {
      this.loadNotifications();
    }
  }

  closeNotifications(): void {
    this.showNotifications = false;
  }

  private loadNotifications(): void {
    const from = new Date();
    from.setDate(from.getDate() - 1);
    const to = new Date();
    to.setDate(to.getDate() + 30);
    this.notificationsLoading = true;

    this.jobProviderService.getScheduledInvites(from.toISOString(), to.toISOString()).subscribe({
      next: res => {
        const invites = Array.isArray(res) ? res : [];
        this.providerNotifications = invites
          .filter(invite => !!invite?.selectedSlot)
          .sort((a, b) => new Date(b.selectedSlot).getTime() - new Date(a.selectedSlot).getTime());
        this.notificationCount = this.providerNotifications.length;
      },
      error: () => {
        this.providerNotifications = [];
        this.notificationCount = 0;
      },
      complete: () => {
        this.notificationsLoading = false;
      }
    });
  }
}
