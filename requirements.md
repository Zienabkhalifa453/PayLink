---
title: "PayLink — Requirements Pack"
subtitle: "Phase 1 · Advanced Software Engineering · Year Work Project"
---

| | |
|---|---|
| **Project** | PayLink — bill-payment and wallet backend |
| **Document** | Requirements Pack (Phase 1) |
| **Version** | 1.0 |
| **Date** | 25 November 2026 |
| **Status** | Final for `phase-1` tag |
| **Team** | _[Team name / members]_ |

**Document control**

| Version | Date | Change |
|---|---|---|
| 0.1 | 24–25 Nov 2026 | Discovery: framing, scope, 44 ambiguity questions |
| 0.2 | 25 Nov 2026 | Decision log D-01 – D-36, stakeholders, user stories E1 – E11 |
| 1.0 | 25 Nov 2026 | Decisions D-37 – D-39, NFRs, traceability, final check |

**Contents**

1. Problem summary
2. Business goals and success indicators
3. Stakeholders
4. Scope and assumptions
5. Key risks
6. Conventions
7. Epic overview
8. User stories
9. Non-functional requirements
10. Questions, ambiguities and decisions
11. Traceability matrix
12. Final check
- Appendix A — Configuration keys
- Appendix B — Ledger entry types and data entities (input to the design pack)

\newpage

# 1. Problem summary

PayLink lets people in Egypt pay their electricity, water, mobile and internet bills from a digital wallet, either in the app or at an agent's shop. The happy path is simple. The real problem is everything around it: a biller that answers late, an app that sends the same payment twice, two payments hitting the same wallet at once, an SMS provider that's down, or a settlement file that doesn't match our records.

Because PayLink holds customers' money, each of these failures can turn into a double charge, a negative balance, or a payment the biller never received. That costs trust, money and possibly regulator attention.

We are building a **new backend** that:

- is **correct with money**: every movement is a ledger entry, nothing is charged twice, and nothing is refunded before we're sure the biller didn't get it;
- **absorbs new billers and biller API changes** by changing only the integration and configuration, not the core;
- **keeps working on the busiest day of the month**, even when one biller is down.

# 2. Business goals and success indicators

**Business goals**

| Goal | What it means for PayLink | Brief § |
|---|---|---|
| Revenue | Service fee on each payment; grow the agent network through commission | §2.1, §2.6 |
| Trust | No double charges, no lost payments | §2.1, §2.7 #1, #4 |
| Regulation | Full ledger and audit trail of customer money | §2.1, FR-5, FR-25 |
| Scalability | Handle the busiest day of the month | §2.1, §3.3 |
| Extensibility | Add a biller or absorb an API change without touching the core | §2.4, §2.7 #8 |
| Biller satisfaction | Pay billers fast and correctly; reconcile nightly | §2.2 step 9, FR-26 |
| Operational control | Change fees and limits without redeploying; support tools to search and block | FR-20, FR-21, FR-24 |

**Success indicators**

| Indicator | Target | Source |
|---|---|---|
| Double charges / negative balances | 0 | §3.3 |
| Throughput | 100 payments/s for 1 min | §3.3 |
| Audit coverage of money movements | 100% | §3.3 |
| Domain logic coverage | ≥ 70% | §3.3 |
| Hard-case tests passing | 8/8 | §2.7 |
| Files changed to add a biller or apply ElectroCo v2 | Integration + config only | §2.4, §3.3 |
| Other billers' success rate during one biller's outage | ±1% of baseline (team-defined) | §3.3 |
| Unknown payments older than the escalation threshold | 100% flagged (team-defined) | §2.5 |
| Fee and commission accuracy against §2.6 test vectors | 100% (team-defined) | §2.6 |

# 3. Stakeholders

