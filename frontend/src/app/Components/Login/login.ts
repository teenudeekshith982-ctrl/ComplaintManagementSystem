import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../Services/auth.service';
import { LoginResponse } from '../../models/auth.model';

@Component({
  selector: 'app-login',
  imports: [RouterLink,FormsModule],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login {

    email : string ='';
    password : string ='';
    loading = signal(false);

    constructor(private authService : AuthService,private router : Router) {
      this.loading.set(this.authService.isLoading());
    }

    Login(){
      const credentials = { email: this.email, password: this.password };
      this.loading.set(true);
      this.authService.Login(credentials).subscribe({
        next: (response) => {
          console.log('Login successful', response);
          this.loading.set(false);
          this.router.navigate(['/dashboard']);
        },
        error: (error : any) => {
          console.error('Login failed', error);
          this.loading.set(false);
        }
      });
    }


}
