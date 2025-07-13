import { Injectable } from "@angular/core"
import { Observable } from "rxjs"
import { environment } from "../../environments/environment"
import { ApiService } from "./api.service"
import { PaginatedResponse, PaginationRequest } from "../models/common.model"
import { User, CreateUserDto, UpdateUserDto, AssignInstancesDto, Instance } from "../models/user.model"

@Injectable({
  providedIn: "root",
})
export class UserService {
  private apiUrl = `${environment.apiUrl}/users`

  constructor(private apiService: ApiService) {}

  getUsers(pagination: PaginationRequest): Observable<PaginatedResponse<User>> {
    let url = `${this.apiUrl}?page=${pagination.page}&pageSize=${pagination.pageSize}&sortBy=${pagination.sortBy}&sortDirection=${pagination.sortDirection}`
    if (pagination.search) {
      url += `&search=${pagination.search}`
    }
    return this.apiService.get<PaginatedResponse<User>>(url)
  }

  getUser(id: number): Observable<User> {
    return this.apiService.get<User>(`${this.apiUrl}/${id}`)
  }

  createUser(user: CreateUserDto): Observable<User> {
    return this.apiService.post<User>(this.apiUrl, user)
  }

  updateUser(id: number, user: UpdateUserDto): Observable<void> {
    return this.apiService.put<void>(`${this.apiUrl}/${id}`, user)
  }

  deleteUser(id: number): Observable<void> {
    return this.apiService.delete<void>(`${this.apiUrl}/${id}`)
  }

  assignInstances(userId: number, assignment: AssignInstancesDto): Observable<void> {
    return this.apiService.post<void>(`${this.apiUrl}/${userId}/assign-instances`, assignment)
  }

  getUserInstances(userId: number): Observable<Instance[]> {
    return this.apiService.get<Instance[]>(`${this.apiUrl}/${userId}/instances`)
  }
}
