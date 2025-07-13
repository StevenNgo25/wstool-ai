import { Injectable } from "@angular/core"
import { HttpClient, HttpParams } from "@angular/common/http"
import { Observable } from "rxjs"
import { environment } from "../../environments/environment"
import { PaginatedResponse, PaginationRequest } from "../models/common.model"
import { Group, DashboardStats } from "../models/message.model" // Corrected import for Group

@Injectable({
  providedIn: "root",
})
export class GroupService {
  private apiUrl = `${environment.apiUrl}/groups`

  constructor(private http: HttpClient) {}

  getGroups(pagination: PaginationRequest): Observable<PaginatedResponse<Group>> {
    let params = new HttpParams()
      .set("page", pagination.page.toString())
      .set("pageSize", pagination.pageSize.toString())
      .set("sortBy", pagination.sortBy ?? '')
      .set("sortDirection", pagination.sortDirection ?? '')

    if (pagination.search) {
      params = params.set("search", pagination.search)
    }

    return this.http.get<PaginatedResponse<Group>>(this.apiUrl, { params })
  }
  

  searchGroups(pagination: PaginationRequest, instanceIds: number[]): Observable<PaginatedResponse<Group>> {
    const body = {
      page: pagination.page,
      pageSize: pagination.pageSize,
      sortBy: pagination.sortBy ?? '',
      sortDirection: pagination.sortDirection ?? '',
      search: pagination.search ?? '',
      instanceIds: instanceIds
    };
  
    return this.http.post<PaginatedResponse<Group>>(this.apiUrl + "/search", body);
  }

  getDashboardStats(): Observable<DashboardStats> {
    return this.http.get<DashboardStats>(`${environment.apiUrl}/dashboard/stats`)
  }
}
