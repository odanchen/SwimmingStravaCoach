import { useEffect, useState, type FormEvent } from 'react'
import {
  getProfile,
  saveProfile,
  type AthleteProfileInput,
  type ExperienceLevel,
  type MeasurementSystem,
  type PoolLengthUnit,
} from './api/profile'

type FormStatus = 'loading' | 'idle' | 'saving' | 'saved' | 'error'

function createDefaultProfile(): AthleteProfileInput {
  return {
    timeZoneId: Intl.DateTimeFormat().resolvedOptions().timeZone,
    preferredUnits: 'Metric',
    poolLength: 25,
    poolLengthUnit: 'Meters',
    swimmingExperience: 'Intermediate',
    runningExperience: 'None',
    trainingDaysPerWeek: 4,
    typicalSessionMinutes: 60,
  }
}

function ProfileEditor() {
  const [profile, setProfile] = useState<AthleteProfileInput>(
    createDefaultProfile,
  )
  const [status, setStatus] = useState<FormStatus>('loading')

  useEffect(() => {
    const controller = new AbortController()

    void getProfile(controller.signal)
      .then((storedProfile) => {
        if (storedProfile) {
          setProfile(storedProfile)
        }
        setStatus('idle')
      })
      .catch((error: unknown) => {
        if (!(error instanceof DOMException && error.name === 'AbortError')) {
          setStatus('error')
        }
      })

    return () => controller.abort()
  }, [])

  function updateProfile(changes: Partial<AthleteProfileInput>) {
    setStatus('idle')
    setProfile((currentProfile) => ({
      ...currentProfile,
      ...changes,
    }))
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setStatus('saving')

    try {
      const savedProfile = await saveProfile(profile)
      setProfile(savedProfile)
      setStatus('saved')
    } catch {
      setStatus('error')
    }
  }

  if (status === 'loading') {
    return <section className="profile-card">Loading athlete profile…</section>
  }

  return (
    <section className="profile-card">
      <div className="section-heading">
        <div>
          <p className="eyebrow">Coaching inputs</p>
          <h2>Athlete profile</h2>
        </div>
        {status === 'saved' && <span className="saved-label">Saved</span>}
      </div>

      <form onSubmit={handleSubmit}>
        <fieldset disabled={status === 'saving'}>
          <label>
            Time zone
            <input
              required
              maxLength={100}
              value={profile.timeZoneId}
              onChange={(event) =>
                updateProfile({ timeZoneId: event.target.value })
              }
            />
          </label>

          <label>
            Preferred units
            <select
              value={profile.preferredUnits}
              onChange={(event) =>
                updateProfile({
                  preferredUnits: event.target.value as MeasurementSystem,
                })
              }
            >
              <option value="Metric">Metric</option>
              <option value="Imperial">Imperial</option>
            </select>
          </label>

          <div className="field-pair">
            <label>
              Usual pool length
              <input
                required
                min={10}
                max={100}
                step="0.01"
                type="number"
                value={profile.poolLength}
                onChange={(event) =>
                  updateProfile({ poolLength: Number(event.target.value) })
                }
              />
            </label>

            <label>
              Pool unit
              <select
                value={profile.poolLengthUnit}
                onChange={(event) =>
                  updateProfile({
                    poolLengthUnit: event.target.value as PoolLengthUnit,
                  })
                }
              >
                <option value="Meters">Meters</option>
                <option value="Yards">Yards</option>
              </select>
            </label>
          </div>

          <label>
            Swimming experience
            <ExperienceSelect
              value={profile.swimmingExperience}
              includeNone={false}
              onChange={(value) =>
                updateProfile({ swimmingExperience: value })
              }
            />
          </label>

          <label>
            Running experience
            <ExperienceSelect
              value={profile.runningExperience}
              includeNone
              onChange={(value) => updateProfile({ runningExperience: value })}
            />
          </label>

          <div className="field-pair">
            <label>
              Training days per week
              <input
                required
                min={1}
                max={7}
                type="number"
                value={profile.trainingDaysPerWeek}
                onChange={(event) =>
                  updateProfile({
                    trainingDaysPerWeek: Number(event.target.value),
                  })
                }
              />
            </label>

            <label>
              Typical session (minutes)
              <input
                required
                min={15}
                max={300}
                type="number"
                value={profile.typicalSessionMinutes}
                onChange={(event) =>
                  updateProfile({
                    typicalSessionMinutes: Number(event.target.value),
                  })
                }
              />
            </label>
          </div>

          <button className="primary-button" type="submit">
            {status === 'saving' ? 'Saving…' : 'Save profile'}
          </button>
        </fieldset>
      </form>

      {status === 'error' && (
        <p role="alert">The athlete profile could not be loaded or saved.</p>
      )}
    </section>
  )
}

type ExperienceSelectProps = {
  value: ExperienceLevel
  includeNone: boolean
  onChange: (value: ExperienceLevel) => void
}

function ExperienceSelect({
  value,
  includeNone,
  onChange,
}: ExperienceSelectProps) {
  return (
    <select
      value={value}
      onChange={(event) => onChange(event.target.value as ExperienceLevel)}
    >
      {includeNone && <option value="None">None</option>}
      <option value="Beginner">Beginner</option>
      <option value="Intermediate">Intermediate</option>
      <option value="Advanced">Advanced</option>
      <option value="Competitive">Competitive</option>
    </select>
  )
}

export default ProfileEditor
