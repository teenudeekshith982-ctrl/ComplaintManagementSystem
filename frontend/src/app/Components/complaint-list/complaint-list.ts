import { Component, signal, computed, inject, EffectRef } from '@angular/core';
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
  selector: 'app-complaint-list',
  imports: [Header, NgClass, NgFor, NgIf, DatePipe, FormsModule],
  templateUrl: './complaint-list.html',
  styleUrl: './complaint-list.css',
})
export class ComplaintList {
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
  categoryFilter = signal<number | null>(null);
  searchTerm = signal('');
  fromDate = signal('');
  toDate = signal('');

  // Combined Filters state
  filters = computed(() => ({
    pageNumber: this.pageNumber(),
    pageSize: this.pageSize(),
    status: this.statusFilter() ?? undefined,
    category: this.categoryFilter() ?? undefined,
    searchTerm: this.searchTerm() || undefined,
    fromDate: this.fromDate() || undefined,
    toDate: this.toDate() || undefined
  }));

  // Reactive pipeline
  private complaints$ = toObservable(this.filters).pipe(
    tap(() => this.loading.set(true)),
    switchMap(f => this.complaintService.getUserComplaints(f)),
    tap({
      next: (res) => {
        this.loading.set(false);
        this.totalRecords.set(res.totalRecords);
      },
      error: (err) => {
        console.error(err);
        this.loading.set(false);
        this.error.set('Failed to load complaints.');
      }
    }),
    catchError(() => of({ items: [], totalRecords: 0, pageNumber: 1, pageSize: 10 }))
  );

  complaintsResponse = toSignal(this.complaints$, {
    initialValue: { items: [], totalRecords: 0, pageNumber: 1, pageSize: 10 }
  });

  complaints = computed(() => this.complaintsResponse().items || []);
  
  // Total Pages
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

  newComplaint() {
    this.router.navigate(['/complaints/new']);
  }

  viewDetails(complaintId: number) {
    this.router.navigate(['/complaints', complaintId]);
  }

  backToDashboard() {
    this.router.navigate(['/dashboard']);
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
    this.categoryFilter.set(null);
    this.searchTerm.set('');
    this.fromDate.set('');
    this.toDate.set('');
    this.pageNumber.set(1);
  }
}
