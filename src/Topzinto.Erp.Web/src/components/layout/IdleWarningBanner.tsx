import styles from './IdleWarningBanner.module.css'

interface IdleWarningBannerProps {
  minutesLeft: number
}

export function IdleWarningBanner({ minutesLeft }: IdleWarningBannerProps) {
  return (
    <div className={styles.banner} role="alert">
      You will be signed out in about {minutesLeft} minute{minutesLeft === 1 ? '' : 's'} due to inactivity.
      Move the mouse or press a key to stay signed in.
    </div>
  )
}
