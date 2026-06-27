import { Component, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../Services/auth.service';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-register',
  imports: [RouterLink,FormsModule],
  templateUrl: './register.html',
  styleUrl: './register.css',
})
export class Register {

  fullName : string = '';
  email : string = '';
  phone : string = '';
  password : string = '';
  isloading = signal(false);
  
  
  constructor(private authService: AuthService,private router : Router){
    this.isloading = this.authService.isLoading;

  }

  Register(){
    const credentials = {
      name : this.fullName,
      email : this.email,
      phone : this.phone,
      password : this.password
    }
    this.isloading.set(true);
    this.authService.Register(credentials).subscribe({
      next:(response)=>{
        console.log('Registration successful:', response);
        this.isloading.set(false);
        this.router.navigate(['/login']);
      },
      error:(error)=>{
        console.error('Registration failed:', error);
        this.isloading.set(false);
      }

    })
  }

}
