import { apiFetch } from './client'

export interface UserProfile {
  id: string
  email: string
  firstName: string
  lastName: string
  phone: string | null
  role: string
  lastLoginAt: string | null
}

export interface UpdateMyProfileRequest {
  firstName: string
  lastName: string
  phone?: string | null
}

export function getMyProfile() {
  return apiFetch<UserProfile>('/users/me')
}

export function updateMyProfile(data: UpdateMyProfileRequest) {
  return apiFetch<UserProfile>('/users/me', {
    method: 'PATCH',
    body: JSON.stringify(data),
  })
}
