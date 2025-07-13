export interface Job {
  id: number
  name: string
  description?: string
  jobType: string
  status: string
  messageTitle: string
  instanceName: string
  createdByUserName: string
  scheduledAt?: string
  startedAt?: string
  completedAt?: string
  createdAt: string
  totalSentMessages: number
  successfulMessages: number
  failedMessages: number
  logs: JobLog[]

  progress: number
  sentMessages: any[]
  assignedGroups: any[]
  targetPhoneNumbers: any[]
}

export interface CreateJobDto {
  name: string
  description?: string
  jobType: string
  messageId: number
  instanceId: number
  scheduledAt?: string
  groupIds?: number[]
  phoneNumbers?: string[]
}

export interface JobLog {
  id: number
  logLevel: string
  message: string
  details?: string
  createdAt: string
}
export interface BulkDeleteJobsDto{
  jobIds: number[]
}