import { useEffect, useState } from 'react'
import { getHealth } from './api/health'

type ApiStatus = 'checking' | 'online' | 'offline'

function App() {
  const [apiStatus, setApiStatus] = useState<ApiStatus>('checking')

  useEffect(() => {
    const controller = new AbortController()

    async function checkApi() {
      try {
        const health = await getHealth(controller.signal)

        setApiStatus(health.status === 'ok' ? 'online' : 'offline')
      } catch (error) {
        const requestWasCancelled =
          error instanceof DOMException && error.name === 'AbortError'

        if (!requestWasCancelled) {
          setApiStatus('offline')
        }
      }
    }

    void checkApi()

    return () => {
      controller.abort()
    }
  }, [])

  return (
    <main>
      <h1>Swimming Coach</h1>
      <p>
        API status: <strong>{apiStatus}</strong>
      </p>
    </main>
  )
}

export default App