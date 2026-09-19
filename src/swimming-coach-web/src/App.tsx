import { useEffect, useState } from 'react'
import {
  deleteAccount,
  getCurrentUser,
  getGoogleLoginUrl,
  logOut,
  type CurrentUser,
} from './api/auth'
import { getHealth } from './api/health'
import ProfileEditor from './ProfileEditor'
import './App.css'

type ApiStatus = 'checking' | 'online' | 'offline'
type AuthStatus = 'loading' | 'anonymous' | 'authenticated' | 'error'

function App() {
  const [apiStatus, setApiStatus] = useState<ApiStatus>('checking')
  const [authStatus, setAuthStatus] = useState<AuthStatus>('loading')
  const [user, setUser] = useState<CurrentUser | null>(null)
  const [actionPending, setActionPending] = useState(false)
  const [actionError, setActionError] = useState<string | null>(null)

  useEffect(() => {
    const controller = new AbortController()

    async function loadPage() {
      const healthRequest = getHealth(controller.signal)
        .then((health) => {
          setApiStatus(health.status === 'ok' ? 'online' : 'offline')
        })
        .catch((error: unknown) => {
          if (!(error instanceof DOMException && error.name === 'AbortError')) {
            setApiStatus('offline')
          }
        })

      const userRequest = getCurrentUser(controller.signal)
        .then((currentUser) => {
          setUser(currentUser)
          setAuthStatus(currentUser ? 'authenticated' : 'anonymous')
        })
        .catch((error: unknown) => {
          if (!(error instanceof DOMException && error.name === 'AbortError')) {
            setAuthStatus('error')
          }
        })

      await Promise.all([healthRequest, userRequest])
    }

    void loadPage()

    return () => controller.abort()
  }, [])

  async function handleLogOut() {
    setActionPending(true)
    setActionError(null)

    try {
      await logOut()
      setUser(null)
      setAuthStatus('anonymous')
    } catch {
      setActionError('Log out failed. Please try again.')
    } finally {
      setActionPending(false)
    }
  }

  async function handleDeleteAccount() {
    const confirmed = window.confirm(
      'Delete your local Swimming Coach account? Your Google account is not affected.',
    )

    if (!confirmed) return

    setActionPending(true)
    setActionError(null)

    try {
      await deleteAccount()
      setUser(null)
      setAuthStatus('anonymous')
    } catch {
      setActionError('Account deletion failed. Please try again.')
    } finally {
      setActionPending(false)
    }
  }

  return (
    <main className="app-shell">
      <header>
        <p className="eyebrow">Personal training workspace</p>
        <h1>Swimming Coach</h1>
        <p className="api-status">
          API: <strong data-status={apiStatus}>{apiStatus}</strong>
        </p>
      </header>

      <section className="auth-card">
        {authStatus === 'loading' && <p>Checking your session…</p>}

        {authStatus === 'error' && (
          <p role="alert">The authentication service could not be reached.</p>
        )}

        {authStatus === 'anonymous' && (
          <>
            <h2>Sign in to continue</h2>
            <p>Your Google password is never shared with this application.</p>
            <a className="primary-button" href={getGoogleLoginUrl()}>
              Continue with Google
            </a>
          </>
        )}

        {authStatus === 'authenticated' && user && (
          <>
            <h2>Welcome back</h2>
            <p className="user-email">{user.email ?? 'Google account'}</p>
            <div className="button-row">
              <button disabled={actionPending} onClick={handleLogOut}>
                Log out
              </button>
              <button
                className="danger-button"
                disabled={actionPending}
                onClick={handleDeleteAccount}
              >
                Delete local account
              </button>
            </div>
          </>
        )}

        {actionError && <p role="alert">{actionError}</p>}
      </section>

      {authStatus === 'authenticated' && user && <ProfileEditor />}
    </main>
  )
}

export default App
