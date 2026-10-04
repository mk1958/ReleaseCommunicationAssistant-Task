# Agent Usage

## Overview

AI assistance was used during development of the Release Communication & Readiness Brief Assistant to support implementation, debugging, testing, documentation, and review of the assessment requirements.

The final implementation was manually verified through the running application and focused automated tests.

---

## Tools Used

### AI Assistant

Used for:

- Understanding and breaking down the assessment requirements
- Planning the application architecture
- Designing database entities and relationships
- Writing and reviewing C# code
- ASP.NET Core MVC implementation
- Entity Framework Core guidance
- SQL Server and migration guidance
- Razor view implementation
- Debugging runtime and database issues
- Groq API integration
- AI workflow design
- xUnit test design
- Documentation preparation
- Reviewing completed functionality against the assessment requirements

### Development Tools

- Visual Studio 2022
- .NET 8 SDK
- SQL Server / SQL Server Management Studio
- Entity Framework Core
- Git / GitHub
- Groq API

---

## Representative Prompts

Examples of development prompts used during the project included:

- "Help me build the release communication and readiness assistant according to the assessment requirements."
- "Create the database structure for releases, release items, QA evidence, generated statements, and review decisions."
- "Help implement deterministic release readiness checks."
- "Help implement AI missing-information analysis."
- "Help detect unsupported claims using QA evidence."
- "Help implement technical and stakeholder release summaries."
- "Help implement review, approval, rejection, and editing of generated statements."
- "Help detect stale statements when QA evidence changes."
- "Compare the completed application against the assessment requirements."
- "Help create focused xUnit tests for the important application behaviour."

---

## Delegated Work

AI assistance was used to help produce and review:

- C# controller logic
- Entity Framework models
- Database migration guidance
- Razor view changes
- Groq API integration
- AI prompts and response handling
- Deterministic readiness logic
- Risk and limitation detection logic
- User-impact classification logic
- Version history and comparison logic
- Stale statement detection
- Review workflow
- xUnit test cases
- README documentation
- Assessment requirement comparison

The developer reviewed and tested the resulting implementation rather than accepting generated code without verification.

---

## Important Agent Mistakes and Corrections

### OpenAI API quota issue

The initial AI integration used OpenAI, but the API returned an insufficient-quota response.

The AI integration was changed to use the Groq API instead.

The final application uses the Groq OpenAI-compatible chat completions endpoint.

### User Secrets configuration issue

The Groq API key was stored using ASP.NET Core User Secrets.

The application initially did not correctly load the user secrets because the project configuration was missing the required `UserSecretsId`.

The `UserSecretsId` was restored in the project file and the AI functionality was tested successfully afterward.

### Release readiness limitation check

The initial readiness implementation checked release-item text to determine whether known limitations existed.

This was corrected so that the readiness check uses the actual `KnownLimitations` field of the release package.

The corrected implementation was tested successfully.

### Review status mismatch

The database expected review statuses such as:

- Approved
- Rejected
- Edited

The review workflow initially attempted to save values such as `Approve`.

The review action was corrected to normalize the user action to the database-compatible status.

Approve and Reject workflows were then tested.

### Stale statement verification

The stale statement workflow was tested by modifying QA evidence after a statement had been generated.

The statement was correctly marked stale and displayed a reason requiring review.

The statement was then reviewed again before being included in the final release brief.

---

## Rejected or Avoided Suggestions

The project intentionally avoids adding functionality outside the assessment scope.

The following were not implemented because they are outside the required core workflow:

- Git provider integration
- Automated rollback
- Public changelog publishing
- Production deployment actions performed by the AI

The AI is intentionally prevented from approving a release for deployment or deploying software.

---

## Verification Approach

AI-generated suggestions and implementation changes were verified using several methods.

### Manual application verification

The following workflows were tested through the running application:

- Release readiness
- Risk detection
- Limitation detection
- User-impact classification
- QA evidence details
- QA claim checking
- Version history
- Version comparison
- Generated statement creation
- AI missing-information analysis
- AI unsupported-claim analysis
- Review and approval
- Review rejection
- Stale statement detection
- Re-review of stale statements
- Final release brief generation

### Automated testing

The project contains focused xUnit tests covering important deterministic behaviour.

Current result:

**15 tests passed, 0 failed.**

### Evidence-based verification

AI-generated claims are checked against supplied QA evidence rather than being automatically trusted.

Generated statements require human review before they are treated as reviewed release communication.

---

## Human Oversight

AI assistance was treated as a development and release-communication aid rather than an autonomous deployment authority.

The application maintains a human review boundary:

- AI can analyze supplied release information.
- AI can identify potential evidence gaps.
- AI can generate release summaries.
- Humans review generated statements.
- Humans approve or reject generated communication.
- AI cannot deploy software.
- AI cannot mark a release as approved for deployment.

---

## Final Verification

Before submission, the completed workflow was tested end-to-end.

The final release brief successfully displayed:

- Technical summary
- Nontechnical stakeholder summary
- Reviewed statements
- Supporting evidence
- Risks
- Known limitations
- Review boundary

The stale statement workflow was also verified by changing underlying QA evidence, detecting the stale statement, reviewing it again, and regenerating the final release brief.