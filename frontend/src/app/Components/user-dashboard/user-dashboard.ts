import { Component, signal, computed, inject } from '@angular/core';
import { NgClass, NgFor, DatePipe, NgIf } from '@angular/common';
import { Router } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { Header } from '../Header/header';
import { DashboardService } from '../../Services/dashboard.service';
import { AuthService } from '../../Services/auth.service';
import { UserDashboardData, ComplaintSummary } from '../../models/dashboard.model';
import { tap, catchError, of, map } from 'rxjs';

@Component({
  selector: 'app-user-dashboard',
  imports: [Header, NgClass, NgFor, NgIf, DatePipe],
  templateUrl: './user-dashboard.html',
  styleUrl: './user-dashboard.css',
})
export class UserDashboard {
  private dashboardService = inject(DashboardService);
  private authService = inject(AuthService);
  private router = inject(Router);

  userName = signal('');
  loading = signal(true);
  error = signal<string | null>(null);

  private dashboardData$ = this.dashboardService.getUserDashboard().pipe(
    tap({
      next: () => this.loading.set(false),
      error: () => { this.loading.set(false); this.error.set('Failed to load dashboard data'); }
    }),
    catchError(() => of({
      totalComplaints: 0, openComplaints: 0, assignedComplaints: 0,
      inProgressComplaints: 0, resolvedComplaints: 0, closedComplaints: 0
    } as UserDashboardData))
  );

  dashboardData = toSignal(this.dashboardData$, {
    initialValue: {
      totalComplaints: 0, openComplaints: 0, assignedComplaints: 0,
      inProgressComplaints: 0, resolvedComplaints: 0, closedComplaints: 0
    } as UserDashboardData
  });

  private recentComplaints$ = this.dashboardService.getUserComplaints(1, 5).pipe(
    map(res => res.items || []),
    tap({ error: () => this.error.set('Failed to load complaints') }),
    catchError(() => of([] as ComplaintSummary[]))
  );

  recentComplaints = toSignal(this.recentComplaints$, {
    initialValue: [] as ComplaintSummary[]
  });

  totalComplaints = computed(() => this.dashboardData().totalComplaints);
  openComplaints = computed(() => this.dashboardData().openComplaints);
  assignedComplaints = computed(() => this.dashboardData().assignedComplaints);
  inProgressComplaints = computed(() => this.dashboardData().inProgressComplaints);
  resolvedComplaints = computed(() => this.dashboardData().resolvedComplaints);
  closedComplaints = computed(() => this.dashboardData().closedComplaints);

  constructor() {
    if (!this.authService.isLoggedIn()) {
      this.router.navigate(['/login']);
      return;
    }
    const user = this.authService.getUserInfo();
    if (user) this.userName.set(user.name);
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

  newComplaint() {
    this.router.navigate(['/complaints/new']);
  }

  viewAllComplaints() {
    this.router.navigate(['/complaints']);
  }

  viewDetails(complaintId: number) {
    this.router.navigate(['/complaints', complaintId]);
  }

  goToProfile() {
    this.router.navigate(['/profile']);
  }
}
