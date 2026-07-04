import { useState, useRef, useEffect } from 'react'
import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { Search } from 'lucide-react'
import { search } from '@/api/search'
import styles from './GlobalSearch.module.css'

export function GlobalSearch() {
  const [query, setQuery] = useState('')
  const [open, setOpen] = useState(false)
  const ref = useRef<HTMLDivElement>(null)

  const { data, isFetching } = useQuery({
    queryKey: ['search', query],
    queryFn: () => search(query),
    enabled: query.trim().length >= 2,
    retry: false,
  })

  useEffect(() => {
    const handler = (e: MouseEvent) => {
      if (ref.current && !ref.current.contains(e.target as Node)) setOpen(false)
    }
    document.addEventListener('mousedown', handler)
    return () => document.removeEventListener('mousedown', handler)
  }, [])

  return (
    <div className={styles.wrap} ref={ref}>
      <Search size={18} className={styles.icon} />
      <input
        type="search"
        placeholder="Search projects, clients, suppliers..."
        value={query}
        onChange={(e) => { setQuery(e.target.value); setOpen(true) }}
        onFocus={() => setOpen(true)}
      />
      {open && query.trim().length >= 2 && (
        <div className={styles.dropdown}>
          {isFetching && <p className={styles.empty}>Searching...</p>}
          {!isFetching && data?.length === 0 && <p className={styles.empty}>No results</p>}
          <ul>
            {data?.map((r) => (
              <li key={`${r.type}-${r.id}`}>
                <Link to={r.linkPath} onClick={() => { setOpen(false); setQuery('') }}>
                  <span className={styles.type}>{r.type}</span>
                  <strong>{r.title}</strong>
                  <span className={styles.sub}>{r.subtitle}</span>
                </Link>
              </li>
            ))}
          </ul>
        </div>
      )}
    </div>
  )
}
