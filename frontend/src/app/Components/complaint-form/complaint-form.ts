import { Component, signal, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Header } from '../Header/header';
import { ComplaintService } from '../../Services/complaint.service';
import { AuthService } from '../../Services/auth.service';
import { ToastService } from '../../Services/toast.service';
import { NgIf, NgFor, NgClass, DecimalPipe } from '@angular/common';

@Component({
  selector: 'app-complaint-form',
  imports: [Header, FormsModule, NgIf, NgFor, NgClass, DecimalPipe],
  templateUrl: './complaint-form.html',
  styleUrl: './complaint-form.css',
})
export class ComplaintForm implements OnInit {
  private complaintService = inject(ComplaintService);
  private authService = inject(AuthService);
  private router = inject(Router);
  private toastService = inject(ToastService);

  title = signal('');
  description = signal('');
  category = signal(1); // Default to 1
  categories = signal<{ categoryId: number; categoryName: string }[]>([]);
  submitting = signal(false);
  
  // File Uploader state
  selectedFiles = signal<File[]>([]);
  isDragging = signal(false);

  constructor() {
    if (!this.authService.isLoggedIn()) {
      this.router.navigate(['/login']);
    }
  }

  ngOnInit() {
    this.complaintService.getCategories().subscribe({
      next: (data) => {
        this.categories.set(data);
        if (data.length > 0) {
          this.category.set(data[0].categoryId);
        }
      },
      error: (err) => {
        console.error('Failed to load categories', err);
      }
    });
  }

  onFileSelected(event: any) {
    if (event.target.files) {
      const files: FileList = event.target.files;
      this.addFiles(files);
    }
  }

  addFiles(files: FileList) {
    const current = this.selectedFiles();
    const updated = [...current];
    
    for (let i = 0; i < files.length; i++) {
      const file = files[i];
      // Limit file sizes to 10MB
      if (file.size > 10 * 1024 * 1024) {
        this.toastService.error(`File ${file.name} is too large (max 10MB).`);
        continue;
      }
      // Avoid duplicates
      if (!updated.some(f => f.name === file.name && f.size === file.size)) {
        updated.push(file);
      }
    }
    this.selectedFiles.set(updated);
  }

  removeFile(index: number) {
    const current = this.selectedFiles();
    const updated = current.filter((_, i) => i !== index);
    this.selectedFiles.set(updated);
  }

  // Drag & Drop event handlers
  onDragOver(event: DragEvent) {
    event.preventDefault();
    this.isDragging.set(true);
  }

  onDragLeave(event: DragEvent) {
    event.preventDefault();
    this.isDragging.set(false);
  }

  onDrop(event: DragEvent) {
    event.preventDefault();
    this.isDragging.set(false);
    if (event.dataTransfer?.files) {
      this.addFiles(event.dataTransfer.files);
    }
  }

  submit() {
    if (!this.title().trim() || !this.description().trim()) {
      this.toastService.error('Title and description are required.');
      return;
    }
    
    this.submitting.set(true);
    
    this.complaintService.createComplaint(
      this.title(),
      this.description(),
      Number(this.category()),
      this.selectedFiles()
    ).subscribe({
      next: () => {
        this.submitting.set(false);
        this.toastService.success('Complaint submitted successfully!');
        this.router.navigate(['/complaints']);
      },
      error: (err) => {
        console.error(err);
        this.submitting.set(false);
      }
    });
  }

  cancel() {
    this.router.navigate(['/dashboard']);
  }
}
