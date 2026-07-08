import { Component, signal, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Header } from '../Header/header';
import { AdminService } from '../../Services/admin.service';
import { AuthService } from '../../Services/auth.service';
import { NgIf, NgClass } from '@angular/common';

@Component({
  selector: 'app-admin-employee-form',
  imports: [Header, FormsModule, NgIf, NgClass],
  templateUrl: './admin-employee-form.html',
  styleUrl: './admin-employee-form.css',
})
export class AdminEmployeeForm {
  private adminService = inject(AdminService);
  private authService = inject(AuthService);
  private router = inject(Router);

  name = signal('');
  email = signal('');
  password = signal('');
  phone = signal('');
  designation = signal(1);
  department = signal(1);
  submitting = signal(false);
  submitError = signal<string | null>(null);
  successMessage = signal<string | null>(null);

  constructor() {
    if (!this.authService.isLoggedIn()) {
      this.router.navigate(['/login']);
    }
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
