import { Component, signal, computed, inject } from '@angular/core';
import { NgClass, NgFor, DatePipe, NgIf } from '@angular/common';
import { Router } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { Header } from '../Header/header';
import { DashboardService } from '../../Services/dashboard.service';
import { AuthService } from '../../Services/auth.service';
import { EmployeeDashboardData, ComplaintSummary } from '../../models/dashboard.model';
import { tap, catchError, of, map } from 'rxjs';

@Component({
  selector: 'app-employee-dashboard',
  imports: [Header, NgClass, NgFor, NgIf, DatePipe],
  templateUrl: './employee-dashboard.html',
  styleUrl: './employee-dashboard.css',
})
export class EmployeeDashboard {
  private dashboardService = inject(DashboardService);
  private authService = inject(AuthService);
  private router = inject(Router);

  loading = signal(true);
  error = signal<string | null>(null);

  private dashboardData$ = this.dashboardService.getEmployeeDashboard().pipe(
    tap({
      next: () => this.loading.set(false),
      error: () => { this.loading.set(false); this.error.set('Failed to load dashboard data'); }
    }),
    catchError(() => of({
      assignedComplaints: 0, inProgressComplaints: 0, resolvedComplaints: 0,
      escalatedComplaints: 0, overdueComplaints: 0
    } as EmployeeDashboardData))
  );

  dashboardData = toSignal(this.dashboardData$, {
    initialValue: {
      assignedComplaints: 0, inProgressComplaints: 0, resolvedComplaints: 0,
      escalatedComplaints: 0, overdueComplaints: 0
    } as EmployeeDashboardData
  });

  private assignedComplaints$ = this.dashboardService.getAllComplaints(1, 5).pipe(
    map(res => res.complaints || []),
    tap({ error: () => this.error.set('Failed to load complaints') }),
    catchError(() => of([] as ComplaintSummary[]))
  );

  assignedComplaints = toSignal(this.assignedComplaints$, {
    initialValue: [] as ComplaintSummary[]
  });

  assignedCount = computed(() => this.dashboardData().assignedComplaints);
  inProgressCount = computed(() => this.dashboardData().inProgressComplaints);
  resolvedCount = computed(() => this.dashboardData().resolvedComplaints);
  escalatedCount = computed(() => this.dashboardData().escalatedComplaints);
  overdueCount = computed(() => this.dashboardData().overdueComplaints);

  constructor() {
    if (!this.authService.isLoggedIn()) {
      this.router.navigate(['/login']);
      return;
    }
  }

  statusClass(statusName: string): string {
    const map: Record<string, string> = {
      'Open': 'badge-open',
      'Assigned': 'badge-assigned',
      'InProgress': 'badge-progress',
      'Resolved': 'badge-resolved',
      'Closed': 'badge-closed',
      'Reopened': 'badge-reopened'
    };
    return map[statusName] || '';
  }

  priorityClass(priorityName?: string): string {
    const map: Record<string, string> = {
      'Critical': 'badge-danger',
      'High': 'badge-danger',
      'Medium': 'badge-warning',
      'Low': 'badge-open'
    };
    return priorityName ? (map[priorityName] || 'badge-open') : 'badge-open';
  }

  isOverdue(dueDate?: string): boolean {
    if (!dueDate) return false;
    return new Date(dueDate) < new Date();
  }

  viewAssigned() {
    this.router.navigate(['/employee/complaints']);
  }

  viewDetails(complaintId: number) {
    this.router.navigate(['/complaints', complaintId]);
  }

  goToProfile() {
    this.router.navigate(['/profile']);
  }
}
