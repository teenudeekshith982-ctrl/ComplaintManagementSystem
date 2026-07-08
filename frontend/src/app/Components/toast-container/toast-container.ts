import { Component, inject } from '@angular/core';
import { NgClass, NgFor, NgIf } from '@angular/common';
import { ToastService } from '../../Services/toast.service';

@Component({
  selector: 'app-toast-container',
  imports: [NgFor, NgIf, NgClass],
  templateUrl: './toast-container.html',
  styleUrl: './toast-container.css'
})
export class ToastContainer {
  toastService = inject(ToastService);
  toasts = this.toastService.toasts;

  remove(id: number) {
    this.toastService.remove(id);
  }
}
