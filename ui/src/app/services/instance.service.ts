import { Injectable } from "@angular/core"
import { Observable } from "rxjs"
import { environment } from "../../environments/environment"
import { ApiService } from "./api.service"
import { PaginatedResponse, PaginationRequest } from "../models/common.model"
import { Instance, CreateInstanceDto, UpdateInstanceDto } from "../models/user.model"

@Injectable({
  providedIn: "root",
})
export class InstanceService {
  private apiUrl = `${environment.apiUrl}/instances`

  constructor(private apiService: ApiService) {}

  getInstances(pagination: PaginationRequest): Observable<PaginatedResponse<Instance>> {
    let url = `${this.apiUrl}?page=${pagination.page}&pageSize=${pagination.pageSize}&sortBy=${pagination.sortBy ?? ''}&sortDirection=${pagination.sortDirection ?? ''}`
    if (pagination.search) {
      url += `&search=${pagination.search}`
    }
    return this.apiService.get<PaginatedResponse<Instance>>(url)
  }

  getInstance(id: number): Observable<Instance> {
    return this.apiService.get<Instance>(`${this.apiUrl}/${id}`)
  }

  createInstance(instance: CreateInstanceDto): Observable<Instance> {
    return this.apiService.post<Instance>(this.apiUrl, instance)
  }

  updateInstance(id: number, instance: UpdateInstanceDto): Observable<void> {
    return this.apiService.put<void>(`${this.apiUrl}/${id}`, instance)
  }

  deleteInstance(id: number): Observable<void> {
    return this.apiService.delete<void>(`${this.apiUrl}/${id}`)
  }

  getQrCode(id: number): Observable<string> {
    return this.apiService.get<string>(this.apiUrl+ `/${id}/qrcode-base64`)
  }

  getCode(id: number, phone: string): Observable<string> {
    return this.apiService.get<string>(`${this.apiUrl}/${id}/connect-code?phone=${phone}`)
  }

  logoutInstance(id: number): Observable<void> {
    return this.apiService.get<void>(`${this.apiUrl}/${id}/logout`)
  }
}
