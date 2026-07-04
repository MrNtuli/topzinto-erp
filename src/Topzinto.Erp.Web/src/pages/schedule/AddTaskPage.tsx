import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useQuery, useMutation } from '@tanstack/react-query'
import { getProjects } from '@/api/projects'
import { createProjectTask } from '@/api/schedule'
import styles from '../site-reports/AddSiteReportPage.module.css'

export function AddTaskPage() {
  const navigate = useNavigate()
  const { data: projects } = useQuery({ queryKey: ['projects'], queryFn: () => getProjects() })

  const [projectId, setProjectId] = useState('')
  const [title, setTitle] = useState('')
  const [description, setDescription] = useState('')
  const [assignedToName, setAssignedToName] = useState('')
  const [dueDate, setDueDate] = useState('')
  const [priority, setPriority] = useState('Medium')
  const [status, setStatus] = useState('Open')

  const mutation = useMutation({
    mutationFn: () =>
      createProjectTask({
        projectId,
        title,
        description: description || undefined,
        assignedToName: assignedToName || undefined,
        dueDate: dueDate || undefined,
        priority,
        status,
      }),
    onSuccess: () => navigate('/schedule'),
  })

  return (
    <div className={styles.page}>
      <nav className={styles.breadcrumb}>Home &gt; Schedule &gt; New Task</nav>
      <h1>Add Project Task</h1>
      <form className={styles.form} onSubmit={(e) => { e.preventDefault(); if (!projectId || !title) return; mutation.mutate() }}>
        <label>
          Project *
          <select value={projectId} onChange={(e) => setProjectId(e.target.value)} required>
            <option value="">Select project</option>
            {projects?.map((p) => <option key={p.id} value={p.id}>{p.code} — {p.name}</option>)}
          </select>
        </label>
        <label>Title *<input value={title} onChange={(e) => setTitle(e.target.value)} required /></label>
        <label>Description<textarea rows={3} value={description} onChange={(e) => setDescription(e.target.value)} /></label>
        <div className={styles.row}>
          <label>Assigned To<input value={assignedToName} onChange={(e) => setAssignedToName(e.target.value)} /></label>
          <label>Due Date<input type="date" value={dueDate} onChange={(e) => setDueDate(e.target.value)} /></label>
          <label>
            Priority
            <select value={priority} onChange={(e) => setPriority(e.target.value)}>
              <option value="Low">Low</option><option value="Medium">Medium</option><option value="High">High</option>
            </select>
          </label>
          <label>
            Status
            <select value={status} onChange={(e) => setStatus(e.target.value)}>
              <option value="Open">Open</option><option value="In Progress">In Progress</option><option value="Completed">Completed</option>
            </select>
          </label>
        </div>
        <div className={styles.actions}>
          <button type="button" className={styles.draft} onClick={() => navigate('/schedule')}>Cancel</button>
          <button type="submit" className={styles.submit} disabled={mutation.isPending}>Create Task</button>
        </div>
      </form>
    </div>
  )
}
