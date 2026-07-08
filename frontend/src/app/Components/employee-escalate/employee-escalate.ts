import { Component, signal, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Header } from '../Header/header';
import { EscalationService } from '../../Services/escalation.service';
import { ComplaintService } from '../../Services/complaint.service';
import { AuthService } from '../../Services/auth.service';
import { tap, catchError, of, map } from 'rxjs';
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
  escalationLevel = signal(1);
  reason = signal('');
  submitting = signal(false);
  submitError = signal<string | null>(null);
  successMessage = signal<string | null>(null);

  private complaints$ = this.complaintService.getAllComplaints({ status: 3 }).pipe( // Only InProgress can be manually escalated
    map(res => res.complaints || []),
    catchError(() => of([] as ComplaintListItem[]))
  );

  complaints = toSignal(this.complaints$, { initialValue: [] as ComplaintListItem[] });

  constructor() {
    if (!this.authService.isLoggedIn()) {
      this.router.navigate(['/login']);
    }
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
      escalationLevel: Number(this.escalationLevel()),
      reason: this.reason()
    }).subscribe({
      next: () => {
        this.submitting.set(false);
        this.successMessage.set('Complaint escalated successfully.');
        setTimeout(() => this.router.navigate(['/employee/complaints']), 1500);
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
