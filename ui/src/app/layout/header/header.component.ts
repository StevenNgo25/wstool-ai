import { Component, OnInit } from "@angular/core"
import { CommonModule } from "@angular/common" // Import CommonModule
import { RouterModule } from "@angular/router" // Import RouterModule
import { NgbDropdownModule } from "@ng-bootstrap/ng-bootstrap" // Import NgbDropdownModule

import { Router } from "@angular/router"
import { AuthService } from "../../services/auth.service"
import { User } from "../../models/user.model"

@Component({
  selector: "app-header",
  standalone: true, // Đánh dấu là standalone component
  imports: [CommonModule, RouterModule, NgbDropdownModule], // Import các module cần thiết
  templateUrl: "./header.component.html",
})
export class HeaderComponent implements OnInit {
  currentUser: User | null = null
  isAdmin = false

  constructor(
    private authService: AuthService,
    public router: Router,
  ) {}

  ngOnInit(): void {
    this.authService.currentUser$.subscribe((user) => {
      this.currentUser = user
      this.isAdmin = this.authService.isAdmin()
    })
  }

  logout(event: Event): void {
    event.preventDefault()
    this.authService.logout()
  }
}
