# Release Communication and Readiness Brief Assistant

 http://releasecommunication.runasp.net/ 

A medium-sized ASP.NET Core MVC application that helps teams prepare, validate, review, and finalize release communication briefs using deterministic readiness checks and an AI-assisted analysis workflow.

The application is designed with a human-in-the-loop review process so that AI-generated statements can be reviewed, edited, approved, or rejected before being included in the final release brief.

---

## Project Overview

Release communication often requires information from development, QA, and release management teams.

This application provides a centralized workflow to:

- Capture structured release information
- Record release features and bug fixes
- Capture QA evidence
- Validate release readiness
- Identify missing release information
- Detect unsupported claims
- Classify user impact
- Generate technical release summaries
- Generate non-technical stakeholder summaries
- Review and approve AI-generated statements
- Preserve release versions
- Compare release versions
- Detect stale AI-generated statements after source changes
- Generate a final reviewed release brief

The AI workflow is advisory only. Important release decisions remain under human control.

---

## Key Features

### Release Package Management

Each release can contain:

- Completed features
- Bug fixes
- Changed behaviour
- QA summary
- Known limitations
- Migration/configuration notes
- Affected user groups

### Release Items

Release items can represent:

- Features
- Bug fixes
- Changed behaviour

Each item includes:

- Evidence code
- Title
- Description
- User impact
- Impact reason
- Affected users

### QA Evidence

QA evidence can be recorded with:

- Evidence code
- Test area
- Evidence description
- Test result
- Test date

QA evidence is used by the AI workflow when validating generated claims.

---

## AI-Assisted Release Analysis

The application uses an AI workflow to assist with release analysis.

### Missing Information Analysis

The AI reviews the release package and identifies:

- Missing required information
- Additional concerns
- Evidence or documentation gaps
- Overall release-package completeness

### Unsupported Claim Detection

Generated statements are checked against the supplied release and QA evidence.

The workflow identifies:

- Supported claims
- Unsupported claims
- Evidence gaps
- Overall support status

AI-generated claims are not automatically trusted.

### Technical Summary

The system can generate an internal technical summary containing:

- Release changes
- Bug fixes
- Changed behaviour
- QA evidence
- Risks
- Limitations
- Technical considerations

### Stakeholder Summary

A separate non-technical summary is generated for stakeholders, focusing on:

- Who is affected
- What changed
- QA status
- Risks
- Limitations
- Important user-facing information

---

## Human Review Workflow

AI-generated statements require human review.

A reviewer can:

- Approve a statement
- Reject a statement
- Edit a statement
- Add review comments
- Record reviewer information

The system preserves review decisions for traceability.

### Important AI Boundary

The AI does **not**:

- Deploy the application
- Approve the release
- Automatically publish a release
- Perform production changes

Final decisions remain with the human reviewer.

---

## Version History

Release versions are preserved so that changes can be tracked over time.

The application supports:

- Creating release versions
- Preserving previous release snapshots
- Viewing historical package information
- Comparing release versions

Version comparison includes:

- Release package changes
- Release item changes
- QA evidence changes

---

## Stale Statement Detection

AI-generated statements depend on release information and QA evidence.

If supporting information changes after a statement has been generated, the application marks the statement as **stale**.

Example:

> QA evidence was changed after this statement was generated. Review the statement again.

This prevents previously reviewed AI content from being treated as valid after its supporting evidence changes.

A reviewer must review the statement again before it can be considered current.

---

## Release Readiness

The application performs deterministic readiness checks for required release sections.

The readiness workflow validates:

- Completed features
- Bug fixes
- Changed behaviour
- QA summary
- Known limitations
- Migration/configuration notes
- Affected user groups

The system clearly identifies missing sections when the release package is incomplete.

---

## Risks and Limitations

The application identifies release risks and known limitations based on the supplied release information and evidence.

Examples include:

- High-impact changes
- Medium-impact changes
- Known functional limitations
- Configuration requirements
- QA evidence gaps

---

## Technology Stack

### Backend

- C#
- .NET 8
- ASP.NET Core MVC
- Entity Framework Core 8
- SQL Server
- HTTP-based AI integration

### Frontend

