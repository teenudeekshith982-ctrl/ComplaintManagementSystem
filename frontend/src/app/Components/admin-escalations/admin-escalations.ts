import { Component, signal, computed, inject } from '@angular/core';
import { NgClass, NgFor, NgIf, DatePipe } from '@angular/common';
import { Router } from '@angular/router';
import { toSignal, toObservable } from '@angular/core/rxjs-interop';
import { Header } from '../Header/header';
import { EscalationService } from '../../Services/escalation.service';
import { AuthService } from '../../Services/auth.service';
import { ToastService } from '../../Services/toast.service';
import { EscalationItem } from '../../models/escalation.model';
import { tap, catchError, of, switchMap } from 'rxjs';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-admin-escalations',
  imports: [Header, NgFor, NgIf, NgClass, DatePipe, FormsModule],
  templateUrl: './admin-escalations.html',
  styleUrl: './admin-escalations.css',
})
export class AdminEscalations {
  private escalationService = inject(EscalationService);
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

  // Combined State
  queryState = computed(() => ({
    page: this.pageNumber(),
    size: this.pageSize(),
    trigger: this.reloadTrigger()
  }));

  private escalations$ = toObservable(this.queryState).pipe(
    tap(() => this.loading.set(true)),
    switchMap(s => this.escalationService.getEscalations(s.page, s.size)),
    tap({
      next: (res) => {
        this.loading.set(false);
        this.totalRecords.set(res.totalRecords);
      },
      error: (err) => {
        console.error(err);
        this.loading.set(false);
        this.error.set('Failed to load pending escalations.');
      }
    }),
    catchError(() => of({ data: [] as EscalationItem[], totalRecords: 0, pageNumber: 1, pageSize: 10 }))
  );

  escalationsResponse = toSignal(this.escalations$, {
    initialValue: { data: [] as EscalationItem[], totalRecords: 0, pageNumber: 1, pageSize: 10 }
  });

  escalations = computed(() => this.escalationsResponse().data || []);
  totalPages = computed(() => Math.ceil(this.totalRecords() / this.pageSize()));

  // Action modal states
  showModal = signal(false);
  activeAction = signal<'Accept' | 'ReassignBack'>('ReassignBack');
  activeEscalation = signal<EscalationItem | null>(null);
  actionComments = signal('');
  submittingAction = signal(false);

  constructor() {
    if (!this.authService.isLoggedIn()) {
      this.router.navigate(['/login']);
    }
  }

  openActionModal(escalation: EscalationItem, action: 'Accept' | 'ReassignBack') {
    this.activeEscalation.set(escalation);
    this.activeAction.set(action);
    this.actionComments.set('');
    this.showModal.set(true);
  }

  submitAction() {
    const esc = this.activeEscalation();
    if (!esc) return;

    const comments = this.actionComments().trim();
    if (!comments) {
      this.toastService.error('Please provide feedback comments.');
      return;
    }

    this.submittingAction.set(true);
    this.escalationService.resolveEscalation(esc.escalatedId, this.activeAction(), comments).subscribe({
      next: () => {
        this.submittingAction.set(false);
        this.showModal.set(false);
        this.reloadTrigger.update(t => t + 1); // reload list!
        this.toastService.success('Escalation resolved successfully.');
      },
      error: (err) => {
        this.toastService.error('Failed to resolve escalation: ' + (err.error?.message || ''));
        this.submittingAction.set(false);
      }
    });
  }

  viewDetails(complaintId: number) {
    this.router.navigate(['/complaints', complaintId]);
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
