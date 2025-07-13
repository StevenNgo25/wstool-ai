import { Instance } from "./user.model"

export interface Message {
  id: number
  title: string
  textContent?: string
  imageUrl?: string
  messageType: string
  createdByUserName: string
  instanceId: number
  instanceName: string
  createdAt: string
  updatedAt?: string
  assignedGroups: Group[]
}

export interface CreateMessageDto {
  title: string
  textContent?: string
  messageType: string
  instanceId: number
  groupIds: number[]
}

export interface CreateMessageWithFileDto {
  title: string
  textContent?: string
  messageType: string
  instanceId: number
  imageFile?: File
  groupIds: number[]
}

export interface UpdateMessageDto {
  title?: string
  textContent?: string
  messageType?: string
  instanceId?: number
  groupIds?: number[]
}

export interface UpdateMessageWithFileDto {
  title?: string
  textContent?: string
  messageType?: string
  instanceId?: number
  imageFile?: File
  removeImage?: boolean
  groupIds?: number[]
}

export interface Group {
  id: number
  groupId: string
  name: string
  description?: string
  participantCount: number
  isActive: boolean
  createdAt: string
  lastSyncAt?: string
  instanceId: number
  instance: Instance
}

export interface DashboardStats {
  totalUsers: number
  totalInstances: number
  totalMessages: number
  totalGroups: number
  totalJobs: number
  pendingJobs: number
  completedJobs: number
  failedJobs: number
  last7DaysSentMessages: number
  last7DaysFailedMessages: number
  topGroups: GroupStats[]

  totalSentMessages: number
  totalUniqueUsers: number
}

export interface GroupStats {
  groupId: string
  groupName: string
  participantCount: number
  messagesSent: number
}

export interface BulkDeleteRequest {
  ids: number[]
}