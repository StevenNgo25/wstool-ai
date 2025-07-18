import { Component, OnInit } from "@angular/core"
import { CommonModule } from "@angular/common" // Import CommonModule
import { LoadingComponent } from "../shared/components/loading/loading.component" // Import LoadingComponent

import { ChartConfiguration, ChartData, ChartType } from "chart.js"
import { GroupService } from "../services/group.service"
import { DashboardStats } from "../models/message.model"
import { NgChartsModule } from 'ng2-charts';
import { TranslateModule } from "@ngx-translate/core"

@Component({
  selector: "app-dashboard",
  standalone: true, // Đánh dấu là standalone component
  imports: [CommonModule, LoadingComponent, NgChartsModule, TranslateModule], // Import các module cần thiết
  templateUrl: "./dashboard.component.html",
  styleUrls: ["./dashboard.component.scss"],
})
export class DashboardComponent implements OnInit {
  stats: DashboardStats | null = null
  loading = true

  jobChartType: ChartType = "doughnut"
  jobChartData: ChartData<"doughnut"> = {
    labels: ["Pending", "Completed", "Failed"],
    datasets: [
      {
        data: [0, 0, 0],
        backgroundColor: ["#f6c23e", "#1cc88a", "#e74a3b"],
      },
    ],
  }
  jobChartOptions: ChartConfiguration["options"] = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        position: "bottom",
      },
    },
  }

  constructor(private groupService: GroupService) {}

  ngOnInit(): void {
    this.loadDashboardStats()
  }

  loadDashboardStats(): void {
    this.loading = true
    this.groupService.getDashboardStats().subscribe({
      next: (stats) => {
        this.stats = stats
        this.updateChartData()
        this.loading = false
      },
      error: (error) => {
        console.error("Error loading dashboard stats:", error)
        this.loading = false
      },
    })
  }

  updateChartData(): void {
    if (this.stats) {
      const failedJobs = this.stats.totalJobs - this.stats.completedJobs - this.stats.pendingJobs
      this.jobChartData = {
        labels: ["Pending", "Completed", "Failed"],
        datasets: [
          {
            data: [this.stats.pendingJobs, this.stats.completedJobs, failedJobs],
            backgroundColor: ["#f6c23e", "#1cc88a", "#e74a3b"],
          },
        ],
      }
    }
  }

  getSuccessRate(): number {
    if (!this.stats || this.stats.totalJobs === 0) return 0
    return Math.round((this.stats.completedJobs / this.stats.totalJobs) * 100)
  }
}