| Stakeholder | Type | Goals | Key concerns | Epics |
|---|---|---|---|---|
| Customer | Primary user | Pay bills fast without queuing | Double charges, money stuck in Unknown, clear rejection reasons | E1, E3, E4, E6, E8, E9 |
| Agent | Primary user | Serve walk-in cash customers and earn commission | Correct commission, wallet liquidity, clawbacks | E1, E4, E7 |
| Support Employee | Secondary | Resolve complaints and differences quickly | Full payment history, blocking tools, Reversal approval | E6, E9, E10 |
| Admin | Secondary | Run the catalog, fees and limits without redeploys | Config mistakes, audit of changes | E2, E10, E11 |
| ElectroCo (mock) | External system | Fast REST payments; v2 API with signing | Reference uniqueness, v2 migration | E5 |
| AquaCo (mock) | External system | XML payments, never above the bill amount | Slow replies (2–4 s), error-code mapping | E5 |
| TopUpNet (mock) | External system | Accept now, confirm by callback later | Duplicate or conflicting callbacks | E5 |
| NetCo (mock) | External system | Daily batch file | Out of scope (D-29) | — |
| PayLink Management / Finance | Secondary | Fee revenue, trust, readiness for peak day | Fee income, cost of Reversals (R6), daily totals | E3, E7, E10 |
| Regulator | Regulator | Customer money protected and traceable | Ledger integrity, audit completeness | E1, E9 |
| SMS / Push providers | External system | Deliver messages | Outages must not block payments (§2.7 #5) | E8 |

# 4. Scope and assumptions

**In scope**

- Six core modules: Accounts & Wallet, Biller Catalog, Payment Processing, Biller Integration, Fees & Commissions, Notifications; plus Fraud Checks and Reconciliation (§3.1).
- FR-1 to FR-28 with Must/Should/Could priorities (§3.2).
- Mock billers ElectroCo, AquaCo and TopUpNet (§2.4).
- ElectroCo v2 change request in Phase 2 (§2.4).
- Payment life cycle (§2.5) and business rules held in configuration (§2.6).
- The 8 hard cases, each with an automated test (§2.7).
- Technical requirements of §3.4 (REST + OpenAPI, async flow, relational DB, patterns, Docker, CI/CD).
- A simple web page or CLI client (§3.4).

**Out of scope**

- A full UI (marks are for the backend, §3.4).
- Real card processing — card top-up is simulated (FR-4).
- Real billers — mocks only (§2.4).
- NetCo daily batch (optional in FR-15; D-29).
- Cloud deployment — only a one-page comparison in Phase 2.
- Currencies other than EGP.
- Walk-in, non-registered customers at agents (D-06).

**Assumptions**

- SMS, push and email providers are mocks.
- KYC is not real; "verified" is a status set by an admin (D-16).
- Token-based authentication with roles CUSTOMER, AGENT, SUPPORT, ADMIN (D-21).
- All "days" and times are in Africa/Cairo (D-01).
- One PayLink legal entity, with internal ledger accounts for fee income and commission.
- The team writes the mock billers, including a status operation for re-queries (D-31).

# 5. Key risks

| # | Risk | Area | Impact | § |
|---|---|---|---|---|
| R1 | Double charge or negative balance under retries or concurrent requests | Money / Trust | High | §2.7 #1, #4 |
| R2 | Refunding a payment the biller actually received | Money | High | §2.5, §2.7 #7 |
| R3 | Long-lived Unknown holds look like lost money to customers | Trust | High | §2.5, §2.6 |
| R4 | Gaps in the ledger or audit trail of money held in wallets | Compliance | High | §2.1, FR-5, FR-25 |
| R5 | Biller differences leak into the core, or one biller's outage hits the others | Integration | Med | §2.4, §3.3 |
| R6 | PayLink funds Reversals from its own account while the biller keeps the money | Money | Med | FR-27 |

# 6. Conventions

- **IDs:** epics `E1`–`E11`; stories `US-<epic>.<nn>`; acceptance criteria `AC<n>`; decisions `D-<nn>`; NFRs `NFR-<nn>`; risks `R<n>`; hard cases `#<n>` (§2.7).
- **Roles:** Customer, Agent, Support Employee, Admin, PayLink Management, System (only when no human role fits).
- **Terms:** exactly as in the brief — Hold, FundsHeld, SentToBiller, Unknown, Ledger, Idempotency key, Quote. **Reversal** = FR-27 return of a Completed payment; **Refunded** = the state after Failed only.
- **Priorities:** MoSCoW, inherited from the FR. "Must (hard case)" = raised by the product lead because §2.7 requires an automated test.
- **Money:** EGP, exact decimals, 2 dp, HALF_UP. Never floating point.
- **Rejections:** returned as error codes (e.g. `DAILY_LIMIT_EXCEEDED`), which become the API error contract.
- **Values:** every §2.6 number is a configuration key (Appendix A), never hard-coded.
- **Running example:** Sara, basic account, ElectroCo bill 350.00 + fee 2.50 = 352.50 EGP.

# 7. Epic overview

| Epic | Goal | FRs | Hard cases | Stories |
|---|---|---|---|---|
| E1 Accounts & Wallet | Customers, wallets, top-ups; balances from the ledger | FR-1–5 | — | 7 |
| E2 Biller Catalog | Admin manages billers; customers see what to enter | FR-6, 7 | — | 3 |
| E3 Inquiry & Fees | Show amount, fee and total before paying; correct fees | FR-8, 16 | — | 3 |
| E4 Payment Processing | Pay safely: life cycle, idempotency, holds, timeouts | FR-9–13 | #1, #2, #4 | 7 |
| E5 Biller Integration | Pay each biller despite different protocols | FR-14, 15 | #3, #6, #8 | 4 (+1 Phase 2) |
| E6 Fraud & Limits | Limits, velocity and blocks as a chain of rules | FR-18, 19, 21 | — | 3 |
| E7 Agents | Agents pay for customers and earn commission | FR-17 | — | 1 |
| E8 Notifications | SMS + push with retry | FR-22, 23 | #5 | 2 |
| E9 Support, Audit & Reversal | Search, audit trail, Reversals | FR-20, 25, 27 | — | 3 |
| E10 Reconciliation & Reporting | Nightly reconciliation; daily totals | FR-26, 28 | #7 | 2 |
| E11 Configuration | Change fees and limits without redeploying | FR-24 | — | 1 |
| **Total (Phase 1)** | | **28/28** | **8/8** | **36** |

\newpage

# 8. User stories

## E1 — Accounts & Wallet

### US-1.01 — Customer registers
| Priority | FR | Decisions | Hard case |
|---|---|---|---|
| Must | FR-1 | D-01, D-36 | — |

**As a** Customer, **I want** to register and get a wallet, **so that** I can start paying bills.

*Paths:* valid details → basic account plus an empty wallet. Alternative: phone already registered; required field missing.

- **AC1 (happy)** — **Given** a phone number that isn't registered, **when** Sara registers with phone, name and password, **then** her account is BASIC, her wallet shows available 0.00 and held 0.00, and `limits.daily.basic` (5,000) applies.
- **AC2 (negative)** — **Given** the phone number is already registered, **when** someone registers with it, **then** registration is rejected with `PHONE_ALREADY_REGISTERED`.
- **AC3 (negative)** — **Given** a required field (phone, name or password — D-36) is missing, **when** the form is submitted, **then** it is rejected with `VALIDATION_ERROR` naming the field.

*Dependencies:* US-1.07

### US-1.02 — Admin upgrades a customer to verified
| Priority | FR | Decisions | Hard case |
|---|---|---|---|
| Should | FR-2 | D-02, D-16, D-21, D-22 | — |

**As an** Admin, **I want** to upgrade a customer to verified, **so that** trusted customers can pay larger bills each day.

*Paths:* BASIC → VERIFIED. Alternative: caller isn't an admin; customer already verified.

- **AC1 (happy)** — **Given** Sara is BASIC, **when** an admin upgrades her, **then** she is VERIFIED, her daily limit becomes `limits.daily.verified` (30,000), and an audit record holds who, when and why.
- **AC2 (edge)** — **Given** Sara is VERIFIED, **when** she pays a bill of 10,000.01, **then** the payment is rejected, because the single-payment max stays 10,000 (D-02).
- **AC3 (negative)** — **Given** a support employee, **when** they try to upgrade Sara, **then** the request is rejected with `FORBIDDEN` (D-21).
- **AC4 (negative)** — **Given** Sara is already VERIFIED, **when** an admin upgrades her again, **then** the request is rejected with `ALREADY_VERIFIED`.

*Dependencies:* US-1.01, US-9.02

### US-1.03 — Customer views their balance
| Priority | FR | Decisions | Hard case |
|---|---|---|---|
| Must | FR-3, FR-5 | — | — |

**As a** Customer, **I want** to see my available and held balance, **so that** I know what I can spend and what is pending.

*Paths:* balances computed from the ledger. Alternative: brand-new wallet; request for another customer's wallet.

- **AC1 (happy)** — **Given** Sara's ledger has a TOPUP of 1,000.00 and a HOLD of 352.50, **when** she views her balance, **then** she sees available 647.50 and held 352.50.
- **AC2 (edge)** — **Given** a new wallet, **when** she views it, **then** she sees available 0.00 and held 0.00.
- **AC3 (negative)** — **Given** Sara is logged in, **when** she asks for another customer's balance, **then** the request is rejected with `FORBIDDEN`.

*Dependencies:* US-1.07

### US-1.04 — Customer views transaction history
| Priority | FR | Decisions | Hard case |
|---|---|---|---|
| Must | FR-3 | D-34 | — |

**As a** Customer, **I want** to see my payments and top-ups, **so that** I can check what happened to my money.

*Paths:* list newest first. Alternative: no transactions; Rejected payments; another customer's history.

- **AC1 (happy)** — **Given** Sara has a Completed payment of 352.50 and a top-up of 1,000.00, **when** she opens her history, **then** she sees both, newest first, each with reference, type, amount, fee, total, state and date.
- **AC2 (edge)** — **Given** a payment was Rejected with `INSUFFICIENT_FUNDS`, **then** it appears with its reference and that reason (D-34).
- **AC3 (edge)** — **Given** no transactions, **then** she sees an empty list, not an error.
- **AC4 (negative)** — **Given** Sara is logged in, **when** she asks for another customer's history, **then** the request is rejected with `FORBIDDEN`.

*Dependencies:* US-1.07, E4

### US-1.05 — Customer tops up by card
| Priority | FR | Decisions | Hard case |
|---|---|---|---|
| Must | FR-4 | D-07, D-08, D-18 | — |

**As a** Customer, **I want** to top up my wallet with a (simulated) card, **so that** I have money to pay bills.

*Paths:* card accepted → wallet credited. Alternative: amount out of range; account blocked; duplicate request.

- **AC1 (happy)** — **Given** Sara has available 0.00, **when** she tops up 500.00 by card, **then** her available balance is 500.00, a TOPUP entry is recorded, and no fee is charged.
- **AC2 (negative)** — **Given** `topup.min` = 5.00 and `topup.max` = 10,000.00, **when** she tops up 4.99 or 10,000.01, **then** the top-up is rejected with `TOPUP_BELOW_MIN` or `TOPUP_ABOVE_MAX`.
- **AC3 (negative)** — **Given** her account has a MANUAL block, **when** she tops up, **then** the top-up is rejected with `ACCOUNT_BLOCKED`.
- **AC4 (edge)** — **Given** a top-up of 500.00 with key K2 succeeded, **when** the same request with K2 is resent, **then** her wallet is credited 500.00 only once.

*Dependencies:* US-1.07, US-4.02

### US-1.06 — Agent tops up a customer with cash
| Priority | FR | Decisions | Hard case |
|---|---|---|---|
| Must | FR-4 | D-06, D-07 | — |

**As an** Agent, **I want** to take cash and top up a registered customer's wallet, **so that** customers without cards can use PayLink.

*Paths:* agent wallet debited, customer wallet credited. Alternative: agent short of funds; customer not registered.

- **AC1 (happy)** — **Given** the agent wallet has 2,000.00, **when** the agent tops up Sara with 500.00, **then** the agent wallet shows 1,500.00 and Sara's +500.00, with no fee and no commission.
- **AC2 (negative)** — **Given** the agent wallet has 300.00, **when** the agent tops up 500.00, **then** the top-up is rejected with `INSUFFICIENT_FUNDS`.
- **AC3 (negative)** — **Given** the phone number is not registered, **when** the agent tops up, **then** the top-up is rejected with `CUSTOMER_NOT_FOUND`.

*Dependencies:* US-1.01, US-1.07

### US-1.07 — Balances come only from the ledger
| Priority | FR | Decisions | Hard case |
|---|---|---|---|
| Must | FR-5, FR-25 | D-13 | — |

**As the** System, **I want** every balance change to be a ledger entry and every balance to be the sum of its entries, **so that** money can always be traced and never silently changed.

*Paths:* each movement adds an entry. Alternative: attempt to edit or delete an entry; correction needed.

- **AC1 (happy)** — **Given** TOPUP 1,000.00, HOLD 352.50 and CHARGE 352.50 entries, **when** the balance is read, **then** it equals the sum of the entries: available 647.50, held 0.00.
- **AC2 (negative)** — **Given** an existing entry, **when** anyone tries to change or delete it, **then** the attempt is refused with `LEDGER_IMMUTABLE`.
- **AC3 (edge)** — **Given** a correction is needed, **then** it is made only with a new ADJUSTMENT entry that has an audit record.

*Dependencies:* US-9.02

## E2 — Biller Catalog

### US-2.01 — Admin adds a biller and its services
| Priority | FR | Decisions | Hard case |
|---|---|---|---|
| Must | FR-6 | D-02, D-21, D-22 | — |

**As an** Admin, **I want** to add a biller with its services, required input fields and min/max amounts, **so that** customers can pay it.

*Paths:* valid biller → visible to customers. Alternative: range outside global limits; duplicate.

- **AC1 (happy)** — **Given** an admin, **when** they add AquaCo / Water with input field "account number" and range 5.00–10,000.00, **then** the service appears in the customer catalog and an audit record is written.
- **AC2 (negative)** — **Given** the global range `limits.payment.min` 5.00 to `limits.payment.max` 10,000.00, **when** a service is added with min 2.00 or max 12,000.00, **then** it is rejected with `RANGE_OUTSIDE_GLOBAL`.
- **AC3 (negative)** — **Given** AquaCo exists, **when** it is added again, **then** the request is rejected with `BILLER_ALREADY_EXISTS`.

*Dependencies:* US-5.04

### US-2.02 — Admin edits or disables a biller
| Priority | FR | Decisions | Hard case |
|---|---|---|---|
| Must | FR-6 | D-09, D-19, D-20 | — |

**As an** Admin, **I want** to edit or disable a biller or service, **so that** customers can't pay a biller that is down or withdrawn.

*Paths:* disable → hidden, new payments stopped. Alternative: payments in flight; non-admin caller.

- **AC1 (happy)** — **Given** ElectroCo is enabled, **when** an admin disables it (`billers.electroco.enabled` = false), **then** it disappears from the catalog, and new inquiries and payments are rejected with `BILLER_DISABLED`.
- **AC2 (edge)** — **Given** an ElectroCo payment in Unknown, **when** ElectroCo is disabled, **then** its re-queries and reconciliation continue.
- **AC3 (edge)** — **Given** a Quote was issued with service max 10,000.00, **when** the admin lowers the max to 3,000.00, **then** the change applies to new Quotes only.
- **AC4 (negative)** — **Given** a customer, **when** they try to disable a biller, **then** the request is rejected with `FORBIDDEN`.

*Dependencies:* US-2.01

### US-2.03 — Customer browses billers
| Priority | FR | Decisions | Hard case |
|---|---|---|---|
| Must | FR-7 | — | — |

**As a** Customer, **I want** to see the billers and the input each one needs, **so that** I know what to enter before paying.

*Paths:* list of enabled billers with fields. Alternative: disabled service; unknown service.

- **AC1 (happy)** — **Given** ElectroCo, AquaCo and TopUpNet are enabled, **when** Sara opens the catalog, **then** she sees each one with its required field: meter number, account number, phone number.
- **AC2 (edge)** — **Given** a disabled service, **then** it is not listed.
- **AC3 (negative)** — **Given** a service ID that doesn't exist, **when** its fields are requested, **then** the request is rejected with `SERVICE_NOT_FOUND`.

*Dependencies:* US-2.01

## E3 — Inquiry & Fees

### US-3.01 — Customer sees what they owe before paying
| Priority | FR | Decisions | Hard case |
|---|---|---|---|
| Must | FR-8 | D-03, D-35 | — |

**As a** Customer, **I want** to see the amount, fee and total before I pay, **so that** I'm never surprised by a charge.

*Paths:* inquiry → Quote. Alternative: mobile top-up (customer enters amount); invalid input; biller unavailable.

- **AC1 (happy)** — **Given** a valid ElectroCo meter number, **when** Sara runs an inquiry, **then** she sees 350.00 + 2.50 = 352.50 in a Quote that expires after `quote.ttl_minutes` (15).
- **AC2 (edge)** — **Given** TopUpNet, **when** Sara enters 100.00 for her phone number, **then** the Quote shows 100.00 + 1.50 = 101.50.
- **AC3 (negative)** — **Given** a meter number in the wrong format, or one the biller doesn't know, **then** the inquiry is rejected with `INVALID_INPUT` or `BILL_NOT_FOUND`, and no Quote is created.
- **AC4 (negative)** — **Given** the biller doesn't answer within `billers.timeout.live_seconds` (10), **then** the inquiry is rejected with `BILLER_UNAVAILABLE` and no Quote is created (D-35).

*Dependencies:* US-2.03, US-3.02, E5

### US-3.02 — Customer is charged the correct fee
| Priority | FR | Decisions | Hard case |
|---|---|---|---|
| Must | FR-16 | D-03, D-04, D-20 | — |

**As a** Customer, **I want** the fee to follow PayLink's published rules, **so that** I pay a fair, predictable fee.

*Paths:* fee by bill type. Alternative: mobile minimum; rounding; internet tier boundary; fee change after the Quote.

- **AC1 (happy)** — **Given** `fees.electricity.fixed` = `fees.water.fixed` = 2.50, **when** a bill of 350.00 is quoted, **then** the fee is 2.50.
- **AC2 (edge)** — **Given** `fees.mobile.rate` = 0.015 and `fees.mobile.min` = 1.00, **then** 100.00 → 1.50; 50.00 → 1.00 (minimum applies); 75.30 → 1.13 (HALF_UP).
- **AC3 (edge)** — **Given** `fees.internet.threshold` = 200.00, **then** 200.00 → 2.00 and 200.01 → 4.00.
- **AC4 (negative)** — **Given** a Quote with a fee of 2.50, **when** an admin changes the fee to 3.00 before Sara pays, **then** she is still charged 2.50.

*Dependencies:* US-11.01

### US-3.03 — Customer pays no fee during a promotion
| Priority | FR | Decisions | Hard case |
|---|---|---|---|
| Must | FR-16 | D-04, D-05 | — |

**As a** Customer, **I want** a zero fee for the biller on promotion, **so that** I benefit from PayLink offers.

*Paths:* promoted biller inside the period → fee 0. Alternative: other biller; period boundaries; agent commission.

- **AC1 (happy)** — **Given** `fees.promotion.biller_id` = AquaCo and a period of 1–8 Dec (Cairo time), **when** Sara quotes a water bill of 200.00 on 3 Dec, **then** the fee is 0.00 and the total is 200.00.
- **AC2 (edge)** — **Given** the same promotion, **when** she quotes ElectroCo, **then** the fee is 2.50.
- **AC3 (edge)** — **Given** the end is 8 Dec 00:00 (exclusive), **when** she quotes AquaCo at 8 Dec 00:00, **then** the fee is 2.50.
- **AC4 (edge)** — **Given** an agent pays the AquaCo bill during the promotion, **then** the commission is 0.00.

*Dependencies:* US-3.02, US-7.01

## E4 — Payment Processing

### US-4.01 — Pay a bill from the wallet
| Priority | FR | Decisions | Hard case |
|---|---|---|---|
| Must | FR-9 | D-03, D-09, D-37 | — |

**As a** Customer, **I want** to pay a quoted bill from my wallet, **so that** my bill is settled without queuing at the biller.

*Paths:* valid Quote → checks pass → Initiated → FundsHeld → SentToBiller → Completed. Alternative: Quote expired; insufficient funds; biller disabled → Rejected.

- **AC1 (happy)** — **Given** Sara has available 1,000.00 and held 0.00, and a valid Quote for ElectroCo (350.00 + 2.50 = 352.50), **when** she pays with that Quote, **then** the payment goes Initiated → FundsHeld → SentToBiller → Completed, her wallet shows available 647.50 and held 0.00, PayLink fee income is credited 2.50, and the response returns the reference with state Completed.
- **AC2 (negative)** — **Given** the Quote is older than `quote.ttl_minutes`, **when** Sara pays, **then** the payment goes Initiated → Rejected with `QUOTE_EXPIRED`, and no ledger entry is created.
- **AC3 (negative)** — **Given** Sara has available 300.00 and the total is 352.50, **when** she pays, **then** after the fraud chain passes, the payment goes Initiated → Rejected with `INSUFFICIENT_FUNDS` (checked at the Hold, D-37), and her balance is unchanged.
- **AC4 (negative)** — **Given** ElectroCo was disabled after the Quote was issued, **when** Sara pays, **then** the payment goes Initiated → Rejected with `BILLER_DISABLED`, and no Hold is created.

*Dependencies:* US-1.07, US-3.01, US-3.02, US-5.01, US-6.01, US-8.01

### US-4.02 — Duplicate requests have one effect
| Priority | FR | Decisions | Hard case |
|---|---|---|---|
| Must | FR-10 | D-08, D-33 | #1 |

**As a** Customer, **I want** a repeated payment request with the same Idempotency key to return the original result, **so that** a slow network never charges me twice.

*Paths:* duplicate gets the original result. Alternative: first request still in flight; same key, different payload; key missing; another customer uses the same key.

- **AC1 (happy, #1)** — **Given** Sara's payment with key K1 Completed for 352.50, **when** the app resends the same request with K1 within `idempotency.ttl_hours` (24, D-33), **then** no new payment is created, the response returns the original reference with state Completed, and her wallet shows exactly one charge of 352.50.
- **AC2 (edge)** — **Given** the K1 payment is in SentToBiller, **when** the duplicate arrives, **then** the response returns the same reference with state SentToBiller, and the biller receives exactly one request.
- **AC3 (negative)** — **Given** K1 was used for a 352.50 payment, **when** a request with K1 arrives for a different Quote, **then** it is rejected with `IDEMPOTENCY_KEY_REUSED`, and no payment is created.
- **AC4 (negative)** — **Given** a payment request has no key, **when** it is submitted, **then** it is rejected with `IDEMPOTENCY_KEY_MISSING`.

*Dependencies:* US-4.01. *Note:* keys are scoped per actor (D-08) — customer B may use K1 independently.

### US-4.03 — Enforce the payment life cycle
| Priority | FR | Decisions | Hard case |
|---|---|---|---|
| Must | FR-11 | D-09, D-10 | — |

**As the** System, **I want** to allow only the §2.5 transitions and record each one, **so that** no payment reaches a state that could lose or duplicate money.

*Paths:* each valid event moves the state and is recorded. Alternative: an event on a final state, or an invalid transition, is refused and logged.

- **AC1 (happy)** — **Given** a payment in SentToBiller, **when** the biller confirms, **then** the state becomes Completed and the history records from-state, to-state, event, actor and time.
- **AC2 (negative)** — **Given** a Completed payment, **when** a "failed" result arrives for it, **then** the state stays Completed and the attempt is logged with `INVALID_TRANSITION`.
- **AC3 (negative)** — **Given** a payment in Unknown, **when** a release of the Hold is attempted, **then** it is refused with `INVALID_TRANSITION` and 352.50 stays held (Unknown → Refunded is not allowed).
- **AC4 (edge)** — **Given** a payment in Initiated, **when** an attempt is made to move it straight to SentToBiller, **then** it is refused with `INVALID_TRANSITION`.

*Dependencies:* US-9.01, US-9.02

### US-4.04 — Hold before the biller, release on failure
| Priority | FR | Decisions | Hard case |
|---|---|---|---|
| Must | FR-12 | D-09, D-10 | (#3, owned by US-5.02) |

**As a** Customer, **I want** my money held before the biller is called and returned if the payment fails, **so that** I'm never charged for a payment the biller didn't take.

*Paths:* Hold → biller confirms → charge. Alternative: biller refuses or connection refused → Failed → Refunded.

- **AC1 (happy)** — **Given** Sara has available 1,000.00, **when** her 352.50 payment reaches FundsHeld, **then** available is 647.50 and held is 352.50 before any request reaches the biller.
- **AC2 (negative)** — **Given** an AquaCo payment of 202.50 in SentToBiller, **when** AquaCo refuses with an error code, **then** the payment goes Failed → Refunded immediately, and 202.50 returns to available with held 0.00.
- **AC3 (negative)** — **Given** a payment of 352.50 after the Hold, **when** the connection to ElectroCo is refused, **then** the payment goes SentToBiller → Failed (treated as "Biller refuses", D-09) → Refunded, and the balance is restored.
- **AC4 (edge)** — **Given** a payment is Rejected at Initiated, **then** no Hold or release is ever recorded for it.

*Dependencies:* US-4.03, US-1.07, US-5.01, US-5.02

### US-4.05 — Timeout becomes Unknown
| Priority | FR | Decisions | Hard case |
|---|---|---|---|
| Must | FR-13 | D-12, D-15, D-32 | #2 |

**As a** Customer, **I want** to be told my payment is being processed when the biller is slow, while my money stays held, **so that** I don't pay again and don't lose money the biller may already have.

*Paths:* no answer within 10 s → Unknown, money held. Alternative: TopUpNet sends no "accepted" in 10 s; or "accepted" but no callback within the callback timeout.

- **AC1 (happy, #2)** — **Given** `billers.timeout.live_seconds` = 10 and the ElectroCo mock answers after 12 s, **when** Sara pays 352.50, **then** after 10 s the payment goes SentToBiller → Unknown, the response says it is being processed, available stays 647.50 and held 352.50, and no SMS is sent. If the late answer then arrives, it is applied as "Biller later confirms" (D-32).
- **AC2 (edge)** — **Given** ElectroCo answers "paid" within 10 s, **then** the payment goes to Completed and never to Unknown.
- **AC3 (edge)** — **Given** TopUpNet sends "accepted" within 10 s but no callback arrives within `billers.topupnet.callback_timeout_minutes` (30), **then** the payment goes to Unknown and the money stays held.
- **AC4 (negative)** — **Given** a payment in Unknown, **when** time passes with no answer, **then** no release happens automatically.

*Dependencies:* US-4.03, US-5.01, US-5.03

### US-4.06 — Re-query Unknown payments
| Priority | FR | Decisions | Hard case |
|---|---|---|---|
| Must | FR-13 | D-13, D-15, D-30, D-31 | — |

**As the** System, **I want** to re-query an Unknown payment at the configured delays, **so that** customers get a final result without waiting for the nightly file.

*Paths:* re-query confirms → Completed. Alternative: refusal → Failed → Refunded; no answer after all re-queries → wait for reconciliation; a callback resolves it first.

- **AC1 (happy)** — **Given** a payment of 352.50 in Unknown and `retries.unknown.delays_minutes` = [1, 5, 15], measured from entering Unknown (D-30), **when** the first status inquiry (D-31) returns "paid", **then** the payment goes to Completed, the Hold becomes a charge, and Sara gets SMS + push.
- **AC2 (negative)** — **Given** the re-query returns "refused", **then** the payment goes Unknown → Failed → Refunded, and 352.50 returns to available.
- **AC3 (edge)** — **Given** all 3 re-queries get no answer, **then** the payment stays Unknown with 352.50 held, no 4th re-query is sent, and it waits for reconciliation.
- **AC4 (edge)** — **Given** a TopUpNet callback resolves the payment before the next re-query, **then** the remaining re-queries are not sent.

*Dependencies:* US-4.05, E5, US-10.01, US-8.01

### US-4.07 — Concurrent payments never overdraw
| Priority | FR | Decisions | Hard case |
|---|---|---|---|
| Must | FR-9, FR-12 | D-01, D-08 | #4 |

**As a** Customer, **I want** simultaneous payments from my wallet never to take it below zero, **so that** my balance is always true.

*Paths:* two payments at once, only one fits → one goes through, one Rejected. Alternative: both fit the balance but together break the daily limit.

- **AC1 (happy, #4)** — **Given** Sara has available 400.00, **when** two payments of 352.50 with different Idempotency keys arrive at the same moment, **then** exactly one reaches FundsHeld, the other goes Initiated → Rejected with `INSUFFICIENT_FUNDS`, and her wallet shows available 47.50 and held 352.50.
- **AC2 (negative)** — **Given** a basic account with 4,800.00 counted today (`limits.daily.basic` = 5,000) and available 10,000.00, **when** two payments of 152.50 arrive at once, **then** one goes through and the other is Rejected with `DAILY_LIMIT_EXCEEDED`.

*Dependencies:* US-4.01, US-4.02, US-6.01. *Note:* the throughput test (100 payments/s) moved to NFR-02.

## E5 — Biller Integration

### US-5.01 — Customer pays an electricity bill
| Priority | FR | Decisions | Hard case |
|---|---|---|---|
| Must | FR-15 | D-09, D-31, D-32 | — |

**As a** Customer, **I want** to pay ElectroCo from my wallet, **so that** my electricity bill is settled immediately.

*Paths:* immediate confirmation. Alternative: ElectroCo refuses; status inquiry on an Unknown payment.

- **AC1 (happy)** — **Given** a Quote of 352.50 and funds held, **when** the payment is sent with its unique reference, **then** ElectroCo confirms and the payment is Completed.
- **AC2 (negative)** — **Given** SentToBiller, **when** ElectroCo refuses with an error code, **then** the payment goes Failed → Refunded, and the code is mapped to a clear reason for the customer.
- **AC3 (edge)** — **Given** the payment is Unknown, **when** a status inquiry with its reference returns "paid", **then** the payment is Completed and nothing is sent twice.

*Dependencies:* E4, US-3.01

### US-5.02 — Customer pays a water bill
| Priority | FR | Decisions | Hard case |
|---|---|---|---|
| Must | FR-15 | D-09, D-10, D-15 | #3 |

**As a** Customer, **I want** to pay AquaCo from my wallet, **so that** my water bill is settled even though AquaCo is slow.

*Paths:* slow confirmation within 10 s. Alternative: refusal with a known or unknown error code.

- **AC1 (happy)** — **Given** a Quote of 200.00 + 2.50 = 202.50, **when** AquaCo confirms after 3.9 s, **then** the payment is Completed and never Unknown.
- **AC2 (negative, #3)** — **Given** SentToBiller with 202.50 held, **when** AquaCo refuses with an error code, **then** the payment goes Failed → Refunded, 202.50 returns to available, and Sara gets an SMS with a clear reason mapped from AquaCo's code.
- **AC3 (edge)** — **Given** an AquaCo error code not in the mapping, **then** the payment goes Failed → Refunded with the generic reason `BILLER_REFUSED`, and the code is logged.

*Dependencies:* E4, US-8.01

### US-5.03 — Customer tops up a mobile phone
| Priority | FR | Decisions | Hard case |
|---|---|---|---|
| Must | FR-14, FR-15 | D-12, D-23 | #6 |

**As a** Customer, **I want** to top up my mobile balance through TopUpNet, **so that** my line stays active.

*Paths:* accepted → "success" callback → Completed. Alternative: "failed" callback; duplicate callback; conflicting callback.

- **AC1 (happy)** — **Given** a Quote of 101.50, **when** TopUpNet replies "accepted" within 10 s and later sends "success", **then** the payment goes SentToBiller → Completed.
- **AC2 (edge, #6)** — **Given** the payment is Completed after a "success" callback, **when** a second "success" callback arrives, **then** the payment is completed only once, and the duplicate is ignored and logged as DUPLICATE.
- **AC3 (negative)** — **Given** SentToBiller, **when** the callback says "failed", **then** the payment goes Failed → Refunded and 101.50 returns to available.
- **AC4 (negative)** — **Given** the payment is Completed, **when** a "failed" callback arrives, **then** it stays Completed, and the callback is logged as CONFLICT and flagged to support.

*Dependencies:* E4, US-4.05

### US-5.04 — New billers are added without touching existing modules
| Priority | FR | Decisions | Hard case |
|---|---|---|---|
| Must | FR-6, FR-15 | NFR-04 | #8 |

**As** PayLink Management, **I want** new billers to be added through only a new integration and configuration, **so that** customers get new billers without putting existing payments at risk.

*Paths:* new integration plus config → biller available. Alternative: biller configured but has no integration.

- **AC1 (happy, #8)** — **Given** a new mock biller with its own protocol, **when** its integration and configuration are added, **then** customers can quote and pay it, the change touches only the Biller Integration module and configuration, and all existing tests pass unchanged.
- **AC2 (negative)** — **Given** a biller in the catalog with no integration available, **when** an admin tries to enable it, **then** the request is rejected with `BILLER_INTEGRATION_MISSING`.

*Dependencies:* US-2.01

### US-5.05 — Electricity payments keep working on ElectroCo v2 *(Phase 2 — not counted in the Phase 1 total)*
| Priority | FR | Decisions | Hard case |
|---|---|---|---|
| Must (Phase 2) | §2.4 change request | D-27 | — |

**As a** Customer, **I want** electricity payments to keep working after ElectroCo changes its API, **so that** I'm not affected by a biller's upgrade.

- **AC1 (happy)** — **Given** `billers.electroco.api_version` = v2, **when** Sara pays 350.00, **then** ElectroCo receives 35000 piasters in a signed request, the payment is Completed, and Sara sees 350.00 EGP.
- **AC2 (negative)** — **Given** v2, **when** ElectroCo rejects the signature, **then** the payment goes Failed → Refunded, and the error is logged.
- **AC3 (edge)** — **Given** the v2 change is merged, **then** the change report shows only ElectroCo integration and configuration files changed.
- **AC4 (edge)** — **Given** v2 is active, **when** the config is switched back to v1, **then** payments are Completed with no other change.

*Dependencies:* US-5.01

## E6 — Fraud & Limits

> Fraud checks run as a chain before the Hold, in this order (D-37): `ACCOUNT_BLOCKED` → `VELOCITY_BLOCKED` → `AMOUNT_BELOW_MIN` / `AMOUNT_ABOVE_MAX` → `DAILY_LIMIT_EXCEEDED`. The first failing rule stops the chain. `INSUFFICIENT_FUNDS` is checked at the Hold.

### US-6.01 — Customer gets a clear reason when a payment breaks a limit
| Priority | FR | Decisions | Hard case |
|---|---|---|---|
| Must | FR-18 | D-01, D-02, D-37 | — |

**As a** Customer, **I want** a payment that breaks a limit to be rejected with a clear reason, **so that** I know why it failed and what I can still pay.

*Paths:* within all limits → passes the chain. Alternative: below min, above max or over the daily limit → Initiated → Rejected.

- **AC1 (happy)** — **Given** Sara (BASIC) has 4,500.00 counted today, **when** she pays 352.50, **then** the payment passes the fraud chain and reaches FundsHeld, and 4,852.50 is now counted.
- **AC2 (edge)** — **Given** 4,647.50 counted today, **when** she pays 352.50, **then** the payment is allowed (exactly 5,000.00 = `limits.daily.basic`).
- **AC3 (negative)** — **Given** 4,800.00 counted today, **when** she pays 352.50, **then** the payment goes Initiated → Rejected with `DAILY_LIMIT_EXCEEDED` and no Hold is created.
- **AC4 (negative)** — **Given** `limits.payment.min` = 5.00 and `limits.payment.max` = 10,000.00, **when** she pays a bill of 4.99 or 10,000.01, **then** the payment goes Initiated → Rejected with `AMOUNT_BELOW_MIN` or `AMOUNT_ABOVE_MAX`.

*Dependencies:* US-4.01, US-11.01

### US-6.02 — Rapid repeated payments are blocked
| Priority | FR | Decisions | Hard case |
|---|---|---|---|
| Should | FR-19 | D-14, D-37 | — |

**As a** Customer, **I want** my account to block a burst of payments, **so that** a stolen phone or a script can't drain my wallet.

*Paths:* ≤ 5 payments in 60 s processed normally. Alternative: the 6th triggers a 10-minute block; idempotent replays don't count.

- **AC1 (happy)** — **Given** `fraud.velocity.max_payments` = 5 and `fraud.velocity.window_seconds` = 60, **when** Sara makes 5 payments within 60 s, **then** all 5 are processed normally.
- **AC2 (negative)** — **Given** 5 payments in the last 60 s, **when** she makes a 6th, **then** it goes Initiated → Rejected with `VELOCITY_BLOCKED`, and a VELOCITY block is set for `fraud.velocity.block_minutes` (10).
- **AC3 (edge)** — **Given** an active VELOCITY block, **then** payments are Rejected with `VELOCITY_BLOCKED`, top-ups still succeed, and after 10 min payments are allowed again.
- **AC4 (edge)** — **Given** 5 payments plus a resend of one of them with the same Idempotency key, **then** no block is set.

*Dependencies:* US-4.02

### US-6.03 — Support blocks and unblocks an account
| Priority | FR | Decisions | Hard case |
|---|---|---|---|
| Should | FR-21 | D-14, D-18, D-21, D-22, D-37 | — |

**As a** Support Employee, **I want** to block and unblock a customer account, **so that** I can stop suspected fraud quickly.

*Paths:* block → new payments and top-ups rejected; unblock → allowed again. Alternative: payment in flight; VELOCITY block active; caller isn't support.

- **AC1 (happy)** — **Given** Sara is active, **when** support blocks her with a reason, **then** new payments and top-ups are Rejected with `ACCOUNT_BLOCKED`, and an audit record holds who, when and why.
- **AC2 (edge)** — **Given** her payment is in SentToBiller when the block is applied, **then** it continues normally to Completed.
- **AC3 (edge)** — **Given** both a MANUAL and an active VELOCITY block, **when** support unblocks her, **then** only the MANUAL block is removed, and payments stay `VELOCITY_BLOCKED` until it expires.
- **AC4 (negative)** — **Given** a customer, **when** they try to block or unblock any account, **then** the request is rejected with `FORBIDDEN`.

*Dependencies:* US-9.02

## E7 — Agents

### US-7.01 — Agent pays for a customer and earns commission
| Priority | FR | Decisions | Hard case |
|---|---|---|---|
| Should | FR-17 | D-05, D-06 | — |

**As an** Agent, **I want** to pay a bill for a registered customer from my wallet and earn commission, **so that** I can serve cash customers and make money from it.

*Paths:* agent wallet debited → Completed → commission credited. Alternative: customer over limit; agent short of funds; customer not registered; payment Failed.

- **AC1 (happy)** — **Given** the agent wallet has 1,000.00 and `commission.agent.rate` = 0.40, **when** the agent pays Sara's bill of 350.00 + 2.50 = 352.50, **then** the payment is Completed; the agent wallet shows 1,000.00 − 352.50 + 1.00 = 648.50; PayLink fee income shows +2.50 then −1.00 for the commission; Sara's wallet is unchanged and 352.50 counts toward Sara's daily limit.
- **AC2 (negative)** — **Given** Sara has 4,800.00 counted today, **when** the agent pays 352.50 for her, **then** the payment is Rejected with `DAILY_LIMIT_EXCEEDED`, even though the agent has funds.
- **AC3 (negative)** — **Given** the agent wallet has 300.00, or the customer isn't registered, **then** the payment is Rejected with `INSUFFICIENT_FUNDS` or `CUSTOMER_NOT_FOUND`.
- **AC4 (edge)** — **Given** the agent payment goes Failed → Refunded, **then** 352.50 returns to the agent wallet and no commission is credited.

*Dependencies:* E4, US-3.02, US-1.06

## E8 — Notifications

### US-8.01 — Customer is told when a payment completes or fails
| Priority | FR | Decisions | Hard case |
|---|---|---|---|
| Must | FR-22 | D-15 | (#3, with US-5.02) |

**As a** Customer, **I want** an SMS and a push message when my payment completes or fails, **so that** I have a receipt, or know why the payment didn't go through.

*Paths:* Completed or Failed → SMS + push. Alternative: Unknown or Rejected → response only, no message.

- **AC1 (happy)** — **Given** Sara's 352.50 payment to ElectroCo completes, **then** she gets an SMS and a push message with the reference, biller and 352.50.
- **AC2 (happy)** — **Given** her payment goes Failed, **then** she gets an SMS and a push message with a clear reason and confirmation that the money was returned.
- **AC3 (edge)** — **Given** a payment becomes Unknown or Rejected, **then** no SMS or push is sent, and the status is only in the response.

*Dependencies:* E4, US-8.02

### US-8.02 — Failed messages are retried, then marked undelivered
| Priority | FR | Decisions | Hard case |
|---|---|---|---|
| **Must (hard case)** — FR-23 is Should | FR-23 | D-15, D-38 | #5 |

**As a** Customer, **I want** failed messages to be retried, **so that** I still get my receipt when the provider has a brief outage.

*Paths:* a retry succeeds → DELIVERED. Alternative: all retries fail → UNDELIVERED; the payment is never affected.

- **AC1 (negative, #5)** — **Given** the SMS provider is down when Sara's payment completes, **then** the payment stays Completed; the SMS is retried `notifications.retry.max_retries` (3) times after the first attempt — 4 attempts in total — with backoff starting at `notifications.retry.backoff_base_seconds` (30); and after the 4th failed attempt it is marked UNDELIVERED (D-38).
- **AC2 (happy)** — **Given** the first SMS attempt fails, **when** the next attempt succeeds, **then** the SMS is DELIVERED and its attempt count is recorded.
- **AC3 (edge)** — **Given** the push provider is down, **then** the same retry rule applies to push.
- **AC4 (edge)** — **Given** an UNDELIVERED SMS, **when** support opens the payment, **then** the notification shows status UNDELIVERED.

*Dependencies:* US-8.01, US-9.01

## E9 — Support, Audit & Reversal

### US-9.01 — Support finds a payment and its full history
| Priority | FR | Decisions | Hard case |
|---|---|---|---|
| Must | FR-20 | D-21, D-28, D-34 | — |

**As a** Support Employee, **I want** to search a payment by reference and see its full history, **so that** I can answer a customer complaint in one place.

*Paths:* search by PayLink reference → details and history. Alternative: biller reference; Rejected payment; reference not found.

- **AC1 (happy)** — **Given** a payment that went Initiated → FundsHeld → SentToBiller → Unknown → Completed, **when** support searches its PayLink reference, **then** they see every transition (event, actor, time), its ledger entries and its notification statuses.
- **AC2 (edge)** — **Given** the same payment, **when** support searches by its biller reference, **then** they find the same payment.
- **AC3 (edge)** — **Given** a payment Rejected with `INSUFFICIENT_FUNDS`, **when** support searches its reference, **then** they see it with that reason.
- **AC4 (negative)** — **Given** a reference that doesn't exist, or a caller who is a customer, **then** the search is rejected with `PAYMENT_NOT_FOUND` or `FORBIDDEN`.

*Dependencies:* US-4.03, US-9.02

### US-9.02 — Every money movement and admin action is audited
| Priority | FR | Decisions | Hard case |
|---|---|---|---|
| Must | FR-25 | D-22 | — |

**As** PayLink Management, **I want** an audit record for every money movement and every admin or support action, **so that** we can prove to the regulator who did what, when and why.

*Paths:* each movement or action → audit record. Alternative: attempt to change a record; completeness check.

- **AC1 (happy)** — **Given** any ledger entry (TOPUP, HOLD, RELEASE, CHARGE, FEE_INCOME, COMMISSION, COMMISSION_REVERSAL, REVERSAL, ADJUSTMENT), **then** an audit record exists with actor (user or SYSTEM), time and reason code.
- **AC2 (happy)** — **Given** an admin or support action (disable a biller, change a fee, block, upgrade, approve a Reversal), **then** an audit record holds who, when and why.
- **AC3 (negative)** — **Given** an audit record, **when** anyone tries to change or delete it, **then** the attempt is refused with `AUDIT_IMMUTABLE`.
- **AC4 (edge)** — **Given** the end of any test run, **then** the number of money-movement audit records equals the number of ledger entries (100%).

*Dependencies:* US-1.07

### US-9.03 — Customer gets a Reversal of a completed payment
| Priority | FR | Decisions | Hard case |
|---|---|---|---|
| Could | FR-27 | D-05, D-11 · R6 | — |

**As a** Customer, **I want** to request a Reversal of a completed payment, for support to approve, **so that** I get my money back when a payment was made in error.

*Paths:* request → support approves → wallet credited; payment stays Completed. Alternative: support rejects; payment not Completed; Reversal already exists; agent payment.

- **AC1 (happy)** — **Given** Sara's Completed payment of 350.00 + 2.50, **when** she requests a Reversal and support approves it, **then** 352.50 is credited to her wallet (`reversal.include_fee` = true) from a PayLink account, the payment stays Completed, and the Reversal is Approved and audited.
- **AC2 (edge)** — **Given** the payment was made by an agent, **when** its Reversal is approved, **then** the 1.00 commission is clawed back from the agent wallet as COMMISSION_REVERSAL.
- **AC3 (negative)** — **Given** a Reversal in Requested, **when** support rejects it, **then** the Reversal is Rejected and no money moves.
- **AC4 (negative)** — **Given** a payment in Failed, Refunded or Unknown, or one that already has a Reversal, **when** a Reversal is requested, **then** it is rejected with `REVERSAL_NOT_ALLOWED` or `REVERSAL_ALREADY_EXISTS`.

*Dependencies:* US-9.01, US-9.02, US-7.01

## E10 — Reconciliation & Reporting

### US-10.01 — Support gets a daily reconciliation report
| Priority | FR | Decisions | Hard case |
|---|---|---|---|
| **Must (hard case)** — FR-26 is Should | FR-26 | D-13, D-17 | #7 |

**As a** Support Employee, **I want** a daily report comparing our payments with each biller's settlement file, **so that** I can find and fix every money difference.

*Paths:* file received → match → Unknown payments resolved → differences reported. Alternative: file late or missing; difference on a payment in a final state.

- **AC1 (happy)** — **Given** ElectroCo's file for 24 Nov was received, listing every Completed payment plus one payment PayLink has in Unknown, **then** the Unknown payment goes to Completed and the report shows 0 differences.
- **AC2 (negative, #7)** — **Given** the file lists a payment PayLink has as Failed (now Refunded), **then** the difference is flagged in the report for support to review, and the payment state does not change.
- **AC3 (edge)** — **Given** an Unknown payment missing from the file, **then** if the file was received, the payment goes Failed → Refunded; if no file arrived by `reconciliation.file_due_time`, it stays Unknown and is flagged `FILE_MISSING`.
- **AC4 (negative)** — **Given** a Completed payment missing from the file, or a payment whose amount doesn't match, **then** each is flagged as a difference with its type.

*Dependencies:* US-4.06, US-5.01 – US-5.03

### US-10.02 — Admin sees daily totals
| Priority | FR | Decisions | Hard case |
|---|---|---|---|
| Could | FR-28 | D-01, D-21, D-39 | — |

**As an** Admin, **I want** daily totals of payments, fees, commissions and failures per biller, **so that** I can track revenue and biller health.

*Paths:* totals for a business day. Alternative: day boundary; a day with no payments; non-admin caller.

- **AC1 (happy)** — **Given** on 24 Nov there were 3 Completed ElectroCo payments of 352.50 (one by an agent), 1 Failed AquaCo payment and no Reversals, **then** the totals show payments 3, value 1,057.50; fees earned 7.50; commissions paid 1.00; Reversals and clawbacks as separate lines (0.00), with gross = net (D-39); failures AquaCo 1.
- **AC2 (edge)** — **Given** one payment Completed at 23:59:59 on 24 Nov and one at 00:00:00 on 25 Nov (Cairo), **then** only the first counts toward 24 Nov.
- **AC3 (edge)** — **Given** a day with no payments, **then** all totals show 0.
- **AC4 (negative)** — **Given** a support employee, **when** they request the totals, **then** the request is rejected with `FORBIDDEN` (D-21).

*Dependencies:* E4, US-7.01

## E11 — Configuration

### US-11.01 — Admin changes fees and limits without a redeploy
| Priority | FR | Decisions | Hard case |
|---|---|---|---|
| Should | FR-24 | D-03, D-20, D-21 | — |

**As an** Admin, **I want** to change fee and limit values while the system is running, **so that** promotions and policy changes take effect without downtime.

*Paths:* change → new version → applies to new Quotes and payments. Alternative: invalid value; existing Quote; non-admin caller.

- **AC1 (happy)** — **Given** `fees.electricity.fixed` = 2.50, **when** an admin sets it to 3.00, **then** new Quotes show 3.00 without a restart, and a new config version is recorded with who and when.
- **AC2 (edge)** — **Given** a Quote issued at 2.50 before the change, **when** Sara pays with it, **then** she is charged 2.50.
- **AC3 (negative)** — **Given** an invalid value (a negative fee, or `limits.daily.basic` above `limits.daily.verified`), **when** the admin saves it, **then** the change is rejected with `CONFIG_INVALID` and the old value stays.
- **AC4 (negative)** — **Given** a support employee, **when** they try to change a value, **then** the request is rejected with `FORBIDDEN`.

*Dependencies:* US-9.02

\newpage

# 9. Non-functional requirements

| ID | Quality | Requirement | Target | How measured | When | Linked stories |
|---|---|---|---|---|---|---|
| NFR-01 | Correctness | No double charges, no negative balances (§3.3) | 0 double charges, 0 negative balances | Hard-case tests #1 and #4, concurrency suite, ledger sum check after each run | CI (every push) | US-4.02, 4.07, 1.07 |
| NFR-02 | Performance | Sustained throughput against the mock billers (§3.3) | ≥ 100 payments/s for 60 s (6,000 payments), equal ElectroCo / AquaCo / TopUpNet mix (D-24); 0 double charges; 0 negative balances | Load-test tool against docker compose; ledger and balance checks after the run | Load test before the final tag | US-4.01, 4.07 |
| NFR-03 | Resilience | One biller's outage doesn't stop payments to the others (§3.3) | Other billers' success rate within ±1% of baseline (D-25, team-defined) | Load test with one mock biller down for the whole run | Load test | US-4.05, 5.01, 5.02, 5.03 |
| NFR-04 | Modifiability | Adding a biller, or ElectroCo v2, changes only Biller Integration + configuration (§3.3, §2.4) | 0 files changed outside the module and config | Diff check on the #8 test branch and the v2 change report | PR review + Phase 2 report | US-5.04, 5.05 |
| NFR-05 | Auditability | Every money movement has an audit record with who, when, why (§3.3) | 100% (audit records = ledger entries) | US-9.02 AC4 count check | CI | US-9.02, 1.07, 6.03, 11.01 |
| NFR-06 | Testability | Line coverage on domain logic (§3.3, D-26) | ≥ 70% on domain and application layers; adapters still unit-tested | Coverage report in the pipeline; build fails below 70% | CI | All |
| NFR-07 | Correctness (money precision) | Exact decimals, never floating point (§2.6) | 0 floating-point money fields; 100% of fee test vectors match at 2 dp, HALF_UP | Lint rule + §2.6 fee test vectors | CI | US-3.02, 3.03, 1.07 |
| NFR-08 | Configurability | §2.6 values live in configuration, not code (§2.6, FR-24) | 0 hard-coded §2.6 values; 100% of listed keys changeable without restart | Review checklist + test that changes each key at runtime | Review + CI | US-11.01, 6.01, 3.02 |
| NFR-09 | Recoverability (team-defined) | No Unknown payment is stuck unnoticed (R3) | 100% of Unknown payments older than `unknown.escalation_hours` (48) are flagged | Test with a simulated clock | CI | US-4.06, 10.01 |
| NFR-10 | Deployability (team-defined) | A push to main deploys locally and passes the health check (§4 Phase 2 #6) | Health check passes within 120 s; rollback step runs successfully once | Pipeline deploy job + rollback drill | CI/CD (main) | — |

NFR-01 to NFR-06 come from §3.3; NFR-07 and NFR-08 from §2.6; NFR-09 and NFR-10 are team-defined.

\newpage

# 10. Questions, ambiguities and decisions

The questions below were raised by the AI during discovery (Q-01 – Q-44) and while writing stories (OQ-1 – OQ-10), or found during the product lead's review. Each was decided by the team; none was left to the AI. The full prompt history is in `/docs/prompt-log.md`.

**Corrections the product lead made to AI recommendations**

| Item | AI recommendation | Problem | Team decision |
|---|---|---|---|
| Q-19 | Add a FundsHeld → Failed transition | Violates FR-11 ("no other transitions") | D-09: no new transition |
| Q-16, Q-22, Q-36 | "Ask the business" | The team is the business | Decided in D-05, D-11, D-07 |
| Success indicators | Invented targets (≥ 98% success, < 0.5% Unknown) | Not in the brief; they measure billers, not PayLink | Dropped |
| US-8.02, US-10.01 | Priority Should (from FR) | §2.7 requires a test for every hard case | Raised to Must (hard case) |

## 10.1 Main decisions (change a requirement, test, state diagram or data model)

| D-ID | Question | Decision | Rationale | Brief § |
|---|---|---|---|---|
| D-01 | Q-01/03/04: Does the daily limit include the fee? Calendar day or rolling 24 h? Which states count? | Amount + fee; calendar day, Africa/Cairo; counts FundsHeld, SentToBiller, Unknown and Completed (not Rejected, Failed, Refunded). Keys `limits.daily.basic`, `limits.daily.verified`, `limits.daily.timezone`. | The Hold is amount + fee; counting pending states closes a bypass. | §2.6, §2.2, §2.5 |
| D-02 | Q-02/05/09: Do single-payment min/max apply to the bill or the total? Biller vs global range? Does verified raise the max? | Bill amount without fee; a biller range can only narrow the global range; verified raises only the daily limit. | §2.6 gives one single-payment range for all accounts. | §2.6, §3.1, FR-2 |
| D-03 | Q-10/11/12: Partial payments? Is the fee locked at inquiry? Inquiry for TopUpNet? | Inquiry creates a Quote (amount, fee, total, expiry `quote.ttl_minutes` = 15). Payment pays exactly the Quote; fee locked. TopUpNet: customer enters the amount; Quote is the fee preview. Expired Quote → Rejected. | FR-8 shows the total before paying; AquaCo rejects amounts above the bill. | FR-8, FR-16, §2.4 |
| D-04 | Q-13/14/15: Rounding? Is 200.00 low or high tier? Promotion boundaries? | 2 dp HALF_UP on the bill amount; ≤ 200.00 → 2, > 200.00 → 4; promotion on one biller, Cairo time, start inclusive, end exclusive. | "Up to" reads inclusive; §2.6 says "one chosen biller". | §2.6 |
| D-05 | Q-16/18: Commission during a 0-fee promotion? Clawback on Reversal? | Commission = 40% × fee actually charged (0 during promotion); paid from fee income on Completed; clawed back on Reversal. | §2.6 defines commission on the service fee; no subsidy rule exists. | §2.6, FR-17, FR-27 |
| D-06 | Q-17: Whose wallet is debited in an agent payment? Whose limits apply? | Agent's wallet is debited; limits, velocity and blocks apply to the customer; agents serve **registered customers only**. | Matches the agent-terminal model; limits need a subject. | FR-17, §2.1 |
| D-07 | Q-36: Top-up fees, limits, agent cash? | No fee or commission; agent cash debits the agent and credits the customer; top-ups don't count toward payment limits or velocity; bounds `topup.min` 5, `topup.max` 10,000 (team-defined). | §2.6 defines fees only for payments. | FR-4, §2.6 |
| D-08 | Q-29/30: Idempotency scope, lifetime, payload mismatch, in-flight duplicates; which endpoints? | Required on every money-moving request; scope per actor; TTL `idempotency.ttl_hours` 24; payload mismatch → error; in-flight → current state of the first request. | FR-10; hard cases #1 and #4. | FR-10, §2.7 |
| D-09 | Q-19: FundsHeld has one exit — what if sending fails before the biller is reached? | No new transition. Disabled/invalid biller → caught before the Hold (Initiated → Rejected). Refused connection after the Hold = "Biller refuses" (SentToBiller → Failed), as the biller definitely did not receive it. Timeout → Unknown. | FR-11 allows only §2.5 transitions. | §2.5, FR-11 |
| D-10 | Q-20: Is Failed → Refunded automatic? | Automatic and immediate; Hold released by a ledger entry. | FR-12 says the Hold is released on failure. | §2.5, FR-12 |
| D-11 | Q-21/22: Completed is final, but FR-27 refunds a completed payment. Is the fee refunded? | FR-27 creates a separate **Reversal**; the payment stays Completed. Approved Reversal credits amount + fee from a PayLink account; no biller call; commission clawed back. Risk accepted (R6). | Keeps §2.5 intact; no reversal operation in §2.4. | FR-27, §2.5 |
| D-12 | Q-27: Does the 10 s timeout apply to TopUpNet? | Applies to the "accepted" reply; the callback wait has its own timeout (`billers.topupnet.callback_timeout_minutes` 30), then Unknown. | Otherwise every TopUpNet payment becomes Unknown. | §2.4, §2.6, FR-14 |
| D-13 | Q-23/24/25: How long can Unknown wait? Late settlement file? What does reconciliation do? | Re-queries, then wait for reconciliation. In file → Completed. Missing → Failed → Refunded **only if that biller's file for that day was received**; otherwise stays Unknown and flagged. Differences on final states flagged; support corrects with an ADJUSTMENT entry. Unknown older than `unknown.escalation_hours` (48) flagged. | §2.5: money returned only when sure the biller didn't get it. | §2.5, §2.7 #7, FR-26 |
| D-14 | Q-06/07: What does velocity count? Is the 6th payment rejected? Same as a support block? | Counts every payment created, excluding idempotent replays; the 6th within 60 s is Rejected and a VELOCITY block is set for 10 min; payments only; separate from the support block. | Replays aren't new payments; FR-21 stays with support. | §2.6, FR-19, FR-21 |
| D-15 | Q-31/32: Which events notify on which channel? Retry interval? | Completed/Failed → SMS + push; Unknown/Rejected → API response only; email is a template channel; exponential backoff; notifications never block payments. | FR-22 names SMS + push; hard case #2 needs only that the customer is told. | FR-22, FR-23, §2.7 |

## 10.2 Further decisions

| D-ID | Question | Decision | Brief § |
|---|---|---|---|
| D-16 | Q-08: Who upgrades to verified? | An admin, manually; audited; no KYC flow. | FR-2 |
| D-17 | Q-26: Settlement file format? | One format per biller, parsed in its adapter. | §2.2, §2.4 |
| D-18 | Q-33: What does a support block stop? | New payments and top-ups; in-flight payments continue; login and balance still allowed. | FR-21 |
| D-19 | Q-34: Disabling a biller with open payments? | Stops new Quotes and payments; retries and reconciliation continue. | FR-6 |
| D-20 | Q-35: When do config changes apply? | Versioned and audited; new Quotes only. | FR-24 |
| D-21 | Q-37: Roles and authentication? | Token-based; roles CUSTOMER, AGENT, SUPPORT, ADMIN. | §2.2 |
| D-22 | Q-38: What is in an audit record? | Actor (user or SYSTEM), time, reason code; support actions included. | FR-25 |
| D-23 | Q-28: Conflicting or unknown TopUpNet callbacks? | First final result wins; duplicates ignored and logged; conflicts flagged; unknown references rejected; shared-secret signing. | FR-14, §2.7 #6 |
| D-24 | Q-39: Load-test mix and pass criteria? | 100 payments/s for 60 s, equal mix of the three billers; 0 double charges, 0 negative balances. | §3.3 |
| D-25 | Q-40: How is resilience measured? | Other billers' success rate within ±1% of baseline while one is down. | §3.3 |
| D-26 | Q-41: What is "domain logic" for coverage? | Domain + application layers; adapters excluded from the gate but unit-tested. | §3.3 |
| D-27 | Q-42: ElectroCo v2 rollout? | Config switch `billers.electroco.api_version`; pounds × 100 = piasters; signing key from an environment secret. | §2.4 |
| D-28 | Q-43: Search by which reference? | PayLink reference or biller reference. | FR-20 |
| D-29 | Q-44: NetCo in scope? | Out of scope. | FR-15 |
| D-30 | OQ-1: Re-query delays from when? | From entering Unknown: T+1, T+5, T+15 min. | §2.6 |
| D-31 | OQ-2: What is a re-query? | A status inquiry by reference; never re-sends the payment. | §2.5 |
| D-32 | OQ-3: A synchronous answer after the timeout? | Applied as "Biller later confirms/refuses" and logged. | §2.5 |
| D-33 | OQ-4: Idempotency TTL? | 24 h. | FR-10 |
| D-34 | OQ-5: Do Rejected payments get a reference? | Yes; shown in history and support search. | FR-3, FR-20 |
| D-35 | OQ-6: Does the 10 s timeout apply to inquiry? | Yes; no answer → `BILLER_UNAVAILABLE`, no Quote. | §2.6, FR-8 |
| D-36 | OQ-7: Registration fields? | Phone, name, password; no national ID; fictional test data. | FR-1, §5 |
| D-37 | OQ-8: Fraud chain order? | Manual block → velocity → min/max → daily limit; first failure wins; funds checked at the Hold. | §3.1, FR-18 |
| D-38 | OQ-9: "Retried 3 times" = 3 or 4 attempts? | 3 retries after the first attempt (4 in total); key `notifications.retry.max_retries`. | §2.6, FR-23 |
| D-39 | OQ-10: Do daily totals net out Reversals? | Reversals and clawbacks shown as separate lines, gross and net. | FR-28 |

*Brief inconsistency noted:* §1 gives Phase 2 70 points, while the §4 table gives 60 + 10 for the discussion. No requirement depends on it.

\newpage

# 11. Traceability matrix

| FR | Priority | Stories | Hard case | NFR |
|---|---|---|---|---|
| FR-1 | Must | US-1.01 | — | NFR-06 |
| FR-2 | Should | US-1.02 | — | NFR-05, 08 |
| FR-3 | Must | US-1.03, 1.04 | — | NFR-07 |
| FR-4 | Must | US-1.05, 1.06 | — | NFR-01, 05 |
| FR-5 | Must | US-1.07 | — | NFR-05, 07 |
| FR-6 | Must | US-2.01, 2.02 | — | NFR-04, 05 |
| FR-7 | Must | US-2.03 | — | NFR-06 |
| FR-8 | Must | US-3.01 | — | NFR-03 |
| FR-9 | Must | US-4.01, 4.07 | #4 | NFR-01, 02 |
| FR-10 | Must | US-4.02 | #1 | NFR-01 |
| FR-11 | Must | US-4.03 | — | NFR-06 |
| FR-12 | Must | US-4.04, 4.07 | #3, #4 | NFR-01 |
| FR-13 | Must | US-4.05, 4.06 | #2 | NFR-03, 09 |
| FR-14 | Must | US-5.03 | #6 | NFR-01 |
| FR-15 | Must | US-5.01, 5.02, 5.03, 5.04 | #3, #8 | NFR-03, 04 |
| FR-16 | Must | US-3.02, 3.03 | — | NFR-07, 08 |
| FR-17 | Should | US-7.01 | — | NFR-01, 05 |
| FR-18 | Must | US-6.01 | — | NFR-08 |
| FR-19 | Should | US-6.02 | — | NFR-08 |
| FR-20 | Must | US-9.01 | — | NFR-05 |
| FR-21 | Should | US-6.03 | — | NFR-05 |
| FR-22 | Must | US-8.01 | #3 | NFR-06 |
| FR-23 | Should → story Must (hard case) | US-8.02 | #5 | NFR-03 |
| FR-24 | Should | US-11.01 | — | NFR-05, 08 |
| FR-25 | Must | US-9.02 | — | NFR-05 |
| FR-26 | Should → story Must (hard case) | US-10.01 | #7 | NFR-09 |
| FR-27 | Could | US-9.03 | — | NFR-05 |
| FR-28 | Could | US-10.02 | — | NFR-07 |
| CR ElectroCo v2 (§2.4) | Must, Phase 2 | US-5.05 | — | NFR-04 |

**Hard cases → acceptance criteria**

| # | Situation | Story AC |
|---|---|---|
| 1 | Duplicate payment request | US-4.02 AC1 |
| 2 | ElectroCo answers after 12 s | US-4.05 AC1 |
| 3 | AquaCo refuses with an error code | US-5.02 AC2 |
| 4 | Concurrent payments, balance covers only one | US-4.07 AC1 |
| 5 | SMS provider down | US-8.02 AC1 |
| 6 | Duplicate TopUpNet callback | US-5.03 AC2 |
| 7 | Settlement file lists a payment PayLink has as Failed | US-10.01 AC2 |
| 8 | New biller added | US-5.04 AC1 |

# 12. Final check

| Check | Result |
|---|---|
| Phase 1 user stories | **36** (minimum 25) |
| By priority | Must 29 · Should 5 · Could 2 |
| Must FRs covered | 19 / 19 |
| FRs covered | 28 / 28 |
| Hard cases mapped to an AC | 8 / 8 |
| Measurable NFRs | 10 (minimum 6) |
| Stories without a negative or edge AC | 0 |
| Questions raised and decided | 54 questions → 39 decisions |

\newpage

# Appendix A — Configuration keys

| Area | Keys |
|---|---|
| Limits | `limits.daily.basic`, `limits.daily.verified`, `limits.daily.timezone`, `limits.payment.min`, `limits.payment.max`, `topup.min`, `topup.max` |
| Fees | `fees.electricity.fixed`, `fees.water.fixed`, `fees.mobile.rate`, `fees.mobile.min`, `fees.internet.threshold`, `fees.internet.low`, `fees.internet.high`, `fees.promotion.biller_id`, `fees.promotion.start`, `fees.promotion.end`, `commission.agent.rate` |
| Money | `money.scale`, `money.rounding_mode` |
| Payment flow | `quote.ttl_minutes`, `idempotency.ttl_hours`, `reversal.include_fee` |
| Billers | `billers.<id>.enabled`, `billers.timeout.live_seconds`, `billers.topupnet.callback_timeout_minutes`, `billers.electroco.api_version` |
| Unknown handling | `retries.unknown.delays_minutes`, `unknown.escalation_hours`, `reconciliation.file_due_time` |
| Fraud | `fraud.velocity.max_payments`, `fraud.velocity.window_seconds`, `fraud.velocity.block_minutes` |
| Notifications | `notifications.retry.max_retries`, `notifications.retry.backoff_base_seconds` |
| Config | `config.versioning.enabled` |

# Appendix B — Ledger entry types and data entities (input to the design pack)

**Ledger entry types:** TOPUP, HOLD, RELEASE, CHARGE, FEE_INCOME, COMMISSION, COMMISSION_REVERSAL, REVERSAL, ADJUSTMENT.

| Entity | Purpose | Source |
|---|---|---|
| Customer | Registered wallet holder, verification level | FR-1, FR-2 |
| Agent | Shop with a PayLink terminal | FR-17 |
| StaffUser | Support and admin users | FR-6, FR-20 |
| Wallet | EGP account; balance from the ledger | §2.3, FR-3 |
| LedgerEntry | Every money movement | FR-5 |
| Biller | Utility company, protocol, enabled, API version | §2.4, FR-6 |
| Service | What a biller sells, min/max | FR-6, FR-7 |
| ServiceInputField | Inputs a service requires | FR-7 |
| Quote | Locked amount, fee, total, expiry | FR-8, D-03 |
| Payment | One bill payment and its state | FR-9 |
| PaymentStateHistory | Full transition history | FR-11, FR-20 |
| IdempotencyRecord | Turns duplicate requests into one | FR-10 |
| TopUp | Money added to a wallet | FR-4 |
| Reversal | FR-27 return of a Completed payment | FR-27, D-11 |
| AccountBlock | VELOCITY or MANUAL block | FR-19, FR-21 |
| BillerCallback | Callback log (applied, duplicate, conflict) | FR-14 |
| Notification | One message on one channel | FR-22, FR-23 |
| NotificationTemplate | Message text | §3.1 |
| SettlementFile | A biller's daily file | §2.2, D-13 |
| ReconciliationReport | One reconciliation run | FR-26 |
| ReconciliationDifference | One flagged difference | §2.7 #7 |
| ConfigVersion | Versioned fee and limit values | FR-24 |
| AuditRecord | Who, when, why | FR-25 |
