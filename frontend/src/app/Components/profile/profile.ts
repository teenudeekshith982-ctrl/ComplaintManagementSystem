import { Component, signal, computed, inject, OnInit } from '@angular/core';
import { DatePipe, NgClass, NgIf } from '@angular/common';
import { Router } from '@angular/router';
import { Header } from '../Header/header';
import { ProfileService } from '../../Services/profile.service';
import { AuthService } from '../../Services/auth.service';
import { ProfileData } from '../../models/profile.model';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-profile',
  imports: [Header, DatePipe, NgIf, NgClass, FormsModule],
  templateUrl: './profile.html',
  styleUrl: './profile.css',
})
export class Profile implements OnInit {
  private profileService = inject(ProfileService);
  private authService = inject(AuthService);
  private router = inject(Router);

  loading = signal(true);
  error = signal<string | null>(null);
  successMessage = signal<string | null>(null);
  errorMessage = signal<string | null>(null);

  profile = signal<ProfileData | null>(null);
  
  // Tab control
  activeTab = signal<'view' | 'edit' | 'password'>('view');

  // Form states
  nameInput = signal('');
  phoneInput = signal('');

  currentPassword = signal('');
  newPassword = signal('');
  confirmPassword = signal('');

  userName = computed(() => this.profile()?.name ?? '');
  userEmail = computed(() => this.profile()?.email ?? '');
  userPhone = computed(() => this.profile()?.phone ?? '');
  userRole = computed(() => this.profile()?.role ?? '');
  joinedDate = computed(() => this.profile()?.joinedDate ?? '');
  departmentName = computed(() => this.profile()?.employeeInfo?.departmentName ?? '');

  ngOnInit() {
    if (!this.authService.isLoggedIn()) {
      this.router.navigate(['/login']);
      return;
    }
    this.loadProfile();
  }

  loadProfile() {
    this.loading.set(true);
    this.error.set(null);
    this.profileService.getProfile().subscribe({
      next: (data) => {
        this.profile.set(data);
        this.nameInput.set(data.name);
        this.phoneInput.set(data.phone);
        this.loading.set(false);
      },
      error: (err) => {
        console.error(err);
        this.error.set('Failed to load profile details.');
        this.loading.set(false);
      }
    });
  }

  updateProfile() {
    if (!this.nameInput().trim() || !this.phoneInput().trim()) {
      this.errorMessage.set('Name and Phone are required.');
      return;
    }

    this.loading.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);

    this.profileService.updateProfile({
      name: this.nameInput(),
      phone: this.phoneInput()
    }).subscribe({
      next: (res) => {
        this.successMessage.set('Profile updated successfully.');
        this.activeTab.set('view');
        this.loadProfile();
      },
      error: (err) => {
        this.loading.set(false);
        this.errorMessage.set(err.error?.message || 'Failed to update profile.');
      }
    });
  }

  changePassword() {
    const curP = this.currentPassword().trim();
    const newP = this.newPassword().trim();
    const conP = this.confirmPassword().trim();

    if (!curP || !newP || !conP) {
      this.errorMessage.set('All password fields are required.');
      return;
    }
    if (newP.length < 8) {
      this.errorMessage.set('New password must be at least 8 characters long.');
      return;
    }
    if (newP !== conP) {
      this.errorMessage.set('Passwords do not match.');
      return;
    }

    this.loading.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);

    this.profileService.changePassword({
      currentPassword: curP,
      newPassword: newP,
      confirmPassword: conP
    }).subscribe({
      next: () => {
        this.successMessage.set('Password changed successfully.');
        this.currentPassword.set('');
        this.newPassword.set('');
        this.confirmPassword.set('');
        this.activeTab.set('view');
        this.loading.set(false);
      },
      error: (err) => {
        this.loading.set(false);
        this.errorMessage.set(err.error?.message || 'Failed to change password. Make sure current password is correct.');
      }
    });
  }

  backToDashboard() {
    const user = this.authService.getUserInfo();
    if (user?.role === 'Admin') this.router.navigate(['/admin']);
    else if (user?.role === 'Employee') this.router.navigate(['/employee']);
    else this.router.navigate(['/dashboard']);
  }
}
