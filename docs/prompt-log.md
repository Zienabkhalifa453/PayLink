# Prompt log

Every use of an AI tool on PayLink is recorded here, including answers that were wrong or that we rejected.
Format follows section 5 of the student brief.

**Verdicts:** **Accepted** = used as given after checking · **Fixed** = used after we corrected it · **Rejected** = not used.

## Summary (Phase 1)

| Metric | Value |
|---|---|
| Entries | 9 |
| Accepted | 3 (P1, P8, P9) |
| Fixed | 4 (P2, P3, P4, P7) |
| Rejected | 2 (P5, P6) |
| First answer wrong, prompt improved | 2 (P6, P7) |
| AI errors caught by our pipeline (lint/tests) | 2 (P3, P4) |

## Log

| # | Date | Author | Stage | Tool and model | Prompt | Summary of answer | Verdict and why |
|---|---|---|---|---|---|---|---|
| P1 | 2026-11-25 | Zienab Alaa | Requirements | Claude Code (Claude Opus 5.5) | "I need you to read and understand this file very well and then we will move next" + the brief PDF | Structured summary of the brief: payment flow, billers, life cycle, business rules, 8 hard cases, deliverables. Flagged that the Phase 1 deadline was the same day. | **Accepted.** Checked against brief sections 2-7: numbers and states matched. Useful warning about the deadline. |
| P2 | 2026-11-25 | Zienab Alaa | DevOps | Claude Code (Claude Opus 5.5) | "The Docker/CI skeleton? I should do this section but I need it structured very well and in a good way, make all this" | Asked which stack first (locked after Phase 1); we chose .NET 8. Generated a modular monolith (API host, shared kernel, 8 module projects), mock-billers service, docker compose (db + mocks + api), 2-job GitHub Actions pipeline and 7 tests. | **Fixed.** Structure accepted, but 3 problems had to be corrected before it worked (P3, P4, P5). |
| P3 | 2026-11-25 | Zienab Alaa | DevOps (lint) | Claude Code (Claude Opus 5.5) | (same session as P2) | The generated `.editorconfig` required a `_` prefix on *every* private field, including constants. | **Fixed.** `dotnet format --verify-no-changes` failed with IDE1006 on the constant `ModulePrefix`. Added a PascalCase rule for constants above the field rule. Our lint step caught the AI's mistake. |
| P4 | 2026-11-25 | Zienab Alaa | Testing | Claude Code (Claude Opus 5.5) | (same session as P2) | The generated `coverage.runsettings` contained an XML comment with `--`, which is illegal in XML. | **Fixed.** `dotnet test` printed "Settings file provided does not conform to required format" and ran **zero tests** without reporting a failure. Rewrote the comment; 7/7 tests then ran. Lesson: check the test count, not only the exit code. |
| P5 | 2026-11-25 | Zienab Alaa | DevOps (coverage) | Claude Code (Claude Opus 5.5) | (same session as P2) | Tried to install the ReportGenerator tool on the laptop to build an HTML coverage report. | **Rejected.** We do not install extra tools on team machines. Coverage HTML is generated only in CI with the ReportGenerator GitHub Action; locally we keep the raw Cobertura files. |
| P6 | 2026-11-25 | Zienab Alaa | DevOps (Git) | Claude Code (Claude Opus 5.5) | "Make PayLink in Desktop and make repo and push it" → improved: "I don't need to show Claude participation here, I need to be the only one to deliver it" | First answer: committed with a `Co-Authored-By: Claude` trailer, so GitHub listed "claude" as a contributor. After the improved prompt: trailer removed, commit rewritten; the contributor list stayed cached, so the repository was recreated. | **Rejected** (first answer). The team submits the work, so only team members should be contributors. The AI-use note required by brief §5 stays in the commit subject. |
| P7 | 2026-11-25 | Zienab Alaa | DevOps (Git) | Claude Code (Claude Opus 5.5) | "Still exist the commit that contains (AI: Claude Opus 5.5)" → "Yes, short it" | AI refused to delete the AI-use note because brief §5 requires naming the AI tool in the commit message; it offered a shorter form instead. | **Fixed.** Subject shortened to `(AI: Claude)`. Still meets §5, less noise. A good example of the AI pushing back with the brief's own rule. |
| P8 | 2026-11-25 | Zienab Alaa | DevOps (GitHub) | Claude Code (Claude Opus 5.5) | "This correct?" + screenshots of the branch protection form, then "done, please check" | Listed missing settings, then checked the GitHub API and found the **required status checks list was empty** even though the rule existed. After we added both checks it re-verified: protected, no bypass, 2 required checks. | **Accepted.** It found a real gap the GitHub UI made easy to miss: without required checks a red pipeline could still be merged. |
| P9 | 2026-11-25 | Zienab Alaa | Documentation | Claude Code (Claude Opus 5.5) | "Prompt log and phase-1 tag and PDF report, make this please" | Drafted this prompt log and the Phase 1 report (HTML source + PDF). | **Accepted** after review by the team (update this verdict if you change anything). |

## Prompt-improvement notes

Our early prompts were short and unstructured. Section 5 of the brief asks for **role, context, constraints, output format and an example**.
These are the improved versions we will use from now on:

**P2 (Docker/CI), improved:**

> **Role:** You are a DevOps engineer for a .NET 8 university project.
> **Context:** PayLink, a bill-payment backend; brief section 3.4 and Phase 1 item 3 attached.
> **Constraints:** .NET 8, PostgreSQL 16, docker compose must start with no `.env` file, CI on GitHub Actions must build, lint and run at least one test on every push, no secrets in the repo.
> **Output:** the file tree first, then each file, then the exact commands to verify it locally.

**P6 (Git), improved:**

> Create a local git repository in `Desktop\paylink`, commit everything as me only (no co-author lines), but keep the AI tool name in the commit subject as brief §5 requires. Do not push until I give you the repository URL.

## Phase 2 entries

(add new entries below; the brief asks for at least 5 more, including at least 5 AI suggestions rejected or fixed)
