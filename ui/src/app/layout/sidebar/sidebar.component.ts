import { Component, OnInit } from "@angular/core"
import { CommonModule } from "@angular/common" // Import CommonModule
import { RouterModule } from "@angular/router" // Import RouterModule

import { AuthService } from "../../services/auth.service"
import { TranslateModule } from "@ngx-translate/core"

@Component({
  selector: "app-sidebar",
  standalone: true, // Đánh dấu là standalone component
  imports: [CommonModule, RouterModule, TranslateModule], // Import các module cần thiết
  templateUrl: "./sidebar.component.html",
  styleUrls: ["./sidebar.component.scss"],
})
export class SidebarComponent implements OnInit {
  isAdmin = false

  constructor(private authService: AuthService) {}

  ngOnInit(): void {
    this.isAdmin = this.authService.isAdmin()
  }
}
