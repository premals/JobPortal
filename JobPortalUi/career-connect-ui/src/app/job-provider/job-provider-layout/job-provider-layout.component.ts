import { Component } from '@angular/core';
import { Router, RouterOutlet, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ViewProfileComponent } from '../../profile/view-profile/view-profile.component';
import { EditProfileComponent } from '../../profile/edit-profile/edit-profile.component';

@Component({
  selector: 'app-job-provider-layout',
  standalone: true,
  imports: [CommonModule, RouterModule, RouterOutlet, ViewProfileComponent, EditProfileComponent],
  templateUrl: './job-provider-layout.component.html',
  styleUrls: ['./job-provider-layout.component.scss']
})
export class JobProviderLayoutComponent {
  profileModalOpen = false;
  profileMode: 'view' | 'edit' = 'view';

  constructor(private router: Router) {}

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
}
