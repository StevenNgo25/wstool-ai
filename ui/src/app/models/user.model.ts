export interface User {
  id: number
  username: string
  email: string
  role: string
  isActive: boolean
  createdAt: string
  lastLoginAt?: string
  assignedInstances: Instance[]
}

export interface CreateUserDto {
  username: string
  email: string
  password: string
  role: string
}

export interface UpdateUserDto {
  username?: string
  email?: string
  password?: string
  role?: string
  isActive?: boolean
}

export interface LoginDto {
  username: string
  password: string
}

export interface LoginResponse {
  token: string
  username: string
  email: string
  role: string
  expiresAt: string
  user: any
}

export interface Instance {
  id: number
  name: string
  whatsAppNumber?: string
  whapiToken: string
  whapiUrl?: string
  isActive: boolean
  createdAt: string
  updatedAt?: string
}

export interface CreateInstanceDto {
  name: string
  whatsAppNumber?: string
  whapiToken: string
  whapiUrl?: string
}

export interface UpdateInstanceDto {
  name?: string
  whapiToken?: string
  whapiUrl?: string
  isActive?: boolean
}

export interface AssignInstancesDto {
  instanceIds: number[]
  canSendMessages: boolean
  canCreateJobs: boolean
}


export interface LoginRequest {
  username?: string
  password?: string
}