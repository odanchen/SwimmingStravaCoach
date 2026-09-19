export type MeasurementSystem = 'Metric' | 'Imperial'
export type PoolLengthUnit = 'Meters' | 'Yards'
export type ExperienceLevel =
  | 'None'
  | 'Beginner'
  | 'Intermediate'
  | 'Advanced'
  | 'Competitive'

export type AthleteProfileInput = {
  timeZoneId: string
  preferredUnits: MeasurementSystem
  poolLength: number
  poolLengthUnit: PoolLengthUnit
  swimmingExperience: ExperienceLevel
  runningExperience: ExperienceLevel
  trainingDaysPerWeek: number
  typicalSessionMinutes: number
}

export type AthleteProfile = AthleteProfileInput & {
  onboardingComplete: boolean
  updatedAtUtc: string
}

const mutationHeaders = {
  'Content-Type': 'application/json',
  'X-Requested-With': 'SwimmingCoach.Web',
}

export async function getProfile(
  signal?: AbortSignal,
): Promise<AthleteProfile | null> {
  const response = await fetch('/api/profile/', {
    credentials: 'include',
    signal,
  })

  if (response.status === 404) {
    return null
  }

  if (!response.ok) {
    throw new Error(`Could not load the profile (${response.status})`)
  }

  return response.json() as Promise<AthleteProfile>
}

export async function saveProfile(
  profile: AthleteProfileInput,
): Promise<AthleteProfile> {
  const response = await fetch('/api/profile/', {
    method: 'PUT',
    credentials: 'include',
    headers: mutationHeaders,
    body: JSON.stringify(profile),
  })

  if (!response.ok) {
    throw new Error(`Could not save the profile (${response.status})`)
  }

  return response.json() as Promise<AthleteProfile>
}