- Razor Views
- HTML
- CSS
- JavaScript
- Bootstrap

### AI

- Groq API
- `openai/gpt-oss-20b`

The application uses an HTTP-based integration with the Groq API.

### Testing

- xUnit
- .NET test project

### Development Tools

- Visual Studio 2022
- SQL Server
- SQL Server Object Explorer
- Git
- GitHub

### Deployment

- MonsterASP.NET
- IIS
- Web Deploy

---

## Project Structure

```text
ReleaseCommunicationAssistant-Task/
│
├── Controllers/
│   ├── HomeController.cs
│   ├── ReleasesController.cs
│   ├── ReleaseItemsController.cs
│   ├── QaevidencesController.cs
│   ├── GeneratedStatementsController.cs
│   ├── ReviewDecisionsController.cs
│   ├── ReadinessController.cs
│   ├── RisksController.cs
│   ├── VersionHistoryController.cs
│   ├── VersionComparisonController.cs
│   └── FinalBriefController.cs
│
├── Models/
│   ├── Release.cs
│   ├── ReleaseItem.cs
│   ├── QAEvidence.cs
│   ├── GeneratedStatements.cs
│   ├── ReviewDecision.cs
│   ├── ReleaseVersionHistory.cs
│   └── AppDbContext.cs
│
├── Services/
│   └── OpenAIReleaseAnalysisService.cs
│
├── Views/
│   ├── Home/
│   ├── Releases/
│   ├── ReleaseItems/
│   ├── Qaevidences/
│   ├── GeneratedStatements/
│   ├── ReviewDecisions/
│   ├── Readiness/
│   ├── Risks/
│   ├── VersionHistory/
│   ├── VersionComparison/
│   └── FinalBrief/
│
├── Migrations/
│
├── wwwroot/
│   ├── css/
│   ├── js/
│   └── lib/
│
├── Program.cs
├── appsettings.json
├── .env.example
├── .gitignore
├── AGENT_USAGE.md
├── README.md
└── ReleaseCommunicationAssistant Task.csproj




Database Design
The application uses SQL Server with Entity Framework Core.
Main tables:
Releases
   │
   ├── ReleaseItems
   │
   ├── QAEvidences
   │
   ├── GeneratedStatements
   │       │
   │       └── ReviewDecisions
   │
   └── ReleaseVersionHistories




Releases
Stores the main release package.
ReleaseItems
Stores individual features, bug fixes, and behaviour changes.
QAEvidences
Stores QA/test evidence associated with releases.
GeneratedStatements
Stores AI-generated statements and their evidence/support status.
ReviewDecisions
Stores human review actions.
ReleaseVersionHistories
Stores snapshots of previous release versions.



Local Setup
1. Clone the Repository
git clone https://github.com/mk1958/ReleaseCommunicationAssistant-Task.git

Open the project in Visual Studio 2022.
2. Configure SQL Server
Create a SQL Server database and update the connection string.
Example:
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ReleaseCommunicationAssistantDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}

3. Apply Entity Framework Migrations
Open:
Tools
→ NuGet Package Manager
→ Package Manager Console

Run:
Update-Database

AI Configuration
The application uses a Groq API key.
Do not commit API keys to GitHub.
Configure the secret using .NET User Secrets.
Initialize User Secrets:
dotnet user-secrets init

Then configure:
dotnet user-secrets set "Groq:ApiKey" "YOUR_GROQ_API_KEY"

The repository contains only configuration placeholders.
Environment Variables
An example configuration is provided in:
.env.example

Example:
Groq__ApiKey=YOUR_GROQ_API_KEY
ConnectionStrings__DefaultConnection=YOUR_CONNECTION_STRING

Never commit real credentials.
Running the Application
From Visual Studio:
1. Open the solution.
2. Restore NuGet packages.
3. Verify the SQL Server connection.
4. Configure the Groq API key.
5. Apply migrations.
6. Press F5 or Ctrl + F5.
Running Tests
The solution contains a dedicated xUnit test project.
Run:
dotnet test

The tests cover important deterministic application behaviour including:
- Release readiness
- Risk and limitation detection
- User impact classification
- QA evidence behaviour
- Version comparison
- Stale statement detection
- Review decision behaviour
Architecture
The application follows a standard ASP.NET Core MVC structure.
Browser
   │
   ▼
MVC Controllers
   │
   ├──────────────► Services
   │                  │
   │                  ▼
   │              Groq AI API
   │
   ▼
Entity Framework Core
   │
   ▼
SQL Server

Deterministic Logic
Business-critical checks such as release readiness and stale-state detection are handled by application logic rather than relying solely on AI.
AI Logic
AI is used for tasks where language understanding provides value:
- Missing information analysis
- Unsupported claim analysis
- Technical summary generation
- Stakeholder summary generation
Human Review
AI-generated content passes through a review workflow before being treated as final.
Evidence-Based AI Workflow
The AI workflow is designed to reduce unsupported claims.
The general flow is:
Release Package
      │
      ▼
Required Information Check
      │
      ▼
QA Evidence
      │
      ▼
AI Analysis
      │
      ├── Missing Information
      │
      ├── Unsupported Claims
      │
      ├── Technical Summary
      │
      └── Stakeholder Summary
      │
      ▼
Human Review
      │
      ├── Approve
      ├── Reject
      └── Edit
      │
      ▼
Final Release Brief

Demo Data
The application contains demonstration release data.
Current local dataset:
Releases:                  2
Release Items:            30
QA Evidence records:      14
Generated Statements:      2
Review Decisions:          4
Version History records:   2

Deployment
The application is designed to run as an ASP.NET Core application hosted on IIS.
Deployment configuration:
.NET 8
IIS
Web Deploy
MonsterASP.NET
SQL Server

Hosted application:
http://releasecommunication.runasp.net/

Deployment availability may depend on the hosting environment.

Scope Completed
- [x] Release package management
- [x] Release item management
- [x] QA evidence management
- [x] Release readiness checks
- [x] User impact classification
- [x] Missing information analysis
- [x] Unsupported claim detection
- [x] Technical summary generation
- [x] Stakeholder summary generation
- [x] Human review workflow
- [x] Approve/reject/edit workflow
- [x] Review decision persistence
- [x] Release version history
- [x] Version comparison
- [x] Stale statement detection
- [x] Risk and limitation analysis
- [x] Final release brief
- [x] SQL Server persistence
- [x] Structured AI service logging
- [x] xUnit tests
- [x] GitHub repository
- [x] Deployment configuration
- [x] Documentation

Intentionally Excluded
The following features were intentionally kept outside the project scope:
- Git provider integration
- Automatic production deployment
- Automatic rollback
- Public changelog publishing
- Automatic release approval
- Automatic production changes


These exclusions keep the application focused on the core release communication and readiness workflow.
Responsible AI Design
The application follows a human-in-the-loop approach.
AI output is treated as assistance rather than authoritative truth.
Important safeguards include:
- Evidence-based claim validation
- Human review
- Explicit approval/rejection
- Stale statement detection
- Persistent review history
- No automatic deployment
- No automatic release approval
- No automatic production changes


Limitations
Current limitations include:
- AI output depends on the quality of supplied release information and QA evidence.
- AI-generated summaries require human review.
- The application does not integrate directly with Git providers.
- The application does not perform actual deployment automation.
- The application does not automatically publish public changelogs.
- External AI availability depends on the configured Groq service.
- SMTP-dependent functionality requires appropriate SMTP configuration.


Documentation
Additional documentation is available in:
AGENT_USAGE.md
Documents:
- Tools used
- Representative prompts
- Delegated development work
- Agent mistakes or rejected suggestions
- Verification performed
.env.example
Documents required configuration names without exposing secrets.
Security Notes
Never commit:
- API keys
- Passwords
- Database credentials
- User secrets
- Production connection strings
The repository uses .gitignore rules to prevent sensitive local configuration from being committed.



Author
B. Mohan Krishna
.NET Developer | ASP.NET Core | C# | SQL Server | AI-Assisted Development
GitHub:
https://github.com/mk1958

License
This project was created for assessment and demonstration purposes.

**Copy the entire block above into `README.md`, save it, commit it, and push it to GitHub.**
