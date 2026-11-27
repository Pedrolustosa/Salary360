# Salary360 – Full-Stack Project Skeleton

<p align="center">
  <img src="https://img.shields.io/badge/.NET-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt=".NET" />
  <img src="https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=csharp&logoColor=white" alt="C%23" />
  <img src="https://img.shields.io/badge/Angular-DD0031?style=for-the-badge&logo=angular&logoColor=white" alt="Angular" />
  <img src="https://img.shields.io/badge/TypeScript-3178C6?style=for-the-badge&logo=typescript&logoColor=white" alt="TypeScript" />
  <img src="https://img.shields.io/badge/Tailwind%20CSS-06B6D4?style=for-the-badge&logo=tailwindcss&logoColor=white" alt="Tailwind%20CSS" />
  <img src="https://img.shields.io/badge/Sass-CC6699?style=for-the-badge&logo=sass&logoColor=white" alt="Sass" />
  <img src="https://img.shields.io/badge/Chart%20js-FF6384?style=for-the-badge&logo=chartdotjs&logoColor=white" alt="Chart%20js" />
  <img src="https://img.shields.io/badge/Sqlite-003B57?style=for-the-badge&logo=sqlite&logoColor=white" alt="Sqlite" />
  <img src="https://img.shields.io/badge/JWT-000000?style=for-the-badge&logo=jsonwebtokens&logoColor=white" alt="JWT" />
  <img src="https://img.shields.io/badge/SignalR-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt="SignalR" />
  <img src="https://img.shields.io/badge/Heroicons-8B5CF6?style=for-the-badge&logo=tailwindcss&logoColor=white" alt="Heroicons" />
  <img src="https://img.shields.io/badge/Node.js-339933?style=for-the-badge&logo=nodedotjs&logoColor=white" alt="Node.js" />
</p>

<p align="center">
  <img src="https://img.shields.io/badge/Clean%20Architecture-informational?style=for-the-badge" alt="Clean Architecture" />
  <img src="https://img.shields.io/badge/CQRS-informational?style=for-the-badge" alt="CQRS" />
  <img src="https://img.shields.io/badge/i18n-pt--BR%20%7C%20en--US%20%7C%20es--ES-success?style=for-the-badge" alt="i18n" />
  <img src="https://img.shields.io/badge/Theme-Light%20%7C%20Dark-lightgrey?style=for-the-badge" alt="Light and dark theme" />
</p>

## Purpose of this repository

This repository is the **base skeleton (template)** for new enterprise applications built with **.NET 10** and **Angular 22**. It contains everything that every project needs and nothing that belongs to a specific business:

- a clean, layered backend architecture;
- a complete **authentication** flow (sign-up with e-mail confirmation, resend of the confirmation link, login with JWT Bearer, roles);
- an Angular client with an application **shell (sidebar + navbar)**, a **real-time notification bell** (SignalR), Heroicons, the standard libraries already configured, **light/dark theme** and full **internationalization** (pt-BR, en-US, es-ES);
- **automated tests** for the backend (xUnit unit and integration tests) and the frontend (Vitest);
- **secrets kept out of the repository** with `dotnet user-secrets` and setup scripts.

To start a new project you only change the **project name** and **description**, and then implement your domain on top of the skeleton. **No business entities, use cases or screens are included on purpose.**

The instance in this repository is named **Salary360**: a project to organize, plan and manage the monthly salary, so that every salary received has a plan, a purpose and a defined destination. The business features of Salary360 are not part of the skeleton.

## What is included

