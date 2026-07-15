import { Component, signal, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../Services/auth.service';
import { ToastService } from '../../Services/toast.service';
import { FormsModule } from '@angular/forms';
import { NgIf, NgClass } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
  selector: 'app-register',
  imports: [RouterLink, FormsModule, NgClass],
  templateUrl: './register.html',
  styleUrl: './register.css',
})
export class Register {
  private authService = inject(AuthService);
  private router = inject(Router);
  private toastService = inject(ToastService);

  fullName = '';
  email = '';
  phone = '';
  password = '';
  isloading = signal(false);

  Register() {
    if (!this.fullName || !this.email || !this.phone || !this.password) return;

    const credentials = {
      name: this.fullName,
      email: this.email,
      phone: this.phone,
      password: this.password
    };
    
    this.isloading.set(true);

    this.authService.Register(credentials).subscribe({
      next: (response) => {
        this.isloading.set(false);
        this.toastService.success('Registration successful! Redirecting to login...');
        setTimeout(() => {
          this.router.navigate(['/login']);
        }, 1500);
      },
      error: (error: HttpErrorResponse) => {
        this.isloading.set(false);
      }
    });
  }
}
