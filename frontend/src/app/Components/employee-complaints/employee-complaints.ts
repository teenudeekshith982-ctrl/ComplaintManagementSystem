import { Component, signal, computed, inject } from '@angular/core';
import { NgClass, NgFor, NgIf, DatePipe } from '@angular/common';
import { Router } from '@angular/router';
import { toSignal, toObservable } from '@angular/core/rxjs-interop';
import { Header } from '../Header/header';
import { ComplaintService } from '../../Services/complaint.service';
import { AuthService } from '../../Services/auth.service';
import { ComplaintListItem } from '../../models/complaint.model';
import { tap, catchError, of, switchMap } from 'rxjs';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-employee-complaints',
  imports: [Header, NgClass, NgFor, NgIf, DatePipe, FormsModule],
  templateUrl: './employee-complaints.html',
  styleUrl: './employee-complaints.css',
})
export class EmployeeComplaints {
  private complaintService = inject(ComplaintService);
  private authService = inject(AuthService);
  private router = inject(Router);

  loading = signal(true);
  error = signal<string | null>(null);
  totalRecords = signal(0);

  // Filter Signals
  pageNumber = signal(1);
  pageSize = signal(10);
  statusFilter = signal<number | null>(null);
  priorityFilter = signal<number | null>(null);
  searchTerm = signal('');

  // Combined Filters Signal
  filters = computed(() => ({
    pageNumber: this.pageNumber(),
    pageSize: this.pageSize(),
    status: this.statusFilter() ?? undefined,
    priority: this.priorityFilter() ?? undefined,
    searchTerm: this.searchTerm() || undefined
  }));

  // Reactive Pipeline
  private complaints$ = toObservable(this.filters).pipe(
    tap(() => this.loading.set(true)),
    switchMap(f => this.complaintService.getAllComplaints(f)), // Backend automatically filters by employee role/ID!
    tap({
      next: (res) => {
        this.loading.set(false);
        this.totalRecords.set(res.totalRecords);
      },
      error: (err) => {
        console.error(err);
        this.loading.set(false);
        this.error.set('Failed to load assigned complaints.');
      }
    }),
    catchError(() => of({ complaints: [], totalRecords: 0, pageNumber: 1, pageSize: 10 }))
  );

  complaintsResponse = toSignal(this.complaints$, {
    initialValue: { complaints: [], totalRecords: 0, pageNumber: 1, pageSize: 10 }
  });

  complaints = computed(() => this.complaintsResponse().complaints || []);
  totalPages = computed(() => Math.ceil(this.totalRecords() / this.pageSize()));

  constructor() {
    if (!this.authService.isLoggedIn()) {
      this.router.navigate(['/login']);
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

  viewDetails(complaintId: number) {
    this.router.navigate(['/complaints', complaintId]);
  }

  backToDashboard() {
    this.router.navigate(['/employee']);
  }

  // Paging controls
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

  resetFilters() {
    this.statusFilter.set(null);
    this.priorityFilter.set(null);
    this.searchTerm.set('');
    this.pageNumber.set(1);
  }
}
