import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';

@Component({
  selector: 'app-profile-popup',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './profile-popup.component.html'
})
export class ProfilePopupComponent {

  fullName = localStorage.getItem('fullName') || 'User';
  email = localStorage.getItem('email') || '';
  userType = localStorage.getItem('userType') || '';

  constructor(private router: Router) {}

  navigate(path: string): void {
    this.router.navigate([path]);
  }

  logout(): void {
    localStorage.clear();
    this.router.navigate(['/login']);
  }
}
