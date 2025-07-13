import { Injectable } from "@angular/core"
import { Observable } from "rxjs"
import { environment } from "../../environments/environment"
import { ApiService } from "./api.service"
import { PaginatedResponse, PaginationRequest } from "../models/common.model"
import { Job, CreateJobDto, BulkDeleteJobsDto } from "../models/job.model"

@Injectable({
  providedIn: "root",
})
export class JobService {
  private apiUrl = `${environment.apiUrl}/jobs`

  constructor(private apiService: ApiService) {}

  getJobs(pagination: PaginationRequest): Observable<PaginatedResponse<Job>> {
    let url = `${this.apiUrl}?page=${pagination.page}&pageSize=${pagination.pageSize}&sortBy=${pagination.sortBy ?? ''}&sortDirection=${pagination.sortDirection ?? ''}`
    if (pagination.search) {
      url += `&search=${pagination.search}`
    }
    return this.apiService.get<PaginatedResponse<Job>>(url)
  }

  getJob(id: number): Observable<Job> {
    return this.apiService.get<Job>(`${this.apiUrl}/${id}`)
  }

  createJob(job: CreateJobDto): Observable<Job> {
    return this.apiService.post<Job>(this.apiUrl, job)
  }

  restartJob(id: number): Observable<void> {
    return this.apiService.post<void>(`${this.apiUrl}/${id}/restart`, {})
  }

  cancelJob(id: number): Observable<void> {
    return this.apiService.post<void>(`${this.apiUrl}/${id}/cancel`, {})
  }

  deleteJob(id: number): Observable<void> {
    return this.apiService.delete<void>(`${this.apiUrl}/${id}`)
  }

  bulkDeleteJobs(dto: BulkDeleteJobsDto): Observable<void> {
    return this.apiService.post<void>(`${this.apiUrl}/bulk-delete`, dto)
  }
}
