import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';

import { ProfileService } from '../../core/services/profile.service';
import { ProfileResponse } from '../../core/models/profile/profile-response.model';
import {AuthService} from '../../core/services/auth.service';

@Component({
  selector: 'app-view-profile',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './view-profile.component.html'
})
export class ViewProfileComponent implements OnInit {

  profile!: ProfileResponse;
  isLoading = true;

  constructor(
    private profileService: ProfileService,
    private router: Router,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.profileService.getProfile().subscribe({
      next: (res) => {
        this.profile = res;
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
      }
    });
  }

  editProfile(): void {
    this.router.navigate(['/profile/edit']);
  }

  logout(): void {
    this.authService.logout();
  }
   
}
