import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Register } from './Components/Register/register';
import { Login } from './Components/Login/login';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet,Login,Register],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('frontend');
}
