import { Component, signal, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../Services/auth.service';
import { ToastService } from '../../Services/toast.service';
import { NgIf, NgClass } from '@angular/common';
import { HttpErrorResponse, HttpResponse } from '@angular/common/http';

@Component({
  selector: 'app-login',
  imports: [RouterLink, FormsModule, NgClass],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login {
  private authService = inject(AuthService);
  private router = inject(Router);
  private toastService = inject(ToastService);

  email = '';
  password = '';
  loading = signal(false);

  Login() {
    if (!this.email || !this.password) return;

    const credentials = { email: this.email, password: this.password };
    this.loading.set(true);

    this.authService.Login(credentials).subscribe({
      next: (response) => {
        localStorage.setItem('token', response.token);
        this.loading.set(false);
        this.toastService.success('Logged in successfully!');

        const user = this.authService.getUserInfo();
        if (user?.role === 'Admin'){
          this.router.navigate(['/admin']);
        } 
        else if (user?.role === 'Employee') 
        {
          this.router.navigate(['/employee']);
        } 
        else {
          this.router.navigate(['/dashboard']);
        }
      },
      error: (error: HttpErrorResponse) => {
        this.loading.set(false);
      }
    });
  }
}
