import { Component, OnInit } from "@angular/core"
import { Router, NavigationEnd, RouterModule } from "@angular/router"
import { CommonModule } from "@angular/common"
import { ToastaModule } from "ngx-toasta"

import { AuthService } from "./services/auth.service"
import { HeaderComponent } from "./layout/header/header.component"
import { SidebarComponent } from "./layout/sidebar/sidebar.component"

@Component({
  selector: "app-root",
  standalone: true, // Đánh dấu là standalone component
  imports: [
    CommonModule,
    RouterModule,
    HeaderComponent,
    SidebarComponent,
    ToastaModule, // Import ToastaModule trực tiếp
  ],
  templateUrl: "./app.component.html",
  styleUrls: ["./app.component.scss"],
})
export class AppComponent implements OnInit {
  showLayout = true

  constructor(
    public router: Router,
    private authService: AuthService,
  ) {}

  ngOnInit(): void {
    this.router.events.subscribe((event) => {
      if (event instanceof NavigationEnd) {
        this.showLayout = !event.url.includes("/login")
      }
    })
  }
}