| Area | Included |
| --- | --- |
| Architecture | Clean Architecture, DDD building blocks, CQRS (own lightweight dispatcher, no MediatR), dependency injection (IoC), centralized exception handling, structured JSON logging |
| Code organization | One type per file; interfaces live in `Interfaces/` and enums in `Enums/` folders (backend) or `interfaces/` and `enums/` (frontend), never inside class files |
| Authentication | ASP.NET Core Identity, JWT Bearer, sign-up with **e-mail confirmation**, **resend confirmation link**, lockout, rate limiting on auth endpoints |
| Authorization | Roles `Admin` and `User` and **role-based permissions** (`Resource.Action` with View/Create/Update/Delete) stored as role claims, copied into the JWT and enforced by one policy per permission |
| User data | Display name, **date of birth**, **phone**, **created at**, **updated at**, e-mail, roles; profile screen and admin user list |; **status** (Active/Inactive), **TermsAcceptedAt** and **PrivacyPolicyAcceptedAt** (`DateTimeOffset?`, set at sign-up when both are accepted), **ProfilePictureUrl**, and an optional **address** stored in a separate `UserAddress` entity (postal code, street, number, complement, neighborhood, city, state, country ISO code)
| Data | Entity Framework Core 10 + SQLite, migrations-ready, idempotent **seed** of the Admin and User accounts |
| E-mail | `IEmailSender` with SMTP (MailKit) and Log implementations, HTML **templates** for account creation and link resend |
| Real time | ASP.NET Core **SignalR** hub (`/hubs/notifications`, JWT via query string), `INotificationPublisher` abstraction in Application, `@microsoft/signalr` client with automatic reconnect |
| Frontend | Angular 22 (standalone components split into `.ts` / `.html` / `.scss`), Tailwind CSS, Chart.js, ngx-toastr, ngx-translate (pt-BR, en-US, es-ES), light/dark theme with persisted choice and system preference detection |
| Tests | xUnit unit tests, `WebApplicationFactory` integration tests (SQLite, fake e-mail sender, fake secrets) and Angular Vitest specs for services, interceptors, guards, validators, masks and components |
| Errors and security | RFC 9457 `ProblemDetails` with stable `code`s, FluentValidation on every input, native rate limiting (3 login attempts per 5 minutes) and Identity lockout, secrets in user-secrets |

## Repository layout

```text
Salary360/
├── Salary360Client/                 Angular 22 application
│   ├── public/i18n/                 pt-BR.json, en-US.json, es-ES.json
│   └── src/app/
│       ├── core/                    auth, API helpers, interceptors, language and theme services (each with interfaces/, enums/, constants/)
│       ├── layout/                  shell, sidebar, navbar, notification bell
│       ├── features/                login, register, check-email, confirm-email,
│       │                            resend-confirmation, home, admin
│       └── shared/                  icon (Heroicons), toolbar, language switcher, theme toggle, doughnut chart
└── Salary360Server/
    ├── src/
    │   ├── Salary360.API/           controllers, JWT setup, CORS, rate limiting, exception handler
    │   ├── Salary360.Application/   commands, queries, handlers, validators, Interfaces/, Settings/
    │   ├── Salary360.Domain/        Entity, AggregateRoot, DomainException, Interfaces/
    │   ├── Salary360.Infrastructure/ EF Core + SQLite, Identity, JWT, e-mail, seed, Interfaces/, Enums/
    │   └── Salary360.IoC/           composition root (dependency registration)
    ├── scripts/                     setup-secrets.sh / setup-secrets.ps1 (user-secrets)
    ├── tests/
    │   ├── Salary360.UnitTests/
    │   └── Salary360.IntegrationTests/
    └── Salary360.slnx
```

Dependencies between layers: `API → Application, IoC` · `IoC → Application, Infrastructure, Domain` · `Infrastructure → Application, Domain` · `Application → Domain` · `Domain → (nothing)`.

Frontend and backend are independent and can be versioned and deployed separately.

## Requirements

- .NET SDK 10
- Node.js 22+ and npm

## Default users (development only)

After the backend starts, the seed creates two confirmed accounts (when their passwords are configured):

