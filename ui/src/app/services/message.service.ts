import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { ApiService } from './api.service';
import { PaginatedResponse, PaginationRequest } from '../models/common.model';
import {
  Message,
  CreateMessageDto,
  UpdateMessageDto,
  CreateMessageWithFileDto,
  UpdateMessageWithFileDto,
  BulkDeleteRequest,
} from '../models/message.model';

@Injectable({
  providedIn: 'root',
})
export class MessageService {
  private apiUrl = `${environment.apiUrl}/messages`;

  constructor(private apiService: ApiService) {}

  getMessages(
    pagination: PaginationRequest
  ): Observable<PaginatedResponse<Message>> {
    let url = `${this.apiUrl}?page=${pagination.page}&pageSize=${pagination.pageSize}&sortBy=${pagination.sortBy}&sortDirection=${pagination.sortDirection}`;
    if (pagination.search) {
      url += `&search=${pagination.search}`;
    }
    return this.apiService.get<PaginatedResponse<Message>>(url);
  }

  getMessage(id: number): Observable<Message> {
    return this.apiService.get<Message>(`${this.apiUrl}/${id}`);
  }

  createMessage(message: CreateMessageDto): Observable<Message> {
    return this.apiService.post<Message>(this.apiUrl, message);
  }

  createMessageWithFile(
    message: CreateMessageWithFileDto
  ): Observable<Message> {
    var url = `${this.apiUrl}/with-file`

    const formData = new FormData();
    formData.append('title', message.title);
    formData.append('messageType', message.messageType);
    formData.append('instanceId', message.instanceId.toString());

    if (message.textContent) {
      formData.append('textContent', message.textContent);
    }
    if (message.imageFile) {
      formData.append('imageFile', message.imageFile, message.imageFile.name);
    }
    if (message.groupIds && message.groupIds.length > 0) {
      message.groupIds.forEach((id) =>
        formData.append('groupIds', id.toString())
      );
    }

    return this.apiService.postFile<Message>(url, formData);
  }

  updateMessage(id: number, message: UpdateMessageDto): Observable<void> {
    return this.apiService.put<void>(`${this.apiUrl}/${id}`, message);
  }

  updateMessageWithFile(
    id: number,
    message: UpdateMessageWithFileDto
  ): Observable<void> {
    const formData = new FormData();
    if (message.title) formData.append('title', message.title);
    if (message.messageType)
      formData.append('messageType', message.messageType);
    if (message.instanceId)
      formData.append('instanceId', message.instanceId.toString());
    if (message.removeImage)
      formData.append('removeImage', message.removeImage.toString());

    if (message.textContent) {
      formData.append('textContent', message.textContent);
    }
    if (message.imageFile) {
      formData.append('imageFile', message.imageFile, message.imageFile.name);
    }
    if (message.groupIds && message.groupIds.length > 0) {
      message.groupIds.forEach((groupId) =>
        formData.append('groupIds', groupId.toString())
      );
    }

    return this.apiService.putFile<void>(`${this.apiUrl}/${id}/with-file`, formData);
  }

  deleteMessage(id: number): Observable<void> {
    return this.apiService.delete<void>(`${this.apiUrl}/${id}`);
  }

  bulkDeleteMessages(dto: BulkDeleteRequest): Observable<void> {
    return this.apiService.post<void>(`${this.apiUrl}/bulk-delete`, dto);
  }
}
