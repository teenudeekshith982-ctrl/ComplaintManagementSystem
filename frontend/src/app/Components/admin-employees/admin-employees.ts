import { Component, signal, computed, inject, OnInit } from '@angular/core';
import { NgClass, NgFor, NgIf, DatePipe } from '@angular/common';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Header } from '../Header/header';
import { AdminService } from '../../Services/admin.service';
import { AuthService } from '../../Services/auth.service';
import { ToastService } from '../../Services/toast.service';
import { UserListItem } from '../../models/admin.model';

@Component({
  selector: 'app-admin-employees',
  imports: [Header, NgClass, NgIf, FormsModule],
  templateUrl: './admin-employees.html',
  styleUrl: './admin-employees.css',
})
export class AdminEmployees implements OnInit {
  private adminService = inject(AdminService);
  private authService = inject(AuthService);
  private router = inject(Router);
  private toastService = inject(ToastService);

  // UI & Data State
  userList = signal<UserListItem[]>([]);
  loading = signal(true);
  error = signal<string | null>(null);

  // Pagination State
  pageNumber = signal(1);
  pageSize = signal(10);
  totalRecords = signal(0);
  totalPages = computed(() => Math.ceil(this.totalRecords() / this.pageSize()));

  ngOnInit(): void {
    if (!this.authService.isLoggedIn()) {
      this.router.navigate(['/login']);
      return;
    }
    this.loadUsers();
  }

  loadUsers(): void {
    this.loading.set(true);
    this.error.set(null);

    this.adminService.getUsers(this.pageNumber(), this.pageSize(), 'User').subscribe({
      next: (res) => {
        this.userList.set(res.data || []);
        this.totalRecords.set(res.totalRecords || 0);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Failed to load user accounts:', err);
        this.error.set('Failed to load user accounts.');
        this.loading.set(false);
      }
    });
  }

  toggleActive(userId: number, currentActive: boolean): void {
    this.adminService.toggleUserActive(userId, !currentActive).subscribe({
      next: () => {
        this.toastService.success(`User successfully ${!currentActive ? 'activated' : 'deactivated'}.`);
        this.loadUsers();
      },
      error: () => {
      }
    });
  }

  newEmployee(): void {
    this.router.navigate(['/admin/employees/new']);
  }

  backToDashboard(): void {
    this.router.navigate(['/admin']);
  }

  nextPage(): void {
    if (this.pageNumber() < this.totalPages()) {
      this.pageNumber.update(n => n + 1);
      this.loadUsers();
    }
  }

  prevPage(): void {
    if (this.pageNumber() > 1) {
      this.pageNumber.update(n => n - 1);
      this.loadUsers();
    }
  }
}

