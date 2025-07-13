import { Component, OnInit } from "@angular/core"
import { CommonModule } from "@angular/common" // Import CommonModule
import { FormsModule } from "@angular/forms" // Import FormsModule
import { PaginationComponent } from "../shared/components/pagination/pagination.component" // Import PaginationComponent
import { LoadingComponent } from "../shared/components/loading/loading.component" // Import LoadingComponent

import { GroupService } from "../services/group.service"
import { Group } from "../models/message.model" // Corrected import for Group
import { PaginationRequest, PaginatedResponse } from "../models/common.model"

@Component({
  selector: "app-groups",
  standalone: true, // Đánh dấu là standalone component
  imports: [CommonModule, FormsModule, PaginationComponent, LoadingComponent], // Import các module cần thiết
  templateUrl: "./groups.component.html",
  styleUrls: ["./groups.component.scss"],
})
export class GroupsComponent implements OnInit {
  groups: PaginatedResponse<Group> = { data: [], pagination: {} as any }
  loading = true
  searchTerm = ""

  paginationRequest: PaginationRequest = {
    page: 1,
    pageSize: 10,
    search: "",
    sortBy: "",
    sortDirection: "asc",
  }

  constructor(private groupService: GroupService) {}

  ngOnInit(): void {
    this.loadGroups()
  }

  loadGroups(): void {
    this.loading = true
    this.groupService.getGroups(this.paginationRequest).subscribe({
      next: (response) => {
        this.groups = response
        this.loading = false
      },
      error: (error) => {
        console.error("Error loading groups:", error)
        this.loading = false
      },
    })
  }

  search(): void {
    this.paginationRequest.search = this.searchTerm
    this.paginationRequest.page = 1
    this.loadGroups()
  }

  onPageChange(page: number): void {
    this.paginationRequest.page = page
    this.loadGroups()
  }

  onPageSizeChange(pageSize: number): void {
    this.paginationRequest.pageSize = pageSize
    this.paginationRequest.page = 1
    this.loadGroups()
  }
}
