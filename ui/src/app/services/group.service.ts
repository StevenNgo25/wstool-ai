import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { PaginatedResponse, PaginationRequest } from '../models/common.model';
import { Group, DashboardStats } from '../models/message.model'; // Corrected import for Group
import { ApiService } from './api.service';

@Injectable({
  providedIn: 'root',
})
export class GroupService {
  private apiUrl = `${environment.apiUrl}/groups`;

  constructor(private apiService: ApiService) {}

  getGroups(
    pagination: PaginationRequest
  ): Observable<PaginatedResponse<Group>> {
    return this.apiService.get<PaginatedResponse<Group>>(
      this.apiUrl +
        `?page=${pagination.page}&pageSize=${pagination.pageSize}&sortBy=${
          pagination.sortBy ?? ''
        }&sortDirection=${pagination.sortDirection ?? ''}`
    );
  }

  searchGroups(
    pagination: PaginationRequest,
    instanceIds: number[]
  ): Observable<PaginatedResponse<Group>> {
    const body = {
      page: pagination.page,
      pageSize: pagination.pageSize,
      sortBy: pagination.sortBy ?? '',
      sortDirection: pagination.sortDirection ?? '',
      search: pagination.search ?? '',
      instanceIds: instanceIds,
    };

    return this.apiService.post<PaginatedResponse<Group>>(
      this.apiUrl + '/search',
      body
    );
  }

  getDashboardStats(): Observable<DashboardStats> {
    return this.apiService.get<DashboardStats>(
      `${environment.apiUrl}/groups/stats`
    );
  }
}