| Role | E-mail | Password |
| --- | --- | --- |
| Admin | `admin@salary360.local` | defined by you (see [Secrets](#secrets)) |
| User | `user@salary360.local` | defined by you (see [Secrets](#secrets)) |

No password is stored in the repository. The e-mails and profile data of the seed accounts live in `Salary360.API/appsettings.json` (`Seed:Admin` / `Seed:User`); the passwords come from user-secrets or environment variables. Without a password the seed skips that account and logs a warning.

## Secrets

Nothing sensitive is committed: `Jwt:Secret`, `Seed:Admin:Password`, `Seed:User:Password` and `Email:Password` are empty in every `appsettings*.json`. The API has a `UserSecretsId`, so in `Development` they are read from the .NET user-secrets store on your machine.

```bash
cd Salary360Server
./scripts/setup-secrets.sh          # Windows: ./scripts/setup-secrets.ps1
```

The script generates a random `Jwt:Secret` and asks for the two seed passwords (empty input generates a random one that is printed once). The equivalent manual commands:

```bash
dotnet user-secrets set "Jwt:Secret" "<random 32+ characters>" --project src/Salary360.API
dotnet user-secrets set "Seed:Admin:Password" "<password>" --project src/Salary360.API
dotnet user-secrets set "Seed:User:Password" "<password>" --project src/Salary360.API
dotnet user-secrets list --project src/Salary360.API
```

Outside `Development`, use environment variables with the same names (`Jwt__Secret`, `Seed__Admin__Password`, `Email__Password`, ...); `Salary360Server/.env.example` lists them without values. The application refuses to start without a valid `Jwt:Secret`. The `.gitignore` already excludes `.env*`, `secrets.json`, `*.db` and certificates. If the API runs behind a trusted reverse proxy, set `ASPNETCORE_FORWARDEDHEADERS_ENABLED=true` so the rate limiter sees the real client IP. The Angular app holds no secrets, only the public API URL.

## Getting started

### Backend

Configure the [secrets](#secrets) once, then:

```bash
cd Salary360Server
dotnet restore
dotnet build
dotnet run --project src/Salary360.API      # http://localhost:5080  (opens Swagger UI at /swagger)
```

In `Development`, Swagger UI is available at `http://localhost:5080/swagger`. To call protected endpoints, sign in with `POST /api/auth/login` using one of the [seed accounts](#default-users-development-only), click **Authorize** and paste the returned `accessToken`.

In `Development`, the e-mail provider is `Log`: the confirmation e-mail (with its link) is written to the console instead of being sent, so the whole flow works without an SMTP server. The seed accounts are listed in [Default users](#default-users-development-only).

### Frontend

```bash
cd Salary360Client
npm install
npm start          # http://localhost:4200
npm run build
```

## Theme and internationalization

- **Theme:** `ThemeService` toggles the `dark` class on `<html>` (Tailwind `dark:` variant). The choice is stored in `localStorage`; on the first visit the operating system preference is used. The toggle is in the toolbar of every screen and the Chart.js legend follows the theme.
- **Languages:** `pt-BR`, `en-US`, `es-ES` in `Salary360Client/public/i18n/`. The app waits for the selected language to load before rendering, so translation keys are never shown. Every visible text, validation message and API error (including ASP.NET Identity errors, mapped by code under `errors.identity.*`) comes from these files. The project description shown on the home screen is the `app.description` key.

## Application shell and real-time notifications

- **Layout:** after sign-in every page renders inside `Shell`: a **sidebar** (navigation, role-aware, **expandable/collapsible** to an icon-only rail on desktop with the choice persisted, drawer on mobile) and a **navbar** (sidebar toggle button, notification bell, theme toggle, language selector, user and sign-out). Auth screens stay outside the shell. Add menu entries in `layout/sidebar.ts` (`NAV_ITEMS`).
- **Icons:** [Heroicons](https://heroicons.com) (the Tailwind CSS icon set, outline 24) through the `<app-icon name="bell" />` component. Add an icon by copying its SVG path(s) into `shared/icon-paths.ts`.
- **Notifications (SignalR):** the backend publishes through `INotificationPublisher` (`SendToUserAsync`, `BroadcastAsync`); the API implements it with `IHubContext<NotificationsHub>`. Users are targeted by the JWT `sub` claim. The browser cannot set headers on WebSockets, so the hub accepts the JWT as the `access_token` query string. The Angular `NotificationService` connects when the shell opens, disconnects when it closes, keeps the list and the unread counter as signals, and the bell shows them. Notification texts are translated by `type` (`notifications.types.<type>.title|message`), falling back to the text sent by the server. On the home screen, **Send test notification** exercises the whole path.

## Permissions per role

Permissions follow the `Resource.Action` format and live in `AppPermissions` (Application layer). The seed creates the roles and keeps their permissions in sync (adds missing ones, removes obsolete ones).

| Resource | Action | Admin | User |
| --- | --- | :---: | :---: |
| Profile | View, Update | ✔ | ✔ |
| Users | View, Create, Update, Delete | ✔ | ✘ |
| Roles | View, Create, Update, Delete | ✔ | ✘ |

At login the API returns `roles` and `permissions`, and the JWT carries one `permission` claim for each. Controllers use `[Authorize(Policy = AppPermissions.Users.View)]`; the Angular app uses `permissionGuard` and `AuthService.hasPermission(...)`. The admin screen (`/admin`) shows the roles/permissions matrix, editable only by users with `Roles.Update` (the `Admin` role is locked; changes apply at the next login, and the seed only sets permissions when a role is first created). The Dashboard (`/`, full-width) holds the summary cards (total, active, inactive, administrators), the user list and a sample chart (`Users.View` for the user parts). The app uses a custom thin scrollbar in both themes. To add a resource, add its constants and the role mapping in `AppPermissions`, then protect your endpoints with the matching policy.

## User experience features

- **Remember me:** the login checkbox keeps the session in `localStorage` and remembers the e-mail (never the password). Unchecked, the session lives in `sessionStorage`.
- **Inactive accounts:** an inactive user cannot sign in (`403` with code `user_inactive`, shown as a toast). Each request also checks that the token's user still exists and is active, so deactivated or deleted users are rejected (401) and the client signs out.
- **Profile:** `/profile` is a single tabbed card (Personal data, Address, Account). Personal data and Address are editable; Account shows e-mail-bound info, roles, status and dates. Read-only fields are rendered as disabled (dashed, greyed) inputs.
- **Form components (Tailwind, no native pickers):** `app-date-picker` (typed mask by language, `dd/mm/yyyy` or `mm/dd/yyyy`, plus a calendar with month and year navigation and min/max), `app-select` (searchable dropdown with optional flags, also used for the language switcher and the address country), and `app-phone-input`. Every input has a placeholder and a `maxlength`; required labels show a red asterisk and optional ones an "(optional)" tag.
- **Masks and limits:** postal codes are masked per selected country (e.g. `00000-000` for BR, `00000-0000` for US, `A0A 0A0` for CA) and validated for completeness; the phone mask and the maximum number of digits follow the selected country.
- **International phone input:** a country selector (flag, name, dialing code, searchable) applies the correct mask per country with `libphonenumber-js`; the value is stored in E.164 (e.g. `+5581999990001`). Flags come from `flag-icons` (4x3 SVGs copied as assets).
- **Error pages:** `/error/403`, `/error/404`, `/error/500` (and a generic one). Unknown routes go to 404, missing permission to 403, and server failures to 500.
- **Loading:** `app-spinner` is used in a global route-loading overlay, inside submit buttons, in pages that fetch data, and in the `index.html` boot screen.
- **Birthday:** on the user's birthday, a confetti and cake animation is shown once per day and a cake icon stays in the navbar (click it to replay). To try it, set your date of birth to today on the profile screen.

> **Database note:** the schema is created with `EnsureCreated`, which does not alter an existing SQLite file. After pulling model changes (new user columns, role claims), delete the development database file `salary360.db` (or add EF Core migrations) so it is recreated.

## Running the tests

```bash
cd Salary360Server && dotnet test        # unit + integration tests
cd Salary360Client && npm test           # Angular (Vitest) specs
```

The integration tests start the API in the `Testing` environment with a temporary SQLite file, a fake e-mail sender and fake secrets provided in memory, so they never read your user-secrets or touch real services.

## Errors, validation and rate limiting

Every error response is an RFC 9457 `ProblemDetails` JSON with `status`, `title`, a stable snake_case `code`, `traceId` and, for validation, `errors` (field to list of codes). The API never sends texts meant for end users: the Angular `errorInterceptor` translates the `code` into the user's language and picks the proper feedback.

| Status | `code` | Frontend feedback |
| --- | --- | --- |
| 400 | `validation_failed` | Messages under each invalid input plus one warning toast |
| 401 | `invalid_credentials` | Message in the form and an error toast; a 401 in an authenticated call logs out with an info toast |
| 403 | `email_not_confirmed` | Offer to resend the confirmation link |
| 403 | `user_inactive`, `account_locked` | Error toast (locked accounts show the time to wait) |
| 403 | `forbidden` | `/error/403` on navigation, warning toast on actions |
| 404 | `not_found` | `/error/404` on navigation, warning toast on actions |
| 409 | `conflict` | Warning toast (e.g. e-mail already registered) |
| 422 | e.g. `cannot_deactivate_self` | Warning toast |
| 429 | `too_many_requests` | Warning toast, login button disabled with a countdown (`Retry-After`) |
| 5xx | `internal_error` | Generic error toast (and `/error/500` on navigation); details only in the logs |

**Validation:** the Angular forms validate for convenience, but FluentValidation validates every command and DTO on the server before its handler runs, so requests that bypass the UI are rejected with the same limits (lengths, e-mail, password policy, E.164 phone, http(s) picture URL, birth date, consents, country code).

**Rate limiting** (`RateLimiting` section): login allows **3 attempts per 5 minutes per IP + e-mail** (the 4th gets `429` with `Retry-After`) and 20 per 5 minutes per IP; sign-up, confirmation and resend allow 5 per 10 minutes per IP; every other call is limited to 100 per minute per user or IP. Identity also locks an account for 5 minutes after 3 wrong passwords (`403 account_locked`).

## Authentication flow

1. `POST /api/auth/register` creates the account with the `User` role and an **unconfirmed** e-mail, then sends the confirmation e-mail. It does not return a token.
2. The e-mail link points to the Angular app (`{Client:BaseUrl}/confirm-email?userId=…&token=…`), which calls `POST /api/auth/confirm-email`.
3. `POST /api/auth/login` works only for confirmed e-mails. Otherwise it answers `403` with `code: "email_not_confirmed"` and the login screen offers to resend the link.
4. `POST /api/auth/resend-confirmation` sends a new link. It **always answers `202`**, whether or not the e-mail exists or is already confirmed, so it cannot be used to discover registered users. A minimum interval between resends and IP rate limiting apply.

The confirmation token is valid for 24 hours.

| Method | Route | Access | Description |
| --- | --- | --- | --- |
| POST | `/api/auth/register` | Public | Sign-up and confirmation e-mail (`201`) |
| POST | `/api/auth/confirm-email` | Public | Activates the account (`userId`, `token`) |
| POST | `/api/auth/resend-confirmation` | Public | Sends a new link (always `202`) |
| POST | `/api/auth/login` | Public | Returns a JWT with the user roles |
| GET | `/api/auth/me` | Bearer + `Profile.View` | Profile of the authenticated user |
| PUT | `/api/auth/me` | Bearer + `Profile.Update` | Updates name, date of birth, phone, profile picture URL and address (`null` removes the address) |
| GET | `/api/users` | Bearer + `Users.View` | Lists users |
| GET | `/api/roles` | Bearer + `Roles.View` | Lists roles with their permissions |
| PUT | `/api/users/{id}/status` | Bearer + `Users.Update` | Activates or deactivates a user (you cannot deactivate yourself) |
| PUT | `/api/roles/{name}/permissions` | Bearer + `Roles.Update` | Replaces the permissions of a role (the `Admin` role is locked) |
| POST | `/api/notifications/test` | Bearer | Pushes a test notification to the caller through SignalR |
| WS | `/hubs/notifications` | Bearer | SignalR hub; clients receive `ReceiveNotification` |
| GET | `/health` | Public | Health check |
| GET | `/swagger` | Public (Development) | Swagger UI, with an **Authorize** button for the JWT |
| GET | `/openapi/v1.json` | Public (Development) | OpenAPI document |

## Configuration

Main settings in `Salary360.API/appsettings.json`:

| Section | Purpose |
| --- | --- |
| `Project` | Project name and description (used in the OpenAPI document) |
| `ConnectionStrings:DefaultConnection` | SQLite database |
| `Jwt` | `Issuer`, `Audience`, `ExpirationMinutes`; `Secret` (minimum 32 characters) comes from user-secrets |
| `RateLimiting` | `LoginPermitLimit`, `LoginWindowMinutes`, `LoginIpPermitLimit`, `RegisterPermitLimit`, `RegisterWindowMinutes`, `ApiPermitLimit` |
| `Client:BaseUrl` | URL of the Angular app, used to build the e-mail links |
| `Email` | `Provider` (`Log` or `Smtp`), `Host`, `Port`, `Username`, `FromAddress`, `FromName`, `ResendIntervalSeconds` |
| `Seed` | E-mail, display name, password, date of birth, phone (E.164), profile picture URL and address (`Address` section) of the Admin and User accounts; both are created active with terms and privacy policy accepted |
| `Cors:AllowedOrigins` | Origins allowed to call the API |

**Secrets never go in source control.** Provide `Jwt__Secret`, `Email__Password`, `Seed__Admin__Password` and `Seed__User__Password` through environment variables or user-secrets. Outside `Development`, the application does not start without a JWT secret, and a seed account is skipped when its password is empty.

### Database and migrations

Without migrations, the schema is created from the model on first run. To use migrations:

```bash
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialCreate -p src/Salary360.Infrastructure -s src/Salary360.API
```

From then on, startup applies pending migrations automatically.

### E-mail templates

HTML templates live in `src/Salary360.Infrastructure/Email/Templates/` (`account-confirmation.html` and `resend-confirmation.html`) and are embedded in the assembly. Customize the layout and text freely; the available placeholders are `{{ProjectName}}`, `{{UserName}}` and `{{ConfirmationLink}}` (values are HTML-encoded). Subjects are defined in `ConfirmationEmailService`.

## Using this skeleton for a new project

1. Copy the repository and rename `Salary360` in folders, `.csproj` files, the `.slnx`, namespaces and the Angular project/package name.
2. Set the new name and description in:
   - `Salary360.API/appsettings.json` → `Project`, `Jwt:Issuer`, `Jwt:Audience`, `Email:FromName`, seed e-mails;
   - `Salary360Client/src/environments/*.ts` → `appName`;
   - `Salary360Client/public/i18n/*.json` → `app.description`;
   - this `README.md`.
3. Adjust the e-mail templates to your brand.
4. Add your domain: new feature folders in Domain, Application, Infrastructure and API (use `Auth` in the Application layer as a model), and new routes under `Salary360Client/src/app/features/`.

## Design notes

- **CQRS without MediatR:** `IDispatcher` resolves the handler, runs the FluentValidation validators and logs the execution. Request payloads are never logged because they may contain credentials.
- **Identity stays in the Infrastructure layer.** Domain and Application only know `IIdentityService` and `ICurrentUser`.
- **Errors:** a single exception handler converts exceptions into RFC 9457 problem responses; on the client, a single interceptor turns failed calls into translated toasts.
- **No `Directory.Build.props`:** shared project properties are repeated in each `.csproj` by design.
- NuGet packages use floating versions (`10.0.*`, `12.*`, …) to follow the latest patch; pin them (or enable a lock file) when the project stabilizes.
- `ngx-toastr` still declares an Angular 21 peer dependency; `package.json` contains an `overrides` entry that aligns it with Angular 22.
- In production, **persist the ASP.NET Core Data Protection keys** (volume or shared storage). Otherwise confirmation links already sent stop working after every container restart.
