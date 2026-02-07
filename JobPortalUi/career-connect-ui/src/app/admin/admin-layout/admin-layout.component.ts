import { Component, ViewEncapsulation } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  standalone: true,
  selector: 'app-admin-layout',
  imports: [CommonModule, RouterModule],
  templateUrl: './admin-layout.component.html',
  styleUrls: ['./admin-layout.component.scss'],
  encapsulation: ViewEncapsulation.None
})
export class AdminLayoutComponent {
  sidebarOpen = false;
  adminName = 'Admin User';
  adminEmail = '';

  constructor(private authService: AuthService) {
    if (typeof window !== 'undefined' && typeof localStorage !== 'undefined') {
      this.adminEmail = localStorage.getItem('email') ?? '';
      const emailName = this.adminEmail.split('@')[0];
      if (emailName) {
        this.adminName = emailName.replace('.', ' ');
      }
    }
  }

  toggleSidebar(): void {
    this.sidebarOpen = !this.sidebarOpen;
  }

  logout(): void {
    this.authService.logout();
  }
}

