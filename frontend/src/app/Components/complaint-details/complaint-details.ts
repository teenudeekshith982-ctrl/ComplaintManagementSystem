import { Component, signal, inject, OnInit } from '@angular/core';
import { NgClass, NgFor, NgIf, DatePipe } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { Header } from '../Header/header';
import { ComplaintService } from '../../Services/complaint.service';
import { AuthService } from '../../Services/auth.service';
import { AdminService } from '../../Services/admin.service';
import { EscalationService } from '../../Services/escalation.service';
import { ToastService } from '../../Services/toast.service';
import { ComplaintDetails } from '../../models/complaint.model';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-complaint-details',
  imports: [Header, NgClass, NgFor, NgIf, DatePipe, FormsModule],
  templateUrl: './complaint-details.html',
  styleUrl: './complaint-details.css',
})
export class ComplaintDetailsComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private complaintService = inject(ComplaintService);
  private authService = inject(AuthService);
  private adminService = inject(AdminService);
  private escalationService = inject(EscalationService);
  private toastService = inject(ToastService);

  complaintId = 0;
  role = signal('');
  userId = signal(0);
  loading = signal(true);
  error = signal<string | null>(null);

  // States
  complaint = signal<ComplaintDetails | null>(null);
  trackingLogs = signal<{ action: string; description: string; performedBy: string; createdAt: string }[]>([]);
  employees = signal<{ employeeId: number; employeeName: string; designation: string }[]>([]);

  // Action Inputs
  newCommentText = signal('');
  selectedPriority = signal<number>(4); // Default Low
  selectedEmployeeId = signal<number>(0);
  escalationReason = signal('');

  // UI state
  showEscalationModal = signal(false);
  submittingAction = signal(false);

  ngOnInit() {
    const user = this.authService.getUserInfo();
    if (!user) {
      this.router.navigate(['/login']);
      return;
    }
    this.role.set(user.role);
    this.userId.set(user.userId);

    this.route.paramMap.subscribe(params => {
      const idStr = params.get('id');
      if (idStr) {
        this.complaintId = Number(idStr);
        this.loadDetails();
        this.loadTracking();
      }
    });
  }

  loadDetails() {
    this.loading.set(true);
    this.complaintService.getComplaintDetails(this.complaintId).subscribe({
      next: (data) => {
        this.complaint.set(data);
        this.loading.set(false);

        // Load employees from same department for admin assignment if category is available
        if (this.role() === 'Admin') {
          // Map category name/id to load department employees
          this.loadDepartmentEmployees();
        }
      },
      error: (err) => {
        console.error(err);
        this.error.set('Failed to load complaint details.');
        this.loading.set(false);
      }
    });
  }

  loadTracking() {
    this.complaintService.getComplaintTracking(this.complaintId).subscribe({
      next: (data) => {
        this.trackingLogs.set(data);
      },
      error: (err) => console.error('Failed to load tracking logs', err)
    });
  }

  loadDepartmentEmployees() {
    const details = this.complaint();
    if (details) {
      this.adminService.getEmployeesByDepartment(details.categoryId).subscribe({
        next: (data) => {
          this.employees.set(data);
        },
        error: (err) => console.error('Failed to load department employees', err)
      });
    }
  }

  submitComment() {
    const text = this.newCommentText().trim();
    if (!text) return;

    this.submittingAction.set(true);
    this.complaintService.addComment(this.complaintId, { commentText: text }).subscribe({
      next: (newComment) => {
        this.newCommentText.set('');
        this.submittingAction.set(false);
        this.loadDetails(); // Reload to fetch new comments list
      },
      error: (err) => {
        this.toastService.error('Failed to submit comment. ' + (err.error?.message || ''));
        this.submittingAction.set(false);
      }
    });
  }

  downloadAttachment(attachmentId: number, fileName: string) {
    this.complaintService.downloadAttachment(this.complaintId, attachmentId).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = fileName;
        document.body.appendChild(a);
        a.click();
        window.URL.revokeObjectURL(url);
        a.remove();
      },
      error: (err) => this.toastService.error('Failed to download attachment.')
    });
  }

  // Customer Actions
  closeComplaint() {
    this.submittingAction.set(true);
    this.complaintService.updateStatus(this.complaintId, 6).subscribe({ // 6 is Closed
      next: () => {
        this.submittingAction.set(false);
        this.loadDetails();
        this.loadTracking();
      },
      error: (err) => {
        this.toastService.error('Failed to close complaint.');
        this.submittingAction.set(false);
      }
    });
  }

  reopenComplaint() {
    this.submittingAction.set(true);
    this.complaintService.updateStatus(this.complaintId, 7).subscribe({ // 7 is Reopened
      next: () => {
        this.submittingAction.set(false);
        this.loadDetails();
        this.loadTracking();
      },
      error: (err) => {
        this.toastService.error('Failed to reopen complaint.');
        this.submittingAction.set(false);
      }
    });
  }

  // Employee Actions
  startWork() {
    this.submittingAction.set(true);
    this.complaintService.updateStatus(this.complaintId, 3).subscribe({ // 3 is InProgress
      next: () => {
        this.submittingAction.set(false);
        this.loadDetails();
        this.loadTracking();
      },
      error: (err) => {
        this.toastService.error('Failed to update status.');
        this.submittingAction.set(false);
      }
    });
  }

  resolveComplaint() {
    this.submittingAction.set(true);
    this.complaintService.updateStatus(this.complaintId, 5).subscribe({ // 5 is Resolved
      next: () => {
        this.submittingAction.set(false);
        this.loadDetails();
        this.loadTracking();
      },
      error: (err) => {
        this.toastService.error('Failed to resolve complaint.');
        this.submittingAction.set(false);
      }
    });
  }

  submitEscalation() {
    const reason = this.escalationReason().trim();
    if (!reason) {
      this.toastService.error('Please enter a reason for escalation.');
      return;
    }

    this.submittingAction.set(true);
    // Find next escalation level
    // TeamLead is level 1, Manager level 2, SeniorManager level 3.
    // For simplicity, TeamLead = 1.
    this.escalationService.createEscalation({
      complaintId: this.complaintId,
      escalationLevel: 1, // Will auto-escalate to next valid expected level on backend sequence checking anyway!
      reason: reason
    }).subscribe({
      next: () => {
        this.submittingAction.set(false);
        this.showEscalationModal.set(false);
        this.escalationReason.set('');
        this.loadDetails();
        this.loadTracking();
        this.toastService.success('Complaint escalated successfully.');
      },
      error: (err) => {
        this.toastService.error('Failed to escalate complaint. ' + (err.error?.message || ''));
        this.submittingAction.set(false);
      }
    });
  }

  // Admin Actions
  assignPriority() {
    this.submittingAction.set(true);
    this.complaintService.assignPriority(this.complaintId, Number(this.selectedPriority())).subscribe({
      next: () => {
        this.submittingAction.set(false);
        this.loadDetails();
        this.loadTracking();
        this.toastService.success('Priority assigned successfully.');
      },
      error: (err) => {
        this.toastService.error('Failed to assign priority.');
        this.submittingAction.set(false);
      }
    });
  }

  autoAssignEmployee() {
    this.submittingAction.set(true);
    this.complaintService.assignComplaint(this.complaintId).subscribe({
      next: (res) => {
        this.submittingAction.set(false);
        if (res.isAssigned) {
          this.loadDetails();
          this.loadTracking();
          this.toastService.success(res.message);
        } else if (res.employees && res.employees.length > 0) {
          this.toastService.warning('Multiple least loaded employees found. Please assign one manually.');
        } else {
          this.toastService.info(res.message);
        }
      },
      error: (err) => {
        this.toastService.error('Failed to auto-assign: ' + (err.error?.message || ''));
        this.submittingAction.set(false);
      }
    });
  }

  manualAssignEmployee() {
    const empId = Number(this.selectedEmployeeId());
    if (empId === 0) {
      this.toastService.error('Please select an employee.');
      return;
    }
    this.submittingAction.set(true);
    this.complaintService.assignComplaintToEmployee(this.complaintId, empId).subscribe({
      next: () => {
        this.submittingAction.set(false);
        this.loadDetails();
        this.loadTracking();
        this.toastService.success('Employee assigned successfully.');
      },
      error: (err) => {
        this.toastService.error('Failed to assign employee. ' + (err.error?.message || ''));
        this.submittingAction.set(false);
      }
    });
  }

  back() {
    if (this.role() === 'Admin') this.router.navigate(['/admin/complaints']);
    else if (this.role() === 'Employee') this.router.navigate(['/employee/complaints']);
    else this.router.navigate(['/complaints']);
  }
}
