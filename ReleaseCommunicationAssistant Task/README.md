# Release Communication & Readiness Brief Assistant

## Overview

Release Communication & Readiness Brief Assistant is an ASP.NET Core MVC application designed to help teams prepare evidence-driven release communication.

The application combines deterministic release-readiness checks with an AI-assisted workflow to analyze release information, validate claims against QA evidence, generate technical and stakeholder summaries, and support human review before a release brief is finalized.

The AI workflow does not deploy software or approve a release for deployment.

---

## Technology Stack

- ASP.NET Core 8 MVC
- C#
- Entity Framework Core 8
- SQL Server
- Razor Views
- Bootstrap
- Groq API
- xUnit

---

## Core Workflow

1. Create and maintain a structured release package.
2. Add release items such as features, bug fixes, and changed behaviour.
3. Add QA evidence with evidence codes.
4. Run deterministic release-readiness checks.
5. Classify user impact.
6. Identify risks and known limitations.
7. Use AI to identify missing release information.
8. Use AI to detect claims that are not supported by supplied QA evidence.
9. Generate technical and nontechnical stakeholder summaries.
10. Create generated statements with evidence references.
11. Review, approve, reject, or edit generated statements.
12. Preserve release versions and snapshots.
13. Compare release versions.
14. Mark generated statements stale when underlying release information changes.
15. Re-review stale statements.
16. Generate the final reviewed release brief.

---

## AI Workflow

The application uses the Groq API for AI-assisted analysis.

### AI capabilities

- Missing release information analysis
- Unsupported claim detection
- Technical release summary generation
- Nontechnical stakeholder summary generation

AI-generated content is based on the supplied release package and QA evidence.

The workflow includes human review before generated statements are included as reviewed release communication.

### AI safety boundary

The AI does not:

- Deploy software
- Approve a release for deployment
- Perform production deployment actions
- Automatically publish a public changelog

Human review remains part of the release communication workflow.

---

## Release Readiness

The application performs deterministic checks for the required release sections:

- Completed Features
- Bug Fixes
- Changed Behaviour
- QA Summary
- Known Limitations
- Migration / Configuration Notes
- Affected User Groups

The readiness screen identifies missing sections before a release is considered complete.

---

## Evidence and Validation

Release items and QA evidence use evidence codes such as:

- F-001
- B-001
- CB-001
- QA-001

Evidence references are included in generated communication where applicable.

The unsupported-claim workflow compares generated statements against supplied QA evidence and identifies:

- Supported claims
- Unsupported claims
- Reasons for unsupported claims
- Evidence gaps

---

## Human Review

Generated statements can be reviewed by a human reviewer.

Supported review actions include:

- Approve
- Reject
- Edit

Review decisions record reviewer information and review details.

A generated statement that becomes stale must be reviewed again before it can be treated as current reviewed content.

---

## Version History and Comparison

The application preserves release version information and snapshots.

Version history includes release package information and snapshots of associated release items and QA evidence.

The version comparison workflow identifies changes between release versions, including changes to:

- Release package fields
- Release items
- QA evidence

---

## Stale Statement Detection

When underlying release information changes, related generated statements can become stale.

For example:

1. A statement is generated and reviewed.
2. Related QA evidence is changed.
3. The statement is marked stale.
4. The stale reason is displayed.
5. The statement must be reviewed again.
6. After review, it can be used in the final release brief.

---

## Final Release Brief

The final release brief provides:

- Release information
- Internal technical summary
- Nontechnical stakeholder summary
- Reviewed statements
- Supporting evidence
- Risks
- Known limitations
- Review boundary

The final brief does not deploy software or mark a release as approved for deployment.

---

## Testing

The solution includes an xUnit test project.

Current focused test coverage includes:

- Release readiness
- Risk detection
- Limitation detection
- User-impact classification
- QA evidence behaviour
- Version comparison
- Stale statement behaviour
- Review decisions

Current test result:

**15 tests passed, 0 failed.**

---

## Configuration

Create the required configuration using environment variables or local user secrets.

Example configuration names are provided in:

`.env.example`

Do not commit real API keys, passwords, connection strings, or other secrets.

For local development, the Groq API key is stored using ASP.NET Core User Secrets.

---

## Database

The application uses SQL Server with Entity Framework Core.

Database migrations are used to maintain the application schema.

The database contains data for:

- Releases
- Release Items
- QA Evidence
- Generated Statements
- Review Decisions
- Release Version History

---

## Setup

### Prerequisites

- .NET 8 SDK
- SQL Server
- Visual Studio 2022 or another .NET development environment
- Groq API key

### Steps

1. Clone the repository.
2. Open the solution in Visual Studio.
3. Configure the SQL Server connection string.
4. Configure the Groq API key using User Secrets or environment configuration.
5. Restore NuGet packages.
6. Apply Entity Framework migrations.
7. Build the solution.
8. Run the application.

---

## Running Tests

From the solution directory, run:

```bash
dotnet test