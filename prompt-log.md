# Prompt Log — PayLink

Stage: **Requirements** (Phase 1). Each entry follows the structure required in §5 of the brief: role, context, constraints, output format and, where useful, an example. Full prompts are in the appendix below the table.

| # | Date | Author | Stage | Tool and model | Prompt | Summary of answer | Verdict (accepted, fixed, rejected) and why |
|---|---|---|---|---|---|---|---|
| P-01 | 2026-11-25 | _[name]_ | Requirements — discovery | Claude Opus 5.5 | [P-01](#p-01) | Product framing, goals, success indicators, risks, scope, 44 ambiguity questions — plus an unrequested epic/story skeleton (38 titles). | **Fixed.** (1) Ignored the constraint "Do NOT write epics or stories yet". (2) Q-19 recommended adding FundsHeld → Failed, which violates FR-11 ("no other transitions"). (3) Invented numeric targets not in the brief (≥ 98% success, < 0.5% Unknown, ≤ 1% drop). (4) Described PayLink as an existing system ("Today a payment can fail…"), but the brief describes a new backend. (5) Answered "ask the business" for Q-16, Q-22, Q-36. Kept: the ambiguity list, risks and scope. |
| P-02 | 2026-11-25 | _[name]_ | Requirements — decisions | Claude Opus 5.5 | [P-02](#p-02) — improved prompt after P-01 | Decision log: 15 main decisions + 14 appendix decisions, impact on state diagram, ER entities and config keys; revised success indicators. | **Accepted with fixes.** All 5 corrections from P-01 applied; no transitions beyond §2.5. Gaps the team found: missing AuditRecord and PaymentStateHistory entities, no TOPUP ledger type, D-13 could fail every Unknown payment when a settlement file is late, D-06 left walk-in customers open. |
| P-03 | 2026-11-25 | _[name]_ | Requirements — decisions | Claude Opus 5.5 | [P-03](#p-03) | Updated D-06, D-09, D-11, D-13, D-26; 23-entity list; ledger types; risk R6. | **Accepted.** All 7 fixes applied correctly. |
| P-04 | 2026-11-25 | _[name]_ | Requirements — stories | Claude Opus 5.5 | [P-04](#p-04) | Stakeholder table; 7 E4 Payment Processing stories with Given/When/Then; coverage check; 5 open questions. | **Fixed.** (1) No MoSCoW priority on any story, although the brief requires priorities. (2) A performance target (100 payments/s) written as an acceptance criterion; it belongs in the NFRs. (3) Template drift ("I need to" instead of "I want"). Kept: ACs, hard cases #1, #2, #4. |
| P-05 | 2026-11-25 | _[name]_ | Requirements — stories | Claude Opus 5.5 | [P-05](#p-05) — improved prompt after P-04 | D-30 – D-34; E4 priorities; 17 stories for E1, E2, E3, E5 (+1 Phase 2); coverage check; 2 open questions. | **Accepted with one fix.** Priorities, NFR split and template consistency corrected. US-5.04 used the role Admin for adding integration code; changed to PayLink Management. |
| P-06 | 2026-11-25 | _[name]_ | Requirements — stories | Claude Opus 5.5 | [P-06](#p-06) | D-35, D-36; 12 stories for E6 – E11; full FR and hard-case coverage (36 stories); 3 open questions. | **Accepted, priorities adjusted by the team.** Hard cases #5 and #7 sat in Should stories (US-8.02, US-10.01), but §2.7 requires a test for every hard case, so the product lead raised both to Must. OQ-9 also exposed that the AI's key name `max_attempts` conflicted with the brief's "retried 3 times". |
| P-07 | 2026-11-25 | _[name]_ | Requirements — NFRs & traceability | Claude Opus 5.5 | [P-07](#p-07) | D-37 – D-39; 10 NFRs; traceability matrix; final check (36 stories, 19/19 Musts, 28/28 FRs, 8/8 hard cases). | **Accepted.** Counts verified by hand (Must 29 / Should 5 / Could 2; boundary values 4,852.50 / 5,000.00 / 5,152.50; commission 1.00). 8 missing decision cross-references added by the team during assembly. |

**Summary (Phase 1):** 7 prompts · 2 fixed after a wrong first answer (P-01 → P-02, P-04 → P-05) · 5 accepted (3 of them with team corrections) · 0 rejected. 54 questions raised by the AI were all decided by the team (D-01 – D-39); the AI's own recommendation was overridden 5 times (Q-19, Q-16, Q-22, Q-36, invented targets).

---

## Appendix — full prompts

### P-01

```
ROLE
You are a senior Product Owner and requirements engineer with deep
experience in fintech payments (e-wallets, bill payments, agent networks)
in emerging markets.

CONTEXT
Attached is the brief for "PayLink", a bill-payment and wallet backend
(university project, but treat it as a real product). I am the product
lead. Later we will write epics, user stories with Given/When/Then
acceptance criteria, and measurable NFRs. This step is discovery only.

TASK
Do NOT write epics or stories yet. Produce:
1. Product framing
   - Problem statement (3 lines): who has the problem, what it is,
     why it matters
   - Business goals: what PayLink and management want (revenue, trust,
     regulation, scalability), each linked to a section of the brief
   - Success indicators: how we would know the product works
     (business and quality), each measurable
   - Key risks: top 5 product/business risks (money, trust,
     integration, compliance), with impact (High/Med/Low)
2. Scope
   - In scope / Out of scope / Assumptions, based only on the brief
3. Ambiguities
   - Every gap, ambiguity or contradiction in the brief that would
     change a requirement or a test

CONSTRAINTS
- Use only facts from the brief; mark anything else as [ASSUMPTION]
- Use the brief's exact terms (Hold, FundsHeld, Unknown, Ledger,
  Idempotency key)
- For ambiguities: ask, don't decide. Give options and a recommendation
  only
- Cite the brief section for each item (e.g. §2.6)
- Be concise: bullets and tables, no essays

OUTPUT FORMAT
## 1. Product framing (bullets + risk table)
## 2. Scope (3 short lists)
## 3. Ambiguities, as a table:
| # | Question | Brief § | Why it matters (impact on requirements/tests) | Option A | Option B | Recommendation |

EXAMPLE ROW
| Q-01 | Does the daily limit include the service fee? | §2.6 | Changes
when FR-18 rejects a payment and the boundary tests | Amount only |
Amount + fee | B: the limit protects the wallet's total outflow |
```

### P-02

```
Good discovery pack. Before we move on, I'm applying my review as
product lead. Treat everything below as fixed.

CORRECTIONS
1. You broke my constraint "Do NOT write epics or stories yet".
   Section 4 is parked as a draft; don't change or extend it in
   this step.
2. Q-19: your recommendation adds FundsHeld → Failed, which violates
   FR-11 ("no other transitions"). Never add transitions beyond §2.5.
3. Problem statement: replace yours with mine (below). Also note the
   brief describes a NEW backend, not a current system.
4. Success indicators: remove any target not given in the brief, or
   mark it "Team-defined" with a one-line rationale.
5. We are the business. Never answer "ask the business"; give a
   decision.

MY PROBLEM STATEMENT
<paste your 3–4 lines>

FIXED DECISIONS (already made)
- Q-19: no new transition. A disabled or invalid biller is caught
  before the hold (Initiated → Rejected). A refused connection after
  the hold → SentToBiller → Failed (biller definitely not reached).
- Q-21: an FR-27 refund is a separate "Reversal" record linked to the
  payment. The payment stays Completed. "Refunded" is used only for
  the Failed → Refunded state.
- Q-27: the 10 s timeout applies to TopUpNet's "accepted" reply. The
  callback wait has its own configurable timeout, then the payment
  goes to Unknown.

TASK
Turn the 44 questions into a Decision Log:
1. MAIN TABLE, max 15 decisions: only those that change a
   requirement, an acceptance test, the state diagram or the ER
   model. Include Q-19, Q-21 and Q-27 as given above.
2. APPENDIX TABLE: all remaining questions, with a short decision.
3. Decide Q-16, Q-22 and Q-36 yourself, with a rationale.

CONSTRAINTS
- Decisions must stay inside the brief (§2.5 transitions, §2.6
  values, FR priorities). If a decision needs an assumption, mark it
  [ASSUMPTION].
- Every value that might change goes to configuration; name the
  config key (e.g. limits.daily.basic).
- Use the brief's exact terms. Use "Reversal" for FR-27.
- No epics or stories in this step.

OUTPUT FORMAT
## Decision Log — main
| D-ID | From Q | Decision | Rationale | Brief § | Affects (Req / Test / State diagram / ER / Config key) |

## Decision Log — appendix
| D-ID | From Q | Decision | Brief § |

## Impact summary
- Changes to the state diagram (should be none beyond §2.5)
- New or renamed entities for the ER model (e.g. Reversal, Quote)
- Config keys introduced (list)

EXAMPLE ROW
| D-01 | Q-01 | The daily limit counts amount + fee | The hold is
amount + fee (§2.2 step 5), so the limit caps total outflow | §2.6,
§2.2 | Req FR-18; boundary tests at 5,000/30,000; config
limits.daily.basic, limits.daily.verified |
```

### P-03

```
Good decision log. I'm accepting it with these fixes. Apply them and
return ONLY the changed parts.

FIXES
1. ER entities: add AuditRecord (FR-25) and PaymentStateHistory
   (FR-20 "full history"). Then list the FULL entity set, including
   implied ones (Customer, Agent, Wallet, LedgerEntry, Biller, Service,
   Payment...). We need ≥ 10 tables.
2. Ledger entry types: add TOPUP and COMMISSION_REVERSAL.
3. D-13: an Unknown payment that is missing from the file becomes
   Failed ONLY if that biller's settlement file for that business day
   was received. If the file is late or missing, the payment stays
   Unknown and is flagged.
4. D-09: state explicitly that a refused connection is treated as the
   §2.5 event "Biller refuses" because it is a definite non-receipt.
5. D-06: decision: agents can pay only for registered customers.
   Walk-in non-registered customers are out of scope.
6. D-11: add a risk row: "R6 PayLink funds Reversals from its own
   account while the biller keeps the money | Money | Med | FR-27".
7. D-26: add "Adapters are excluded from the coverage gate but must
   still have unit tests (error mapping, piaster conversion)."

OUTPUT
- Updated rows only: D-06, D-09, D-11, D-13, D-26
- Full ER entity list: | Entity | Purpose | Key fields | Brief § / D-ID |
- Updated Ledger entry types
- Updated risk table (R1–R6)
```

### P-04

```
Next step. Use the decision log (D-01 to D-29) as fixed rules.

TASK 1: Stakeholders
Table: Stakeholder | Type (primary user / secondary / external system /
regulator) | Goals | Key concerns | Epics they touch.
Include: Customer, Agent, Support Employee, Admin, Billers (each mock),
PayLink Management/Finance, Regulator, SMS/Push providers.

TASK 2: User stories for Epic E4 Payment Processing ONLY
Cover the draft titles US-14 to US-20 (renumber them US-4.01 to
US-4.07). Split or merge only if INVEST requires it, and say why.

RULES
- "As a / I want / so that". Roles: Customer, Agent, Support Employee,
  Admin, System only. Use System only when no human role fits.
- ≥ 1 happy-path AC and ≥ 1 negative or edge AC per story, in
  Given/When/Then.
- Use exact values (352.50, 5,000, 10 s) and name the config key.
- Rejections use error codes (e.g. DAILY_LIMIT_EXCEEDED,
  QUOTE_EXPIRED, INSUFFICIENT_FUNDS).
- Only §2.5 transitions. Money as exact decimals, 2 dp.
- Reference the decisions (D-xx) and hard cases (§2.7 #) that each
  story satisfies.
- Every hard case owned by E4 (#1, #2, #4) must appear as an AC.
- No implementation details (no table names, no frameworks).

FORMAT: exactly this template and example:
<paste your template + example story>

AFTER THE STORIES
- Coverage check: FRs covered (FR-9, 10, 11, 12, 13), hard cases
  covered (#1, #2, #4), stories without a negative AC (should be none)
- Open questions, if any (don't decide them; list them)
```

### P-05

```
Good. Apply these, then continue.

DECISIONS ON YOUR OPEN QUESTIONS (add as D-30 to D-34)
- D-30 (OQ-1): re-query delays are measured from entering Unknown
  (1, 5, 15 min).
- D-31 (OQ-2): a re-query is a status inquiry using the payment's
  unique reference; it never re-sends the payment. Our mock billers
  will expose a status operation [ASSUMPTION: we write the mocks].
- D-32 (OQ-3): a late synchronous answer after the timeout is applied
  as "Biller later confirms/refuses" (Unknown → Completed/Failed) and
  logged.
- D-33 (OQ-4): keep idempotency.ttl_hours = 24.
- D-34 (OQ-5): a Rejected payment gets a reference and appears in the
  transaction history and support search.

FIXES TO E4
1. Every story needs "Priority: Must/Should/Could" from the FR it
   implements. Return only a table: | Story | Priority | FR |.
2. Move US-4.07 AC3 (100 payments/s) out of the stories. It goes to
   the NFR section later.
3. Use "I want" (not "I need to") from now on.

TASK: write the stories for E1 Accounts & Wallet, E2 Biller Catalog,
E3 Inquiry & Fees and E5 Biller Integration, with the same rules and
format as E4, plus a Priority field.
- Reframe technical titles as user value (e.g. "Customer pays a water
  bill" instead of "AquaCo adapter"). Story US-07 (Ledger) can stay a
  System story.
- US-26 (ElectroCo v2): Priority Must, marked "Phase 2"; it doesn't
  count toward the Phase 1 total.
- Hard cases #3, #6 and #8 must each appear as an AC.
- Fee ACs use exact §2.6 values: 2.50 fixed; 1.5% with a 1 EGP
  minimum; internet 2/4 at the 200.00 boundary; promotion → 0.
- Keep it compact: max 4 ACs per story.

END WITH
- Coverage check: FRs covered, hard cases #3, #6, #8, stories without
  a negative AC
- A running total of stories (E1–E5, excluding Phase 2)
- Open questions only if they block a test
```

### P-06

```
Accepted. Apply these, then continue.

DECISIONS (add to the Decision Log)
- D-35 (OQ-6): billers.timeout.live_seconds (10) also applies to bill
  inquiry. No answer → inquiry rejected with BILLER_UNAVAILABLE, no
  Quote created.
- D-36 (OQ-7): registration requires phone number, name and password
  only. No national ID. All test data is fictional (§5).

FIX
- US-5.04: change the role from Admin to PayLink Management. Return
  only the changed lines.

TASK: write the stories for E6 Fraud & Limits, E7 Agents,
E8 Notifications, E9 Support, Audit & Reversal, E10 Reconciliation &
Reporting and E11 Configuration. Same rules and format as E1–E5,
including Priority, max 4 ACs per story.
Draft titles to cover:
- E6: reject over the single/daily limit with a reason (FR-18);
  velocity block (FR-19); support blocks/unblocks an account (FR-21)
- E7: agent pays for a customer and earns commission (FR-17)
- E8: SMS + push on Completed and Failed (FR-22); retry 3 times then
  undelivered (FR-23)
- E9: support searches a payment by reference and sees its full
  history (FR-20); audit log of every money movement and admin action
  (FR-25); Reversal of a Completed payment (FR-27)
- E10: daily reconciliation report (FR-26); admin daily totals (FR-28)
- E11: admin edits fee and limit values without a redeploy (FR-24)

RULES
- Hard cases #5 and #7 must each appear as an AC.
- Fraud checks run as a chain before the Hold, and a rejected payment
  is Initiated → Rejected with a code (DAILY_LIMIT_EXCEEDED,
  AMOUNT_BELOW_MIN, AMOUNT_ABOVE_MAX, VELOCITY_BLOCKED,
  ACCOUNT_BLOCKED).
- Use "Reversal" (not refund) for FR-27, and keep the payment
  Completed (D-11).
- Commission example: fee 2.50 → commission 1.00 (40%).

END WITH
1. Running total of Phase 1 stories, all epics E1–E11
2. Full FR coverage: FR-1 to FR-28, each mapped to a story, with any
   gap flagged
3. All 8 hard cases mapped to a story AC
4. Open questions only if they block a test
```

### P-07

```
Accepted. Apply these, then finish.

DECISIONS
- D-37 (OQ-8): the fraud chain runs in this order: ACCOUNT_BLOCKED
  (manual) → VELOCITY_BLOCKED → AMOUNT_BELOW_MIN / AMOUNT_ABOVE_MAX →
  DAILY_LIMIT_EXCEEDED. The first failing rule stops the chain and its
  code is returned. INSUFFICIENT_FUNDS is checked at the Hold, after
  the chain.
- D-38 (OQ-9): "retried 3 times" means 3 retries after the first
  attempt (4 attempts in total). Rename the config key to
  notifications.retry.max_retries = 3. Update US-8.02 AC1.
- D-39 (OQ-10): FR-28 totals show Reversals and commission clawbacks as
  separate lines, with both gross and net figures.

PRIORITY CHANGES (product lead decision)
- US-8.02 and US-10.01 → "Must (hard case)", because the brief requires
  an automated test for every hard case in §2.7.

TASK 1: NFRs (≥ 6, from §3.3, plus Team-defined ones only if useful)
| ID | Quality | Requirement | Target (number) | How measured (test
or tool) | When (CI / load test / review) | Linked stories |
Include the throughput test moved out of US-4.07
(100 payments/s for 60 s).

TASK 2: Final traceability matrix
| FR | Priority | Stories | Hard case | NFR |
covering FR-1 to FR-28, plus a row for the ElectroCo v2 change
request (Phase 2).

TASK 3: Final check (numbers only)
- Phase 1 stories total, and stories per priority
- Musts covered x/x, FRs covered x/28, hard cases x/8, NFRs x
- Stories without a negative or edge AC
- Any decision (D-01 to D-39) that no story references

Return ONLY: the updated US-8.02 AC1, the D-37 to D-39 rows, and
Tasks 1 to 3.
```
