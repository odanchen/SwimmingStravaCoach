# Virtual Endurance Coach — Build Plan

## 1. How to use this plan

Build the application as a sequence of small vertical slices. Each slice should leave the repository runnable, tested in proportion to the change, and easy to demonstrate.

The intended progression is:

1. establish a working application shell;
2. implement the core workflow with manual data;
3. integrate Strava;
4. add AI plan generation;
5. add feedback-driven adaptation;
6. deploy and refine.

Avoid building all database entities, screens, or infrastructure in advance. Add them when a user-visible slice requires them.

## 2. Working conventions

- Use one repository for the React frontend and .NET backend.
- Begin with one backend project organized by feature folders. Split projects only when a clear boundary justifies it.
- Keep the application runnable locally after every work session.
- Put database schema changes in migrations.
- Add a focused test with each important business rule or bug fix.
- Use substitutes for Strava and the AI model in automated tests.
- Do not let AI output bypass backend validation.
- Prefer a narrow completed flow over several partially implemented layers.

Suggested initial structure:

```text
/
├── src/
│   ├── SwimmingCoach.Api/
│   └── swimming-coach-web/
├── tests/
│   └── SwimmingCoach.Api.Tests/
├── PRODUCT_SPEC.md
├── BUILD_PLAN.md
└── README.md
```

## 3. Roadmap

### Phase 0 — Runnable foundation

Outcome: the frontend and backend run locally and can communicate.

Small tasks:

- Create the .NET solution, ASP.NET Core API, and backend test project.
- Create the React and TypeScript application.
- Add a backend health endpoint.
- Show backend health on a temporary frontend home page.
- Add local configuration files without committing secrets.
- Document local start, build, and test commands.
- Add a basic continuous-integration check for backend tests and both production builds.

Done when:

- a fresh checkout can be started using the documented steps;
- the browser can confirm that the API is reachable;
- backend tests and frontend/backend builds pass.

### Phase 1 — Database foundation

Outcome: the backend can store and retrieve data from local MySQL.

Small tasks:

- Add a repeatable local MySQL setup.
- Configure the backend data context and local connection settings.
- Add an initial trivial entity or application metadata record.
- Create and apply the first migration.
- Add one database integration test.
- Document database startup and migration commands.

Done when the API can read and write MySQL locally and the schema can be recreated from migrations.

### Phase 2 — Registration and authentication

Outcome: a user can create an account and use an authenticated session.

Small tasks:

- Add the .NET identity model and database tables.
- Implement register, sign-in, sign-out, and current-user endpoints.
- Configure secure cookie authentication and cross-origin behavior for local development.
- Build registration and sign-in forms.
- Add protected frontend routing.
- Add an account-recovery flow or a development-friendly first version of it.
- Test successful authentication, invalid credentials, and access to protected endpoints.

Done when a registered user can sign in, refresh the browser without losing the session, reach a protected page, and sign out.

### Phase 3 — Athlete profile

Outcome: an authenticated user can maintain the information needed for coaching.

Small tasks:

- Define the first profile fields from the product specification.
- Add the profile migration and user ownership relationship.
- Implement get and update profile endpoints.
- Build the profile form with validation.
- Add pool length, preferred units, availability, and experience fields.
- Show an onboarding-complete state.
- Test that one user cannot access another user's profile.

Done when profile data survives sign-out and is isolated by user.

### Phase 4 — Goal management

Outcome: a user can define the target for a future plan.

Small tasks:

- Implement one goal type first: a dated swimming event with distance and optional target time.
- Add goal status and ownership.
- Implement create, view, edit, complete, and archive endpoints.
- Build the goal form and goal summary screen.
- Validate dates, distances, and target times.
- Add other swimming goal types one at a time.

Done when the user can create and revisit one active goal with enough information to generate a plan.

### Phase 5 — Manual activities

Outcome: workout history can exist before Strava is integrated.

Small tasks:

- Define the normalized activity entity.
- Implement manual activity creation and editing.
- Build an activity list and activity detail screen.
- Support swim, run, and strength activity types.
- Add optional perceived effort and notes.
- Test user ownership and basic validation.

Done when recent training can be entered and reviewed without an external provider.

### Phase 6 — Plan data and deterministic sample plan

Outcome: the complete plan-viewing workflow works without depending on an AI model.

Small tasks:

- Define training plan, week, planned workout, and structured swim-set models.
- Add migrations and read endpoints.
- Implement workout distance calculation and validation.
- Create a deterministic sample-plan generator for the first goal type.
- Build the plan overview and workout detail screens.
- Add workout states such as upcoming, completed, missed, and moved.
- Add tests for nested set arithmetic and scheduling constraints.

