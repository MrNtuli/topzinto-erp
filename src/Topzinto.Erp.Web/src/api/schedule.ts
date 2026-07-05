import { apiFetch } from './client'

export interface ScheduleEvent {
  id: string
  title: string
  type: string
  date: string
  endDate: string | null
  projectName: string | null
  status: string
}

export interface ProjectMilestone {
  id: string
  projectId: string
  projectName: string
  name: string
  startDate: string | null
  dueDate: string
  status: string
  progress: number
}

export interface ProjectTask {
  id: string
  projectId: string
  projectName: string
  title: string
  status: string
  priority: string
  dueDate: string | null
  assignedToName: string | null
}

export function getScheduleEvents() {
  return apiFetch<ScheduleEvent[]>('/schedule')
}

export function getProjectMilestones(projectId: string) {
  return apiFetch<ProjectMilestone[]>(`/projects/${projectId}/milestones`)
}

export function getProjectTasks(projectId: string) {
  return apiFetch<ProjectTask[]>(`/projects/${projectId}/tasks`)
}

export function getOverdueTasks() {
  return apiFetch<ProjectTask[]>('/tasks/overdue')
}

export function createProjectTask(data: {
  projectId: string
  title: string
  description?: string
  assignedToName?: string
  dueDate?: string
  priority: string
  status: string
  milestoneId?: string
}) {
  return apiFetch<ProjectTask>(`/projects/${data.projectId}/tasks`, {
    method: 'POST',
    body: JSON.stringify(data),
  })
}

export interface GanttTask {
  id: string
  projectId: string
  projectName: string
  title: string
  startDate: string | null
  endDate: string | null
  status: string
  priority: string
  milestoneId: string | null
}

export interface GanttMilestone {
  id: string
  projectId: string
  projectName: string
  name: string
  startDate: string | null
  endDate: string
  status: string
  progress: number
}

export interface GanttData {
  tasks: GanttTask[]
  milestones: GanttMilestone[]
}

export function getGanttData(projectId?: string) {
  const query = projectId ? `?projectId=${encodeURIComponent(projectId)}` : ''
  return apiFetch<GanttData>(`/schedule/gantt${query}`)
}
