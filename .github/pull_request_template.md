## What and why
<!-- One or two sentences. Link the issue / user story: Closes #12 (US-09) -->

## Module(s) touched
- [ ] Accounts and Wallet
- [ ] Biller Catalog
- [ ] Payment Processing
- [ ] Biller Integration
- [ ] Fees and Commissions
- [ ] Notifications
- [ ] Fraud Checks
- [ ] Reconciliation
- [ ] Infrastructure (Docker, CI, docs)

## How it was tested
<!-- Tests added/changed. For TDD features: link the failing-test commit, then the code commit. -->

## AI use
- Tool and model:
- Did AI write more than half of this change? yes / no  (if yes, the commit message names the tool)
- Prompt-log entry added in `docs/prompt-log.md`? yes / no

## Checklist
- [ ] `dotnet build` and `dotnet test` pass locally
- [ ] `dotnet format --verify-no-changes` passes
- [ ] Money uses `decimal`, never `double`/`float`
- [ ] Business numbers are in configuration, not hard-coded
- [ ] I understand and can explain every line in this PR
