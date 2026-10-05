# TeamFlow

Team/project management SaaS: **ASP.NET Core 8 API** (`TeamFlow.API`) + **React 19 / Vite** client (`teamflow-client`).

## Run locally

### API
```bash
cd TeamFlow.API
dotnet run --launch-profile https      # https://localhost:8018, Swagger at /swagger
```
In Development the API applies EF migrations automatically on start (SQL Server, see `ConnectionStrings:DefaultConnection`).
Elsewhere run `dotnet ef database update` as a deployment step.

### Client
```bash
cd teamflow-client
cp .env.example .env     # VITE_API_BASE_URL=https://localhost:8018/api
npm install
npm run dev              # http://localhost:5173
npm run lint && npm run build
```

## Configuration (API)
| Key | Purpose |
|---|---|
| `Jwt:Key` / `Jwt:Issuer` / `Jwt:Audience` | Token signing. **Outside Development the app refuses to start with the sample key** – set `Jwt__Key` (32+ chars) via environment/secret store. |
| `ConnectionStrings:DefaultConnection` | SQL Server |
| `Cors:AllowedOrigins` | Array of allowed client origins |
| `Client:BaseUrl` | Used to build invitation links |

## Billing flow
`GET /api/plans` (public, shown on sign-up) → register with `planId` → account starts on Free → paid plan is checked out at `/billing?checkout=<id>` → `POST /api/billing/change-plan`. Only the Owner can change plan or see payments; downgrades are refused while usage exceeds the new limits.

## Roles
`Owner` > `Admin` > `Member`. Owner/Admin manage projects, members and invitations; Members only see and work in projects they belong to.

## Known limitations
* `FakeEmailService` only logs (Debug level) the invitation link – plug a real `IEmailService` in before production.
* Payments use a **simulated gateway** (`MockPaymentGateway`, test cards 4242 4242 4242 4242 = approved, 4000 0000 0000 0002 = declined). Swap it for Stripe/Paymob by implementing `IPaymentGateway` (use hosted fields/tokens so card numbers never touch the API). Paid plans last 1 month with no auto-renewal; `SubscriptionExpiryService` moves expired orgs back to Free.
* A user belongs to one organization per login (first membership is used).
* No automated tests exist yet.
