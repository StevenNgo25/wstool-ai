import { Component, OnInit } from "@angular/core"
import { CommonModule } from "@angular/common" // Import CommonModule
import { RouterModule } from "@angular/router" // Import RouterModule
import { NgbDropdownModule } from "@ng-bootstrap/ng-bootstrap" // Import NgbDropdownModule

import { Router } from "@angular/router"
import { AuthService } from "../../services/auth.service"
import { User } from "../../models/user.model"
import { TranslateModule, TranslateService } from "@ngx-translate/core"

@Component({
  selector: "app-header",
  standalone: true, // Đánh dấu là standalone component
  imports: [CommonModule, RouterModule, NgbDropdownModule, TranslateModule], // Import các module cần thiết
  templateUrl: "./header.component.html",
})
export class HeaderComponent implements OnInit {
  currentUser: User | null = null
  isAdmin = false
  selectedLang = 'en';

  constructor(
    private authService: AuthService,
    public router: Router,
    private translate: TranslateService,
  ) {
    const savedLang = localStorage.getItem('lang') || 'en';
    this.selectedLang = savedLang;
    this.translate.use(savedLang);
  }

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

  changeLang(lang: string) {
    this.selectedLang = lang;
    this.translate.use(lang);
    localStorage.setItem('lang', lang);
  }
}
