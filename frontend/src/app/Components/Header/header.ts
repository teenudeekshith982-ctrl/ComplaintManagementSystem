import { Component, signal, inject, computed } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../Services/auth.service';
import { NotificationService } from '../../Services/notification.service';
import { NgIf, NgFor, NgClass, DatePipe } from '@angular/common';
import { ToastService } from '../../Services/toast.service';

@Component({
  selector: 'app-header',
  imports: [RouterLink, RouterLinkActive, NgIf, NgFor, NgClass, DatePipe],
  templateUrl: './header.html',
  styleUrl: './header.css',
})
export class Header {
  private authService = inject(AuthService);
  private notificationService = inject(NotificationService);
  private toastService = inject(ToastService);
  private router = inject(Router);

  userName = signal('');
  role = signal('');
  navLinks = signal<{ path: string; label: string }[]>([]);

  // Notifications State UI Toggles
  showNotifDropdown = signal(false);
  isMenuOpen = signal(false);

  toggleMenu() {
    this.isMenuOpen.update(v => !v);
  }

  unreadCount = computed(() => this.notificationService.unreadCount());
  notifications = computed(() => this.notificationService.notifications());
  activeToast = computed(() => this.notificationService.activeToast());

  constructor() {
    const user = this.authService.getUserInfo();
    if (user) {
      this.role.set(user.role);
      this.userName.set(user.name);
      this.setupNavLinks(user.role);
      
      // Start Real-Time SignalR WebSockets and fetch initial notifications
      this.notificationService.startHubConnection();
      this.notificationService.loadInitialNotifications();
    }
  }

  setupNavLinks(role: string) {
    if (role === 'Admin') {
      this.navLinks.set([
        { path: '/admin', label: 'Dashboard' },
        { path: '/admin/complaints', label: 'Complaints' },
        { path: '/admin/employees', label: 'Employees' },
        { path: '/admin/escalations', label: 'Escalations' },
        { path: '/admin/reports', label: 'Reports' },
        { path: '/profile', label: 'Profile' }
      ]);
    } else if (role === 'Employee') {
      this.navLinks.set([
        { path: '/employee', label: 'Dashboard' },
        { path: '/employee/complaints', label: 'Assigned Complaints' },
        { path: '/profile', label: 'Profile' }
      ]);
    } else {
      this.navLinks.set([
        { path: '/dashboard', label: 'Dashboard' },
        { path: '/complaints', label: 'My Complaints' },
        { path: '/complaints/new', label: 'File Complaint' },
        { path: '/profile', label: 'Profile' }
      ]);
    }
  }

  toggleNotifDropdown() {
    this.showNotifDropdown.update(v => !v);
  }

  markRead(notificationId: number, relatedComplaintId?: number) {
    this.notificationService.markAsRead(notificationId).subscribe({
      next: () => {
        this.notificationService.loadInitialNotifications();
        
        if (relatedComplaintId) {
          this.showNotifDropdown.set(false);
          this.router.navigate(['/complaints', relatedComplaintId]);
        }
      },
      error: (err) => console.error(err)
    });
  }

  markAllRead() {
    this.notificationService.markAllAsRead().subscribe({
      next: () => {
        this.notificationService.loadInitialNotifications();
      },
      error: (err) => console.error(err)
    });
  }

  dismissToast() {
    this.notificationService.activeToast.set(null);
  }

  navigateToComplaint(id: number) {
    this.dismissToast();
    this.router.navigate(['/complaints', id]);
  }

  logout() {
    this.notificationService.stopHubConnection();
    this.toastService.success("Logged out Successfully");
    this.authService.logout();
  }
}
