import { Component, signal, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Header } from '../Header/header';
import { EscalationService } from '../../Services/escalation.service';
import { ComplaintService } from '../../Services/complaint.service';
import { AuthService } from '../../Services/auth.service';
import { catchError, of, map } from 'rxjs';
import { toSignal } from '@angular/core/rxjs-interop';
import { NgFor, NgIf, NgClass } from '@angular/common';
import { ComplaintListItem } from '../../models/complaint.model';

@Component({
  selector: 'app-employee-escalate',
  imports: [Header, FormsModule, NgFor, NgIf, NgClass],
  templateUrl: './employee-escalate.html',
  styleUrl: './employee-escalate.css',
})
export class EmployeeEscalate {
  private escalationService = inject(EscalationService);
  private complaintService = inject(ComplaintService);
  private authService = inject(AuthService);
  private router = inject(Router);

  complaintId = signal<number | null>(null);
  reason = signal('');
  submitting = signal(false);
  submitError = signal<string | null>(null);
  successMessage = signal<string | null>(null);

  private complaints$ = this.complaintService.getAllComplaints({ pageSize: 100 }).pipe(
    map(res => (res.complaints || []).filter(c => c.status === 'InProgress' || c.status === 'Reopened')),
    catchError(() => of([] as ComplaintListItem[]))
  );

  complaints = toSignal(this.complaints$, { initialValue: [] as ComplaintListItem[] });

  constructor() {
    if (!this.authService.isLoggedIn()) {
      this.router.navigate(['/login']);
    }
  }

  onComplaintSelected(complaintId: number) {
    this.complaintId.set(complaintId);
    this.submitError.set(null);
  }

  submit() {
    if (!this.complaintId() || !this.reason().trim()) {
      this.submitError.set('Please select a complaint and provide a reason.');
      return;
    }
    this.submitting.set(true);
    this.submitError.set(null);
    this.successMessage.set(null);
    this.escalationService.createEscalation({
      complaintId: Number(this.complaintId()!),
      reason: this.reason()
    }).subscribe({
      next: () => {
        this.submitting.set(false);
        this.successMessage.set('Escalation request submitted successfully. Waiting for admin review.');
        setTimeout(() => this.router.navigate(['/employee/complaints']), 2000);
      },
      error: (err: any) => {
        this.submitting.set(false);
        this.submitError.set(err.error?.message || 'Failed to escalate complaint. Please try again.');
      }
    });
  }

  cancel() {
    this.router.navigate(['/employee']);
  }
}
