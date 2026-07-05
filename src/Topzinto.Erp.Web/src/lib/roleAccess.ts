const ADMIN_ROLES = ['SuperAdmin', 'Director', 'Super Admin', 'Managing Director']

const FIELD_ROLES = ['Employee', 'Driver', 'Foreman', 'Supervisor']
const FIELD_NAV = ['/', '/site-reports', '/schedule', '/documents', '/settings', '/profile', '/notifications', '/timesheets', '/messages']

const DISPLAY_TO_SYSTEM: Record<string, string> = {
  'Managing Director': 'Director',
  'Super Admin': 'SuperAdmin',
  'Operations Manager': 'OperationsManager',
  'Project Manager': 'ProjectManager',
  'Contract Manager': 'ContractManager',
  'Quantity Surveyor': 'QuantitySurveyor',
  'Fleet Manager': 'FleetManager',
  'Equipment Manager': 'EquipmentManager',
  'Safety Officer': 'SafetyOfficer',
  'Store Controller': 'StoreController',
}

const NAV_RESTRICTIONS: Record<string, string[]> = {
  '/employees': ['HR', 'OperationsManager', 'ProjectManager', 'Receptionist'],
  '/timesheets': ['HR', 'OperationsManager', 'ProjectManager', 'Supervisor', 'Foreman'],
  '/fleet': ['FleetManager', 'EquipmentManager', 'OperationsManager', 'ProjectManager'],
  '/equipment': ['EquipmentManager', 'FleetManager', 'OperationsManager', 'ProjectManager'],
  '/procurement': ['Procurement', 'Finance', 'StoreController', 'OperationsManager', 'ProjectManager'],
  '/suppliers': ['Procurement', 'Finance', 'OperationsManager', 'ProjectManager'],
  '/stores': ['StoreController', 'Procurement', 'OperationsManager', 'ProjectManager'],
  '/boq': ['QuantitySurveyor', 'Estimator', 'Finance', 'ContractManager', 'ProjectManager', 'OperationsManager'],
  '/reports': ['Finance', 'QuantitySurveyor', 'ContractManager', 'OperationsManager', 'ProjectManager'],
  '/tenders': ['ContractManager', 'Estimator', 'OperationsManager', 'ProjectManager', 'Receptionist'],
  '/contracts': ['ContractManager', 'QuantitySurveyor', 'OperationsManager', 'ProjectManager'],
  '/clients': ['Receptionist', 'ContractManager', 'OperationsManager', 'ProjectManager', 'Estimator'],
  '/admin/audit': [],
  '/admin/users': [],
  '/admin/roles': [],
  '/safety': ['SafetyOfficer'],
  '/compliance': ['SafetyOfficer', 'HR', 'OperationsManager', 'ProjectManager'],
}

function systemRole(role: string | undefined): string {
  if (!role) return ''
  return DISPLAY_TO_SYSTEM[role] ?? role
}

export function canAccessNav(role: string | undefined, path: string): boolean {
  if (!role) return true

  const sys = systemRole(role)

  if (ADMIN_ROLES.includes(role) || ADMIN_ROLES.includes(sys)) return true

  if (FIELD_ROLES.includes(sys)) {
    return FIELD_NAV.includes(path)
  }

  const allowed = NAV_RESTRICTIONS[path]
  if (!allowed) return true
  if (allowed.length === 0) return false
  return allowed.includes(sys)
}

/** Map a URL path to the sidebar module base used for RBAC checks. */
export function navBasePath(pathname: string): string {
  if (pathname === '/') return '/'
  const segments = pathname.split('/').filter(Boolean)
  if (segments[0] === 'admin') return `/admin/${segments[1] ?? ''}`
  return `/${segments[0]}`
}

/** Route guard — same rules as sidebar nav, applied to nested URLs. */
export function canAccessRoute(role: string | undefined, pathname: string): boolean {
  return canAccessNav(role, navBasePath(pathname))
}
