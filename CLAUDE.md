# PayLink: context for AI assistants

This file gives an AI assistant (Claude Code or a Claude chat) the context to continue work on this project.
Read it fully before making changes. Last updated: 2026-11-25.

## 1. What the project is

University year-work project: **PayLink**, a bill-payment and wallet backend (like Fawry / Vodafone Cash, Egypt).
Customers pay electricity, water, mobile and internet bills from a wallet in EGP, directly or through agents.
The full student brief is the PDF `PayLink Year Work Project - Student Brief` (ask the user to attach it if you need details).

Key facts from the brief:
- **Billers (mock servers we write):** ElectroCo (REST/JSON, fast; in Phase 2 becomes v2: renamed fields, amounts in piasters, signed requests; only its adapter may change), AquaCo (SOAP-style XML, 2-4 s, rejects over-payment), TopUpNet (async: "accepted" then callback, may call twice), NetCo (daily CSV batch, optional).
- **Payment states:** Initiated → FundsHeld | Rejected; FundsHeld → SentToBiller; SentToBiller → Completed | Failed | Unknown; Unknown → Completed | Failed; Failed → Refunded. Completed, Rejected, Refunded are final. Unknown is never refunded automatically.
- **Business rules (must live in configuration):** daily limit 5,000 EGP basic / 30,000 verified; single payment 5-10,000 EGP; fee 2.50 fixed (electricity, water), 1.5% min 1 EGP (mobile), 2 EGP ≤ 200 else 4 EGP (internet); promotion = 0 fee for one biller in a date range; agent commission 40% of fee; velocity > 5 payments / 60 s blocks for 10 min; biller timeout 10 s → Unknown; Unknown re-queried after 1, 5, 15 min; notifications retried 3 times then undelivered; money uses `decimal`, never floating point.
- **8 hard cases, each needs an automated test:** duplicate request (idempotency), 12 s timeout → Unknown, AquaCo error → Failed + refund + SMS, concurrent payments never make balance negative, SMS provider down, duplicate TopUpNet callback, settlement file vs Failed payment flagged, new biller touches only integration + config.
- **Technical minimums:** ≥ 6 modules, ≥ 25 user stories, ≥ 6 measurable NFRs, relational DB with ≥ 10 tables + migrations in repo, ≥ 20 REST endpoints in OpenAPI, 5 design patterns, modular monolith then event-driven Notifications + Reconciliation, Dockerfile per component, one compose file, CI that builds, lints, tests, reports coverage (≥ 70% domain), builds images, deploys locally.

## 2. Deadlines and grading

| Phase | Due | Points | Contents |
|---|---|---|---|
| 1 | Wed 25 Nov 2026 | 30 | Requirements (10), design diagrams (10), Docker + CI (5), prompt log (5) |
| 2 | Fri 18 Dec 2026 | 60 | Working system, TDD, patterns, ADRs, ElectroCo v2, deployment, report, demo |
| Discussion | from 19 Dec 2026 | 10 | Each student alone: live change + questions |

Submit by tagging the repo (`phase-1`, `final`) and uploading one PDF. The TA grades the tagged commit.

## 3. Repository

- GitHub: https://github.com/Zienabkhalifa453/PayLink (public, the owner's choice)
- Stack (fixed after Phase 1): **.NET 8 Web API, PostgreSQL 16, xUnit, Docker Compose, GitHub Actions**

```
src/PayLink.Api/            API host: Swagger, /health/live, /health/ready (DB), /api/system/info, module wiring
src/PayLink.SharedKernel/   IModule interface (Name, DisplayName, AddModule, MapEndpoints)
src/Modules/PayLink.Modules.{Accounts,Catalog,Payments,BillerIntegration,Fees,Notifications,Fraud,Reconciliation}
                            one project per module, currently empty shells; modules must NOT reference each other
mocks/PayLink.MockBillers/  mock billers service (only health endpoint so far)
tests/PayLink.Api.IntegrationTests/   3 tests (WebApplicationFactory)
tests/PayLink.ArchitectureTests/      4 tests (module boundary rules)
docker-compose.yml          db (5432) + mock-billers (5090) + api (5080); works with no .env
.github/workflows/ci.yml    job 1 "Build, lint, test"; job 2 "Docker images and stack smoke test"
docs/devops/README.md       structure, pipeline, branch rules, team workflow
docs/prompt-log.md          AI prompt log (entries P1-P9)
docs/report/phase-1-report.html   Phase 1 report source; print to PDF with Edge headless
```

Build rules: `Directory.Build.props` sets TreatWarningsAsErrors + EnforceCodeStyleInBuild; NuGet versions only in `Directory.Packages.props`; lint = `dotnet format PayLink.sln --verify-no-changes`.

Verify locally:
```
dotnet build PayLink.sln
dotnet format PayLink.sln --verify-no-changes
dotnet test PayLink.sln
```

## 4. Branches and rules

- **`main`** is protected: pull request required (1 approval), required checks `Build, lint, test` and `Docker images and stack smoke test`, no bypass. **Never push to main directly.**
- **`devops`** is the integration branch. Work goes into `devops` (directly or through feature branches); deliver with a PR `devops` → `main`.
- Other branches: `docs/prompt-log-and-report` (already merged into devops, can be deleted), `MennaShawkat-Design_Pack` (teammate's design work; should be merged into devops).

## 5. Conventions you MUST follow

1. **No `Co-Authored-By: Claude` trailers** (or any Claude attribution lines) in commits or PRs. The owner must appear as the only contributor.
2. **Keep the AI-use note in the commit subject** when AI wrote more than half of a change, short form: `(AI: Claude)`. The brief (§5) requires naming the AI tool; do not remove it.
3. Add every significant AI interaction to `docs/prompt-log.md` with an honest verdict (Accepted / Fixed / Rejected and why).
4. Money is `decimal`; business numbers go in configuration, not code.
5. Commit or push only when the user asks; never to `main`.
6. The user prefers simple, clear English explanations with step-by-step instructions.

## 6. Phase 1 status

| Item | Status |
|---|---|
| Docker + CI skeleton | Done, pipeline green |
| Branch protection on main | Done and verified through the GitHub API |
| `devops` branch workflow | Done |
| Prompt log | Done (9 entries, 2 improved prompts) |
| Report | HTML + PDF draft with yellow "TO ADD" boxes: team names/IDs/roles, Requirements section, Design section, screenshots |
| **Requirements pack** `docs/requirements.md` | **Not started.** Needs: problem summary, stakeholders, ≥ 25 user stories with given/when/then (FR-1 … FR-28; do not drop any "Must"), ≥ 6 measurable NFRs, priorities, AI questions/ambiguities with decisions |
| **Design pack** `docs/design/` | **Check the `MennaShawkat-Design_Pack` branch first.** Needs PlantUML/Mermaid source: use-case, domain class, sequence "pay a bill", sequence "refund a failed payment", payment state, ER (≥ 10 tables), all consistent |
| `phase-1` tag | **Not created.** Create it on `main` only after requirements + design are merged through devops → main |

## 7. Next steps (in order)

1. Write `docs/requirements.md` on `devops`.
2. Merge the design branch into `devops`; review the diagrams for consistency with the requirements.
3. Fill the report's TO ADD boxes, rebuild the PDF.
4. Add prompt-log entries for the new work.
5. PR `devops` → `main`, teammate approves, merge.
6. `git tag phase-1` on main, push it, confirm the pipeline is green on the tag, upload the PDF.
