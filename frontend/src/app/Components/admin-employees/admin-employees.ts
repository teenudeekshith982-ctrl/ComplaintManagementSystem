import { Component, signal, computed, inject } from '@angular/core';
import { NgClass, NgFor, NgIf, DatePipe } from '@angular/common';
import { Router } from '@angular/router';
import { toSignal, toObservable } from '@angular/core/rxjs-interop';
import { Header } from '../Header/header';
import { AdminService } from '../../Services/admin.service';
import { AuthService } from '../../Services/auth.service';
import { ToastService } from '../../Services/toast.service';
import { UserListItem } from '../../models/admin.model';
import { tap, catchError, of, switchMap } from 'rxjs';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-admin-employees',
  imports: [Header, NgClass, NgFor, NgIf, DatePipe, FormsModule],
  templateUrl: './admin-employees.html',
  styleUrl: './admin-employees.css',
})
export class AdminEmployees {
  private adminService = inject(AdminService);
  private authService = inject(AuthService);
  private router = inject(Router);
  private toastService = inject(ToastService);

  loading = signal(true);
  error = signal<string | null>(null);
  totalRecords = signal(0);

  pageNumber = signal(1);
  pageSize = signal(10);
  
  // Reload trigger
  reloadTrigger = signal(0);

  // Combined state
  queryState = computed(() => ({
    page: this.pageNumber(),
    size: this.pageSize(),
    trigger: this.reloadTrigger()
  }));

  private users$ = toObservable(this.queryState).pipe(
    tap(() => this.loading.set(true)),
    switchMap(s => this.adminService.getUsers(s.page, s.size)),
    tap({
      next: (res) => {
        this.loading.set(false);
        this.totalRecords.set(res.totalRecords);
      },
      error: (err) => {
        console.error(err);
        this.loading.set(false);
        this.error.set('Failed to load user accounts.');
      }
    }),
    catchError(() => of({ data: [] as UserListItem[], totalRecords: 0, pageNumber: 1, pageSize: 10 }))
  );

  usersResponse = toSignal(this.users$, {
    initialValue: { data: [] as UserListItem[], totalRecords: 0, pageNumber: 1, pageSize: 10 }
  });

  userList = computed(() => this.usersResponse().data || []);
  totalPages = computed(() => Math.ceil(this.totalRecords() / this.pageSize()));

  constructor() {
    if (!this.authService.isLoggedIn()) {
      this.router.navigate(['/login']);
    }
  }

  toggleActive(userId: number, currentActive: boolean) {
    this.adminService.toggleUserActive(userId, !currentActive).subscribe({
      next: () => {
        this.reloadTrigger.update(t => t + 1); // Triggers list reload!
        this.toastService.success(`User successfully ${!currentActive ? 'activated' : 'deactivated'}.`);
      },
      error: (err) => this.toastService.error('Failed to update active state.')
    });
  }

  newEmployee() {
    this.router.navigate(['/admin/employees/new']);
  }

  backToDashboard() {
    this.router.navigate(['/admin']);
  }

  nextPage() {
    if (this.pageNumber() < this.totalPages()) {
      this.pageNumber.update(n => n + 1);
    }
  }

  prevPage() {
    if (this.pageNumber() > 1) {
      this.pageNumber.update(n => n - 1);
    }
  }
}
