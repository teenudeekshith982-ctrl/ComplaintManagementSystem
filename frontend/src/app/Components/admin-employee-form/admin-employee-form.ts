import { Component, signal, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Header } from '../Header/header';
import { AdminService } from '../../Services/admin.service';
import { AuthService } from '../../Services/auth.service';
import { ComplaintService } from '../../Services/complaint.service';
import { NgIf, NgFor, NgClass } from '@angular/common';
import { ToastService } from '../../Services/toast.service';

@Component({
  selector: 'app-admin-employee-form',
  imports: [Header, FormsModule, NgIf, NgFor, NgClass],
  templateUrl: './admin-employee-form.html',
  styleUrl: './admin-employee-form.css',
})
export class AdminEmployeeForm implements OnInit {
  private adminService = inject(AdminService);
  private complaintService = inject(ComplaintService);
  private authService = inject(AuthService);
  private toastService = inject(ToastService);
  private router = inject(Router);

  name = signal('');
  email = signal('');
  password = signal('');
  phone = signal('');
  designation = signal(1);
  department = signal(1);
  departments = signal<{ departmentId: number; departmentName: string }[]>([]);
  designations = signal<{ designationId: number; designationName: string }[]>([]);
  submitting = signal(false);
  submitError = signal<string | null>(null);
  successMessage = signal<string | null>(null);

  constructor() {
    if (!this.authService.isLoggedIn()) {
      this.router.navigate(['/login']);
    }
  }

  ngOnInit() {
    this.complaintService.getDepartments().subscribe({
      next: (data) => {
        this.departments.set(data);
        if (data.length > 0) {
          this.department.set(data[0].departmentId);
        }
      },
      error: (err) => console.error('Failed to load departments', err)
    });

    this.complaintService.getDesignations().subscribe({
      next: (data) => {
        this.designations.set(data);
        if (data.length > 0) {
          this.designation.set(data[0].designationId);
        }
      },
      error: (err) => console.error('Failed to load designations', err)
    });
  }

  submit() {
    if (!this.name().trim() || !this.email().trim() || !this.password().trim()) {
      this.submitError.set('Name, email, and password are required.');
      return;
    }
    this.submitting.set(true);
    this.submitError.set(null);
    this.successMessage.set(null);
    this.adminService.createEmployee({
      name: this.name(),
      email: this.email(),
      password: this.password(),
      phone: this.phone(),
      role: 'Employee',
      designation: this.designation(),
      department: this.department()
    }).subscribe({
      next: (res) => {
        this.submitting.set(false);
        this.successMessage.set(`Employee "${res.name}" created successfully.`);
        this.toastService.success(`Employee "${res.name}" created successfully.`,1500);
        setTimeout(() => this.router.navigate(['/admin/employees']), 1500);
      },
      error: (err: any) => {
        this.submitting.set(false);
        this.submitError.set(err.error?.message || 'Failed to create employee. Please try again.');
      }
    });
  }

  cancel() {
    this.router.navigate(['/admin/employees']);
  }
}
