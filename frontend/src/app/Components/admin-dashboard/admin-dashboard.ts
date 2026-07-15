import { Component, signal, computed, inject } from '@angular/core';
import { NgClass, NgFor, DatePipe, NgIf } from '@angular/common';
import { Router } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { Header } from '../Header/header';
import { DashboardService } from '../../Services/dashboard.service';
import { AuthService } from '../../Services/auth.service';
import { AdminDashboardData, ComplaintSummary } from '../../models/dashboard.model';
import { tap, catchError, of, map, finalize } from 'rxjs';


const EMPTY_DASHBOARD: AdminDashboardData = {
  totalComplaints: 0,
  openComplaints: 0,
  inProgressComplaints: 0,
  resolvedComplaints: 0,
  closedComplaints: 0,
  escalatedComplaints: 0,
  totalEmployees: 0
};


@Component({
  selector: 'app-admin-dashboard',
  imports: [Header, NgClass, NgIf, DatePipe],
  templateUrl: './admin-dashboard.html',
  styleUrl: './admin-dashboard.css',
})


export class AdminDashboard {
  
  constructor(private dashboardService:DashboardService, 
    private authService:AuthService, 
    private router:Router){

   this.loadDashboardData();
   this.loadRecentComplaints();
    if (!this.authService.isLoggedIn()) {
      this.router.navigate(['/login']);
      return;
    }
  }

  loading = signal(true);
  error = signal<string | null>(null);
  dashboardData = signal(EMPTY_DASHBOARD);
  recentComplaints = signal<ComplaintSummary[]>([]);

  loadDashboardData()
  {
    this.loading.set(true);
    this.dashboardService.getAdminDashboard()
    .pipe(
      finalize( () =>{
        this.loading.set(false);
      })
    )
    .subscribe({
      next : (data)=>{
        this.dashboardData.set(data);
        
      },
      error : (error)=>{
        this.error.set("Failed to load dashboard data");
        console.error(error);
        
      } 

    })
  }


  loadRecentComplaints(){
    this.loading.set(true);
    this.dashboardService.getAllComplaints(1, 5)
    .pipe(
      finalize( ()=>{
        this.loading.set(false);
      })
    )
    .subscribe({
      next : (data)=>{
        this.recentComplaints.set(data.complaints);
      },
      error : (error)=>{
        this.error.set("Failed to load recent complaints");
        console.error(error);
      }
    });
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



  viewAllComplaints() {
    this.router.navigate(['/admin/complaints']);
  }

  viewDetails(complaintId: number) {
    this.router.navigate(['/complaints', complaintId]);
  }

  manageEmployees() {
    this.router.navigate(['/admin/employees']);
  }

  manageEscalations() {
    this.router.navigate(['/admin/escalations']);
  }

  viewReports() {
    this.router.navigate(['/admin/reports']);
  }

  newEmployee() {
    this.router.navigate(['/admin/employees/new']);
  }

  goToProfile() {
    this.router.navigate(['/profile']);
  }
}
