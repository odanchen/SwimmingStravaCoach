export type CurrentUser = {
  userId: string
  email: string | null
}

const apiOrigin = (
  import.meta.env.VITE_API_ORIGIN ?? 'http://localhost:5038'
).replace(/\/$/, '')

const mutationHeaders = {
  'X-Requested-With': 'SwimmingCoach.Web',
}

export function getGoogleLoginUrl(): string {
  return `${apiOrigin}/api/auth/google`
}

export async function getCurrentUser(
  signal?: AbortSignal,
): Promise<CurrentUser | null> {
  const response = await fetch('/api/auth/me', {
    credentials: 'include',
    signal,
  })

  if (response.status === 401) {
    return null
  }

  if (!response.ok) {
    throw new Error(`Could not load the current user (${response.status})`)
  }

  return response.json() as Promise<CurrentUser>
}

export async function logOut(): Promise<void> {
  const response = await fetch('/api/auth/logout', {
    method: 'POST',
    credentials: 'include',
    headers: mutationHeaders,
  })

  if (!response.ok) {
    throw new Error(`Could not log out (${response.status})`)
  }
}

export async function deleteAccount(): Promise<void> {
  const response = await fetch('/api/auth/account', {
    method: 'DELETE',
    credentials: 'include',
    headers: mutationHeaders,
  })

  if (!response.ok) {
    throw new Error(`Could not delete the account (${response.status})`)
  }
}
