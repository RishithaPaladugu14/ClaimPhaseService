# Claim Phase Middleware Service (.NET 8 Web API)

A lightweight REST middleware that takes a **claim control number**, looks up its
**validation type**, and returns the **current phase** — but only when that phase is
approved for self-service disclosure.

Everything runs on static in-memory data, so there is no database to install.

---

## 1. What the service does

```
caller ──▶ GET /api/v1/claims/{controlId}/current-phase  (JWT required)
             │
             ├─ 1. find the control number         (ControlId  varchar(20))
             ├─ 2. get its validation type         (ValidationTypeId varchar(10))
             ├─ 3. get that type's phases          (1 validation type : many phases)
             ├─ 4. keep the one with IsCurrent = 1
             ├─ 5. drop it if ApprovedForSelfServiceDisclosure = 0
             └─ 6. return only the minimum fields
```

### Data model (the future SQL tables, today just C# lists)

| CLAIM_CONTROL | type |
|---|---|
| ControlId | varchar(20), primary key |
| ValidationTypeId | varchar(10) |

| VALIDATION_PHASE | type |
|---|---|
| ValidationTypeId | varchar(10) |
| PhaseCode | OPEN / INTAKE / APPEAL / UPHELD / OVERTURNED / DENIED / SIU_REVIEW / CLOSED |
| PhaseDescription | text |
| IsCurrent | bit — exactly one per validation type |
| ApprovedForSelfServiceDisclosure | bit |
| EffectiveDate | date |

### Mock data you can test with

| Control number | Validation type | Current phase | Result |
|---|---|---|---|
| CTRL-2024-0000001 | VT-CLM-001 | OPEN | 200 OK |
| CTRL-2024-0000002 | VT-CLM-002 | APPEAL | 200 OK |
| CTRL-2024-0000003 | VT-CLM-003 | UPHELD | 200 OK |
| CTRL-2024-0000004 | VT-CLM-004 | SIU_REVIEW | **404** (not disclosable) |
| CTRL-2024-0000005 | VT-CLM-005 | CLOSED | 200 OK |
| anything else | – | – | 404 |

> Why 404 and not 403 for CTRL-2024-0000004? Returning the same answer for
> "unknown claim" and "claim you may not see" stops callers from probing which
> control numbers exist.

---

## 2. Project layout

```
ClaimPhaseService.sln
src/ClaimPhaseService.Api/
  Program.cs                       app start-up, JWT setup, Swagger, middleware order
  Controllers/AuthController.cs    POST /api/v1/auth/token
  Controllers/ClaimPhaseController.cs  GET  /api/v1/claims/{controlId}/current-phase
  Services/PhaseLookupService.cs   the business rules (steps 1-6 above)
  Data/StaticClaimDataStore.cs     the mock data - replace with SQL later
  Security/TokenService.cs         issues the JWT
  Security/IpAllowListMiddleware.cs  corporate IP/CIDR restriction
  Models/                          request + response shapes
tests/ClaimPhaseService.Tests/     22 xUnit tests (unit + in-memory API tests)
postman/                           ready-to-import Postman collection
```

---

## 3. Run it on your PC

Prerequisite: [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
(check with `dotnet --version`). Visual Studio 2022 or VS Code both work.

```bash
# one time, so the local HTTPS certificate is trusted
dotnet dev-certs https --trust

dotnet restore
dotnet run --project src/ClaimPhaseService.Api --launch-profile https
```

Then open <https://localhost:7297/swagger>.

In Visual Studio: open `ClaimPhaseService.sln`, pick the **https** profile, press F5.

---

## 4. How to test it (three ways)

### A. Swagger UI — easiest

1. Go to <https://localhost:7297/swagger>.
2. Expand **POST /api/v1/auth/token**, click *Try it out*, and send:
   ```json
   { "clientId": "self-service-portal", "clientSecret": "portal-dev-secret" }
   ```
3. Copy the `accessToken` value from the response.
4. Click the green **Authorize** button at the top right, paste the token, click *Authorize*, then *Close*.
   (Paste the raw token only — Swagger adds the word `Bearer` for you.)
5. Expand **GET /api/v1/claims/{controlId}/current-phase**, click *Try it out*,
   enter `CTRL-2024-0000002`, and *Execute*. You should get:
   ```json
   {
     "controlId": "CTRL-2024-0000002",
     "validationTypeId": "VT-CLM-002",
     "phaseCode": "APPEAL",
     "phaseDescription": "Appeal submitted and under review",
     "effectiveDate": "2024-02-10"
   }
   ```
6. Try `CTRL-2024-0000004` → 404, and try again after clicking *Logout* in the
   Authorize dialog → 401. Those two are the security checks doing their job.

### B. Postman

1. Postman → *Import* → choose `postman/ClaimPhaseService.postman_collection.json`.
2. Run request **1. Get token** first — a script stores the token in the
   `accessToken` collection variable automatically.
3. Run requests 2-6. Or hit *Run collection* to run all of them at once and see
   the pass/fail tests.
4. If Postman complains about the certificate: *Settings → General → SSL certificate
   verification → Off* (fine for localhost only).

### C. Automated tests

```bash
dotnet test
```

22 tests: business-rule unit tests, CIDR matching tests, and in-memory HTTP tests
that boot the real API and check 200/401/404 plus that internal flags never leak.

---

## 5. Security features and how to switch them on

| Requirement | Where it lives | Notes |
|---|---|---|
| HTTPS | `app.UseHttpsRedirection()` + HSTS in `Program.cs` | HTTP requests are redirected |
| JWT authentication | `Program.cs` `AddJwtBearer`, `Security/TokenService.cs` | HMAC-SHA256, 30 min expiry |
| Authorization | policy `PhaseRead` requires the `claims.phase.read` scope | a valid token alone is not enough |
| IP / CIDR restriction | `Security/IpAllowListMiddleware.cs` | off by default, see below |
| Minimal disclosure | `Models/PhaseLookupResponse.cs` | `IsCurrent` and the approval flag are never serialised |

Turn on the IP restriction in `appsettings.json`:

```json
"IpAllowList": {
  "Enabled": true,
  "AllowedRanges": [ "127.0.0.1", "::1", "10.0.0.0/8", "203.0.113.14" ]
}
```

Requests from outside those ranges get 403 before authentication even runs.
Behind a load balancer or reverse proxy you also need `UseForwardedHeaders`,
otherwise every request looks like it comes from the proxy.

### Before this ever goes to production

- Move `Jwt:SigningKey` and the client secrets out of `appsettings.json` into
  user-secrets, environment variables, or a vault (`dotnet user-secrets set "Jwt:SigningKey" "..."`).
- Store client secrets hashed, not in plain text.
- Replace the local token endpoint with your real identity provider (Entra ID, Okta, ...)
  and validate its issuer instead.
- Add rate limiting (`builder.Services.AddRateLimiter`) and an audit log of lookups.

---

## 6. Swapping the static data for SQL later

`PhaseLookupService` only depends on the `IClaimDataStore` interface. Write a
`SqlClaimDataStore` that implements the same two methods, then change one line in
`Program.cs`:

```csharp
builder.Services.AddScoped<IClaimDataStore, SqlClaimDataStore>();
```

No controller or business-rule code has to change, and the existing tests keep working.

---

## 7. Endpoint reference

| Method | Path | Auth | Purpose |
|---|---|---|---|
| POST | `/api/v1/auth/token` | none | exchange client credentials for a JWT |
| GET | `/api/v1/claims/{controlId}/current-phase` | JWT + `claims.phase.read` | the main lookup |
| GET | `/health` | none | liveness probe |