Done when a user can generate, view, and navigate a valid sample plan derived from their goal and schedule.

### Phase 7 — First Azure deployment

Outcome: the application skeleton is reachable in Azure before external integrations make deployment more complicated.

Small tasks:

- Create the smallest appropriate Azure resources for the frontend, backend, and MySQL database.
- Configure deployed database migrations and secrets.
- Deploy the API and frontend manually once.
- Resolve HTTPS, cookie, origin, and routing behavior.
- Automate deployment after the manual path is understood.
- Add basic request and error logging.

Done when registration, profile, goal creation, and sample-plan viewing work in the hosted environment.

### Phase 8 — Strava connection and historical import

Outcome: the user can connect Strava and import real activities on demand.

Small tasks:

- Register the application with Strava and configure local/deployed callback addresses.
- Implement the authorization start and callback endpoints.
- Store tokens against the authenticated user without exposing them to React.
- Implement token refresh.
- Build connection status, connect, disconnect, and manual-sync controls.
- Import one page of activities.
- Map supported Strava fields into the normalized activity model.
- Add pagination and choose a history cutoff.
- Deduplicate using the Strava activity identifier.
- Test the client and mapper using saved, sanitized responses.

Done when a user can connect Strava, run a manual sync repeatedly, and see imported activities without duplicates.

### Phase 9 — Webhooks and reliable synchronization

Outcome: new Strava activities appear without a manual sync.

Small tasks:

- Implement webhook subscription verification.
- Add the webhook event receiver.
- Persist a deduplicated work item before returning success.
- Process pending items with a hosted background service.
- Retrieve and normalize the changed activity.
- Retry failed work with a limit and visible status.
- Add periodic reconciliation for missed webhook events.
- Test repeated and out-of-order delivery.

Done when a newly uploaded activity appears automatically and duplicate webhook delivery has no duplicate effect.

### Phase 10 — Activity-to-workout matching

Outcome: actual training is connected to the plan.

Small tasks:

- Implement a simple score using sport, date, distance, and duration.
- Define high-confidence, possible, and unmatched thresholds.
- Run matching after import and after plan creation.
- Show the proposed match on activity and workout screens.
- Let the user confirm, change, or remove a match.
- Mark workouts complete or partially complete from confirmed matches.
- Test ambiguous and corrected matches.

Done when a real imported activity can update the correct planned workout and the user can correct mistakes.

### Phase 11 — Post-workout check-ins

Outcome: the application captures information that Strava cannot provide reliably.

Small tasks:

- Define check-in questions and responses.
- Begin with perceived effort, completion, energy, pain, and free-text notes.
- Show a short check-in after a matched activity.
- Add conditional swim and strength questions.
- Allow questions to be skipped.
- Display answers on the activity detail screen.

Done when the user can add meaningful subjective context to an imported workout in under a minute.

### Phase 12 — AI-generated draft plans

Outcome: the deterministic sample generator can be replaced by an AI-generated draft that still obeys application rules.

Small tasks:

- Define the exact structured request and response contracts.
- Create an AI model interface and a fake implementation for tests.
- Build a prompt from the goal, profile, availability, and summarized history.
- Connect the Azure-hosted model.
- Parse and validate structured output.
- Reject invalid workout arithmetic or scheduling and expose a useful error.
- Store the prompt version and generated draft.
- Add a review-and-activate step.

Done when a valid plan can be generated, reviewed, and activated without giving the model direct write access to application state.

### Phase 13 — Adaptive plan changes

Outcome: new training and feedback can produce a controlled change to the plan.

Small tasks:

- Define a structured plan-change proposal.
- Summarize the relevant recent activities, adherence, and check-ins.
- Trigger evaluation once per meaningful new input.
- Allow a no-change result.
- Validate proposed changes against completed workouts and coaching rules.
- Show the reason and affected workouts.
- Implement accept and reject actions.
- Save a new plan version only after acceptance.
- Add an idempotency test so reprocessing an activity cannot repeat a change.

Done when one completed workout can lead to an explained proposal that the user controls.

### Phase 14 — Refinement

Outcome: the application is dependable enough for regular personal use and a polished demonstration.

Possible tasks:

- Improve mobile and poolside usability.
- Add account recovery suitable for the deployed environment.
- Improve empty, loading, and failure states.
- Add notification or reminder support.
- Add richer swimming goal types and open-water planning.
- Add running-specific plan generation.
- Review logs, secrets, backups, and error handling.
- Add a small seeded demonstration account or scripted demo path.
- Document architecture decisions and tradeoffs for interviews.
