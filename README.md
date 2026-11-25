# PayLink

Bill-payment and wallet backend: customers pay electricity, water, mobile and internet bills from a digital wallet.
Built with **.NET 8**, **PostgreSQL 16**, **Docker Compose** and **GitHub Actions**.

<!-- Replace OWNER/REPO with your GitHub repository path. -->
[![CI](https://github.com/OWNER/REPO/actions/workflows/ci.yml/badge.svg)](https://github.com/OWNER/REPO/actions/workflows/ci.yml)

## Run it (two commands)

Requires Docker Desktop (or Docker Engine with the Compose plugin).

```bash
git clone <repository-url> paylink && cd paylink
docker compose up --build
```

Then open:

| What | URL |
|---|---|
| API docs (Swagger UI) | http://localhost:5080/swagger |
| API liveness | http://localhost:5080/health/live |
| API readiness (database) | http://localhost:5080/health/ready |
| Loaded modules | http://localhost:5080/api/system/info |
| Mock billers | http://localhost:5090 |

Stop with `Ctrl+C`; remove containers and data with `docker compose down -v`.
No `.env` file is needed. To change ports or passwords, copy `.env.example` to `.env`.

## Develop without Docker

Requires the .NET 8 SDK (or newer) and a PostgreSQL instance (or run only the database: `docker compose up db`).

```bash
dotnet build PayLink.sln                                   # build (warnings are errors)
dotnet test PayLink.sln                                    # run all tests
dotnet format PayLink.sln --verify-no-changes              # lint, same check as CI
dotnet run --project src/PayLink.Api                       # API on http://localhost:5080
dotnet run --project mocks/PayLink.MockBillers --urls http://localhost:5090
```

Tests with coverage (same as CI):

```bash
dotnet test PayLink.sln --settings coverage.runsettings --collect "XPlat Code Coverage" --results-directory TestResults
```

## Architecture

A **modular monolith**: one deployable API, split into modules that each own their code, tests and data,
and talk to each other only through public interfaces. Architecture tests fail the build if one module references another.

| Module | Responsibility |
|---|---|
| Accounts and Wallet | Customers, agents, verification levels, wallets, ledger, holds, top-ups, refunds |
| Biller Catalog | Billers, services, input fields, min/max amounts |
| Payment Processing | Payment life cycle, idempotency, orchestration |
| Biller Integration | One adapter per biller; timeout, retry, callbacks |
| Fees and Commissions | Fixed, percentage, tiered and promotional fees; agent commission |
| Notifications | SMS, email, push with retry |
| Fraud Checks | Daily limit, velocity, blocked accounts (chain of rules) |
| Reconciliation | Nightly settlement-file comparison and difference report |

## Documentation

- [DevOps foundation: layout, pipeline, branch rules, team workflow](docs/devops/README.md)
- Requirements: `docs/requirements.md`
- Design diagrams: `docs/design/`
- Architecture decisions: `docs/adr/`
- AI prompt log: `docs/prompt-log.md`
