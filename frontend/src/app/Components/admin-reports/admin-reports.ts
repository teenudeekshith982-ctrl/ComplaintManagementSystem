import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { Router } from '@angular/router';
import { Header } from '../Header/header';
import { AuthService } from '../../Services/auth.service';
import { AnalyticsService} from '../../Services/analytics.service';
import { NgFor, NgIf, NgClass, KeyValuePipe, DecimalPipe } from '@angular/common';
import { DashboardSummaryResponse } from '../../models/DashboardSummaryResponse.model';
import { ChartDataPoint } from '../../models/ChartDataPoint.model';

@Component({
  selector: 'app-admin-reports',
  imports: [Header, NgFor, NgIf, DecimalPipe],
  templateUrl: './admin-reports.html',
  styleUrl: './admin-reports.css',
})
export class AdminReports implements OnInit {
  private authService = inject(AuthService);
  private analyticsService = inject(AnalyticsService);
  private router = inject(Router);

  loading = signal(true);
  error = signal<string | null>(null);

  // States
  dashboardSummary = signal<DashboardSummaryResponse | null>(null);
  statusData = signal<ChartDataPoint[]>([]);
  categoryData = signal<ChartDataPoint[]>([]);
  trendData = signal<ChartDataPoint[]>([]);

  // Calculations
  maxStatusValue = computed(() => Math.max(...this.statusData().map(d => d.value), 1));
  maxCategoryValue = computed(() => Math.max(...this.categoryData().map(d => d.value), 1));
  maxTrendValue = computed(() => Math.max(...this.trendData().map(d => d.value), 1));

  ngOnInit() {
    if (!this.authService.isLoggedIn()) {
      this.router.navigate(['/login']);
      return;
    }
    this.loadData();
  }

  loadData() {
    this.loading.set(true);
    this.error.set(null);

    // Call summary endpoint
    this.analyticsService.getDashboardSummary().subscribe({
      next: (summary) => {
        this.dashboardSummary.set(summary);
        
        // Fetch distributions
        this.loadDistributions();
      },
      error: (err) => {
        console.error(err);
        this.error.set('Failed to load system reports.');
        this.loading.set(false);
      }
    });
  }

  loadDistributions() {
    // 1. By Status
    this.analyticsService.getComplaintsByStatus().subscribe({
      next: (res) => {
        const mapped = res.map(item => ({ name: item.status, value: item.count }));
        this.statusData.set(mapped);
      }
    });

    // 2. By Category
    this.analyticsService.getComplaintsByCategory().subscribe({
      next: (res) => {
        const mapped = res.map(item => ({ name: item.category, value: item.count }));
        this.categoryData.set(mapped);
      }
    });

    // 3. Monthly Trend
    this.analyticsService.getMonthlyTrend(6).subscribe({
      next: (res) => {
        const mapped = res.map(item => ({ name: item.monthName, value: item.submittedCount }));
        this.trendData.set(mapped);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  backToDashboard() {
    this.router.navigate(['/admin']);
  }
}
