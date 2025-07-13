export interface PaginationRequest {
  page: number
  pageSize: number
  search?: string
  sortBy?: string
  sortDirection?: string
}

export interface PaginatedResponse<T> {
  data: T[]
  pagination: PaginationMetadata
}

export interface PaginationMetadata {
  currentPage: number
  pageSize: number
  totalCount: number
  totalPages: number
  hasPrevious: boolean
  hasNext: boolean
  search?: string
  sortBy?: string
  sortDirection?: string
}

export interface BulkDeleteRequest {
  ids: number[]
}

export interface ApiResponse<T> {
  success: boolean
  data?: T
  message?: string
  errors?: string[]
}
