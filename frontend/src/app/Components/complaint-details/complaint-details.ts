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
import { EscalationItem, EligibleEmployee } from '../../models/escalation.model';
import { AIService } from '../../Services/ai.service';
import { AICategoryValidation, AIPriorityRecommendation, AISimilarComplaint } from '../../models/ai.model';
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

  complaint = signal<ComplaintDetails | null>(null);
  trackingLogs = signal<{ action: string; description: string; performedBy: string; createdAt: string }[]>([]);
  employees = signal<{ employeeId: number; employeeName: string; designation: string }[]>([]);

  complaintEscalations = signal<EscalationItem[]>([]);
  latestEscalationStatus = signal<string | null>(null);
  nextEscalationLevel = signal<number>(1);
  nextEscalationLabel = signal('Team Lead');
  hasPendingEscalation = signal(false);
  isMaxLevelReached = signal(false);

  newCommentText = signal('');
  selectedPriority = signal<number>(4);
  selectedEmployeeId = signal<number>(0);
  escalationReason = signal('');

  showEscalationModal = signal(false);
  submittingAction = signal(false);

  pendingEscalation = signal<EscalationItem | null>(null);
  showResolveModal = signal(false);
  resolveAction = signal<'Accept' | 'Reject'>('Accept');
  resolveComments = signal('');
  resolveEmployeeId = signal(0);
  eligibleEmployees = signal<EligibleEmployee[]>([]);
  loadingEligibleEmployees = signal(false);

  private aiService = inject(AIService);

  aiSummary = signal<string | null>(null);
  loadingSummary = signal(false);

  similarComplaints = signal<AISimilarComplaint[]>([]);
  loadingSimilar = signal(false);

  aiCategoryVal = signal<AICategoryValidation | null>(null);
  loadingCategoryVal = signal(false);

  aiPriorityRec = signal<AIPriorityRecommendation | null>(null);
  loadingPriorityRec = signal(false);

  currentCategoryId = signal<number>(0);
  categories: { id: number; name: string }[] = [];

  ngOnInit() {
    const user = this.authService.getUserInfo();
    if (!user) {
      this.router.navigate(['/login']);
      return;
    }
    this.role.set(user.role);
    this.userId.set(user.userId);
    this.loadCategories();

    this.route.paramMap.subscribe(params => {
      const idStr = params.get('id');
      if (idStr) {
        this.complaintId = Number(idStr);
        this.loadDetails();
        this.loadTracking();
        if (this.role() === 'Admin' || this.role() === 'Employee') {
          this.loadComplaintEscalations();
        }
      }
    });
  }

  loadCategories() {
    this.complaintService.getCategories().subscribe({
      next: (data) => {
        this.categories = data.map(c => ({ id: c.categoryId, name: c.categoryName }));
      },
      error: (err) => {
        console.error('Failed to load categories', err);
      }
    });
  }

  loadDetails() {
    this.loading.set(true);
    this.complaintService.getComplaintDetails(this.complaintId).subscribe({
      next: (data) => {
        this.complaint.set(data);
        this.currentCategoryId.set(data.categoryId);
        this.loading.set(false);
        if (this.role() === 'Admin') {
          this.loadDepartmentEmployees();
        }

        // Fetch AI suggestions
        this.loadAISummary();
        if (this.role() === 'Admin') {
          this.loadAICategoryValidation();
          this.loadAIPriorityRecommendation();
        }
        if (this.role() === 'Admin' || this.role() === 'Employee') {
          this.loadAISimilarComplaints();
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

  loadAISummary() {
    this.loadingSummary.set(true);
    this.aiSummary.set(null);
    this.aiService.getComplaintSummary(this.complaintId).subscribe({
      next: (res) => {
        this.aiSummary.set(res.summary);
        this.loadingSummary.set(false);
      },
      error: (err) => {
        console.error('Failed to load AI summary', err);
        this.loadingSummary.set(false);
      }
    });
  }

  loadAICategoryValidation() {
    this.loadingCategoryVal.set(true);
    this.aiCategoryVal.set(null);
    this.aiService.getCategoryValidation(this.complaintId).subscribe({
      next: (res) => {
        this.aiCategoryVal.set(res);
        this.loadingCategoryVal.set(false);
      },
      error: (err) => {
        console.error('Failed to load AI category validation', err);
        this.loadingCategoryVal.set(false);
      }
    });
  }

  loadAIPriorityRecommendation() {
    this.loadingPriorityRec.set(true);
    this.aiPriorityRec.set(null);
    this.aiService.getPriorityRecommendation(this.complaintId).subscribe({
      next: (res) => {
        this.aiPriorityRec.set(res);
        this.loadingPriorityRec.set(false);
      },
      error: (err) => {
        console.error('Failed to load AI priority recommendation', err);
        this.loadingPriorityRec.set(false);
      }
    });
  }

  loadAISimilarComplaints() {
    this.loadingSimilar.set(true);
    this.similarComplaints.set([]);
    this.aiService.getSimilarComplaints(this.complaintId).subscribe({
      next: (res) => {
        this.similarComplaints.set(res);
        this.loadingSimilar.set(false);
      },
      error: (err) => {
        console.error('Failed to load similar complaints', err);
        this.loadingSimilar.set(false);
      }
    });
  }

  applyAIRecommendedPriority(priorityName: string) {
    const name = priorityName.trim().toLowerCase();
    if (name === 'critical') this.selectedPriority.set(1);
    else if (name === 'high') this.selectedPriority.set(2);
    else if (name === 'medium') this.selectedPriority.set(3);
    else if (name === 'low') this.selectedPriority.set(4);
    this.toastService.info(`Applied recommended priority: ${priorityName}`);
  }

  updateComplaintCategory(categoryId: number) {
    this.complaintService.updateCategory(this.complaintId, categoryId).subscribe({
      next: (res) => {
        this.toastService.success('Category updated successfully!');
        this.loadDetails();
      },
      error: (err) => {
      }
    });
  }

  viewSimilarTicket(id: number) {
    window.open(`/complaints/${id}`, '_blank');
  }

  loadComplaintEscalations() {
    this.escalationService.getComplaintEscalations(this.complaintId).subscribe({
      next: (data) => {
        this.complaintEscalations.set(data);
        const pending = data.find(e => e.status === 'Pending');
        this.pendingEscalation.set(pending || null);
        if (data.length > 0) {
          this.latestEscalationStatus.set(data[0].status);
          this.hasPendingEscalation.set(!!pending);
          const levelMap: Record<string, number> = { 'Team Lead': 1, 'Department Manager': 2, 'Senior Manager': 3 };
          const latestLevel = levelMap[data[0].escalationLevel] || data[0].escalatedLevelId || 1;
          this.nextEscalationLevel.set(latestLevel + 1);
          this.isMaxLevelReached.set(latestLevel >= 3);
          const labelMap: Record<number, string> = { 1: 'Team Lead', 2: 'Department Manager', 3: 'Senior Manager' };
          this.nextEscalationLabel.set(labelMap[latestLevel + 1] || 'Senior Manager');
        } else {
          this.latestEscalationStatus.set(null);
          this.hasPendingEscalation.set(false);
          this.nextEscalationLevel.set(1);
          this.nextEscalationLabel.set('Team Lead');
          this.isMaxLevelReached.set(false);
        }
      },
      error: (err) => console.error('Failed to load escalations', err)
    });
  }

  get canEscalate(): boolean {
    if (this.role() !== 'Employee') return false;
    const details = this.complaint();
    if (!details) return false;
    const userInfo = this.authService.getUserInfo();
    if (!userInfo || details.assignedEmployeeId !== userInfo.employeeId) return false;
    const status = details.status;
    if (status !== 'InProgress' && status !== 'Reopened') return false;
    if (this.hasPendingEscalation()) return false;
    if (this.isMaxLevelReached()) return false;
    return true;
  }

  submitComment() {
    const text = this.newCommentText().trim();
    if (!text) return;

    this.submittingAction.set(true);
    this.complaintService.addComment(this.complaintId, { commentText: text }).subscribe({
      next: () => {
        this.newCommentText.set('');
        this.submittingAction.set(false);
        this.loadDetails();
      },
      error: (err) => {
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
      error: () => this.toastService.error('Failed to download attachment.')
    });
  }

  closeComplaint() {
    this.submittingAction.set(true);
    this.complaintService.updateStatus(this.complaintId, 6).subscribe({
      next: () => {
        this.submittingAction.set(false);
        this.loadDetails();
        this.loadTracking();
      },
      error: () => {
        this.submittingAction.set(false);
      }
    });
  }

  reopenComplaint() {
    this.submittingAction.set(true);
    this.complaintService.updateStatus(this.complaintId, 7).subscribe({
      next: () => {
        this.submittingAction.set(false);
        this.loadDetails();
        this.loadTracking();
      },
      error: () => {
        this.submittingAction.set(false);
      }
    });
  }

  startWork() {
    this.submittingAction.set(true);
    this.complaintService.updateStatus(this.complaintId, 3).subscribe({
      next: () => {
        this.submittingAction.set(false);
        this.loadDetails();
        this.loadTracking();
      },
      error: () => {
        this.submittingAction.set(false);
      }
    });
  }

  resolveComplaint() {
    this.submittingAction.set(true);
    this.complaintService.updateStatus(this.complaintId, 5).subscribe({
      next: () => {
        this.submittingAction.set(false);
        this.loadDetails();
        this.loadTracking();
      },
      error: () => {
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
    this.escalationService.createEscalation({
      complaintId: this.complaintId,
      reason: reason
    }).subscribe({
      next: () => {
        this.submittingAction.set(false);
        this.showEscalationModal.set(false);
        this.escalationReason.set('');
        this.loadDetails();
        this.loadTracking();
        this.loadComplaintEscalations();
        this.toastService.success('Escalation request submitted. Waiting for admin review.');
      },
      error: (err) => {
        this.submittingAction.set(false);
      }
    });
  }

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
        this.submittingAction.set(false);
      }
    });
  }

  openResolveEscalationModal(action: 'Accept' | 'Reject') {
    const esc = this.pendingEscalation();
    if (!esc) return;
    this.resolveAction.set(action);
    this.resolveComments.set('');
    this.resolveEmployeeId.set(0);
    this.eligibleEmployees.set([]);
    this.showResolveModal.set(true);

    this.loadingEligibleEmployees.set(true);
    this.escalationService.getEligibleEmployees(esc.escalatedId, action).subscribe({
      next: (employees) => {
        this.eligibleEmployees.set(employees);
        this.loadingEligibleEmployees.set(false);
      },
      error: () => {
        this.loadingEligibleEmployees.set(false);
      }
    });
  }

  submitResolveAction() {
    const esc = this.pendingEscalation();
    if (!esc) return;

    const comments = this.resolveComments().trim();
    if (!comments) {
      this.toastService.error('Please provide comments (mandatory).');
      return;
    }

    const employeeId = this.resolveEmployeeId();
    if (employeeId === 0) {
      this.toastService.error('Please select a target employee.');
      return;
    }

    this.submittingAction.set(true);
    this.escalationService.resolveEscalation(esc.escalatedId, {
      action: this.resolveAction(),
      employeeId: employeeId,
      comments: comments
    }).subscribe({
      next: () => {
        this.submittingAction.set(false);
        this.showResolveModal.set(false);
        this.loadDetails();
        this.loadTracking();
        this.loadComplaintEscalations();
        this.toastService.success(`Escalation ${this.resolveAction().toLowerCase()}ed successfully.`);
      },
      error: (err) => {
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
