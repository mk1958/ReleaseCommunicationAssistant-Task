using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace ReleaseCommunicationAssistant_Task.Services
{
    public class OpenAIReleaseAnalysisService
    {
        private const string ModelName = "openai/gpt-oss-20b";

        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly ILogger<OpenAIReleaseAnalysisService> _logger;

        public OpenAIReleaseAnalysisService(
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            ILogger<OpenAIReleaseAnalysisService> logger)
        {
            _configuration = configuration;
            _httpClient = httpClientFactory.CreateClient();
            _logger = logger;
        }

        public async Task<string> AnalyzeMissingInformationAsync(
            string releaseVersion,
            string releaseInformation)
        {
            const string operation = "MissingInformationAnalysis";
            var stopwatch = Stopwatch.StartNew();

            _logger.LogInformation(
                "AI operation started. Operation={Operation} ReleaseVersion={ReleaseVersion} Model={Model}",
                operation,
                releaseVersion,
                ModelName);

            try
            {
                var apiKey = _configuration["Groq:ApiKey"];

                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    _logger.LogError(
                        "AI operation failed because Groq API key is not configured. Operation={Operation} ReleaseVersion={ReleaseVersion}",
                        operation,
                        releaseVersion);

                    throw new InvalidOperationException(
                        "Groq API key is not configured.");
                }

                var prompt = $"""
                    You are a release readiness assistant.

                    Analyze the following release package for version {releaseVersion}.

                    Identify important release information that is missing or insufficient.

                    The required release sections are:
                    1. Completed Features
                    2. Bug Fixes
                    3. Changed Behaviour
                    4. QA Summary
                    5. Known Limitations
                    6. Migration / Configuration Notes
                    7. Affected User Groups

                    Rules:
                    - Only identify information that is actually missing or insufficient.
                    - Do not invent facts.
                    - Do not assume that an empty section is complete.
                    - Explain why each missing section matters.
                    - Keep the response concise and suitable for a release-readiness review.

                    Release package:
                    {releaseInformation}

                    Return the result using this structure:

                    Missing Information:
                    - <section>: <reason>

                    Additional Concerns:
                    - <concern>

                    Overall Assessment:
                    <short assessment>
                    """;

                var requestBody = new
                {
                    model = ModelName,
                    messages = new[]
                    {
                        new
                        {
                            role = "user",
                            content = prompt
                        }
                    },
                    temperature = 0.2
                };

                var json = JsonSerializer.Serialize(requestBody);

                using var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    "https://api.groq.com/openai/v1/chat/completions");

                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", apiKey);

                request.Content = new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json");

                using var response = await _httpClient.SendAsync(request);

                var responseContent =
                    await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        "AI operation failed. Operation={Operation} ReleaseVersion={ReleaseVersion} Model={Model} StatusCode={StatusCode} DurationMs={DurationMs}",
                        operation,
                        releaseVersion,
                        ModelName,
                        (int)response.StatusCode,
                        stopwatch.ElapsedMilliseconds);

                    throw new InvalidOperationException(
                        $"Groq API request failed ({(int)response.StatusCode}): {responseContent}");
                }

                using var document =
                    JsonDocument.Parse(responseContent);

                var result =
                    document.RootElement
                        .GetProperty("choices")[0]
                        .GetProperty("message")
                        .GetProperty("content")
                        .GetString();

                stopwatch.Stop();

                _logger.LogInformation(
                    "AI operation completed successfully. Operation={Operation} ReleaseVersion={ReleaseVersion} Model={Model} DurationMs={DurationMs}",
                    operation,
                    releaseVersion,
                    ModelName,
                    stopwatch.ElapsedMilliseconds);

                return result ?? "No AI analysis was returned.";
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                _logger.LogError(
                    ex,
                    "AI operation failed unexpectedly. Operation={Operation} ReleaseVersion={ReleaseVersion} Model={Model} DurationMs={DurationMs}",
                    operation,
                    releaseVersion,
                    ModelName,
                    stopwatch.ElapsedMilliseconds);

                throw;
            }
        }


        public async Task<string> AnalyzeUnsupportedClaimsAsync(
            string statementText,
            string qaEvidence)
        {
            const string operation = "UnsupportedClaimsAnalysis";
            var stopwatch = Stopwatch.StartNew();

            _logger.LogInformation(
                "AI operation started. Operation={Operation} Model={Model}",
                operation,
                ModelName);

            try
            {
                var apiKey = _configuration["Groq:ApiKey"];

                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    _logger.LogError(
                        "AI operation failed because Groq API key is not configured. Operation={Operation}",
                        operation);

                    throw new InvalidOperationException(
                        "Groq API key is not configured.");
                }

                var prompt = $"""
                    You are a release quality and evidence validation assistant.

                    Analyze the generated release statement below and determine
                    whether its claims are supported by the supplied QA evidence.

                    Generated Statement:
                    {statementText}

                    Supplied QA Evidence:
                    {qaEvidence}

                    Rules:
                    - Do not invent evidence.
                    - A claim is supported only when the supplied QA evidence
                      provides reasonable evidence for that claim.
                    - Identify every claim that is unsupported or insufficiently supported.
                    - Reference the relevant QA evidence code when evidence supports a claim.
                    - Clearly distinguish Supported Claims from Unsupported Claims.
                    - If there is insufficient evidence, say so explicitly.
                    - Do not assume that a successful QA result proves claims that
                      were not actually tested.

                    Return the result using exactly this structure:

                    Supported Claims:
                    - <claim> — Evidence: <evidence code>

                    Unsupported Claims:
                    - <claim> — Reason: <reason>

                    Evidence Gaps:
                    - <missing evidence>

                    Overall Assessment:
                    <Supported / Partially Supported / Unsupported>
                    """;

                var requestBody = new
                {
                    model = ModelName,
                    messages = new[]
                    {
                        new
                        {
                            role = "user",
                            content = prompt
                        }
                    },
                    temperature = 0.1
                };

                var json = JsonSerializer.Serialize(requestBody);

                using var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    "https://api.groq.com/openai/v1/chat/completions");

                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", apiKey);

                request.Content = new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json");

                using var response = await _httpClient.SendAsync(request);

                var responseContent =
                    await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        "AI operation failed. Operation={Operation} Model={Model} StatusCode={StatusCode} DurationMs={DurationMs}",
                        operation,
                        ModelName,
                        (int)response.StatusCode,
                        stopwatch.ElapsedMilliseconds);

                    throw new InvalidOperationException(
                        $"Groq API request failed ({(int)response.StatusCode}): {responseContent}");
                }

                using var document =
                    JsonDocument.Parse(responseContent);

                var result =
                    document.RootElement
                        .GetProperty("choices")[0]
                        .GetProperty("message")
                        .GetProperty("content")
                        .GetString();

                stopwatch.Stop();

                _logger.LogInformation(
                    "AI operation completed successfully. Operation={Operation} Model={Model} DurationMs={DurationMs}",
                    operation,
                    ModelName,
                    stopwatch.ElapsedMilliseconds);

                return result ?? "No unsupported-claim analysis was returned.";
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                _logger.LogError(
                    ex,
                    "AI operation failed unexpectedly. Operation={Operation} Model={Model} DurationMs={DurationMs}",
                    operation,
                    ModelName,
                    stopwatch.ElapsedMilliseconds);

                throw;
            }
        }


        public async Task<string> GenerateTechnicalSummaryAsync(
            string releaseVersion,
            string releaseInformation)
        {
            const string operation = "TechnicalSummaryGeneration";
            var stopwatch = Stopwatch.StartNew();

            _logger.LogInformation(
                "AI operation started. Operation={Operation} ReleaseVersion={ReleaseVersion} Model={Model}",
                operation,
                releaseVersion,
                ModelName);

            try
            {
                var apiKey = _configuration["Groq:ApiKey"];

                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    _logger.LogError(
                        "AI operation failed because Groq API key is not configured. Operation={Operation} ReleaseVersion={ReleaseVersion}",
                        operation,
                        releaseVersion);

                    throw new InvalidOperationException(
                        "Groq API key is not configured.");
                }

                var prompt = $"""
                    You are a senior software release communication assistant.

                    Generate an INTERNAL TECHNICAL RELEASE SUMMARY
                    for release version {releaseVersion}.

                    Use ONLY the supplied release package, release items,
                    and QA evidence.

                    Rules:
                    - Do not invent features, fixes, test results, metrics,
                      performance improvements, or technical details.
                    - Clearly separate completed features, bug fixes,
                      changed behaviour, and QA results.
                    - Include evidence codes next to relevant claims.
                    - Mention known limitations or risks when supported
                      by the supplied information.
                    - Do not claim that the release is approved.
                    - Do not claim that the release has been deployed.
                    - Keep the summary professional and concise.

                    Release information:
                    {releaseInformation}

                    Return using this structure:

                    TECHNICAL RELEASE SUMMARY

                    Completed Features:
                    - <feature> [Evidence: <code>]

                    Bug Fixes:
                    - <bug fix> [Evidence: <code>]

                    Changed Behaviour:
                    - <change> [Evidence: <code>]

                    QA Summary:
                    - <test result> [Evidence: <code>]

                    Risks / Limitations:
                    - <risk or limitation> [Evidence: <code>]

                    Overall Technical Notes:
                    <short technical summary>
                    """;

                var requestBody = new
                {
                    model = ModelName,
                    messages = new[]
                    {
                        new
                        {
                            role = "user",
                            content = prompt
                        }
                    },
                    temperature = 0.2
                };

                var json = JsonSerializer.Serialize(requestBody);

                using var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    "https://api.groq.com/openai/v1/chat/completions");

                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", apiKey);

                request.Content = new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json");

                using var response = await _httpClient.SendAsync(request);

                var responseContent =
                    await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        "AI operation failed. Operation={Operation} ReleaseVersion={ReleaseVersion} Model={Model} StatusCode={StatusCode} DurationMs={DurationMs}",
                        operation,
                        releaseVersion,
                        ModelName,
                        (int)response.StatusCode,
                        stopwatch.ElapsedMilliseconds);

                    throw new InvalidOperationException(
                        $"Groq API request failed ({(int)response.StatusCode}): {responseContent}");
                }

                using var document =
                    JsonDocument.Parse(responseContent);

                var result =
                    document.RootElement
                        .GetProperty("choices")[0]
                        .GetProperty("message")
                        .GetProperty("content")
                        .GetString();

                stopwatch.Stop();

                _logger.LogInformation(
                    "AI operation completed successfully. Operation={Operation} ReleaseVersion={ReleaseVersion} Model={Model} DurationMs={DurationMs}",
                    operation,
                    releaseVersion,
                    ModelName,
                    stopwatch.ElapsedMilliseconds);

                return result ?? "No technical summary was returned.";
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                _logger.LogError(
                    ex,
                    "AI operation failed unexpectedly. Operation={Operation} ReleaseVersion={ReleaseVersion} Model={Model} DurationMs={DurationMs}",
                    operation,
                    releaseVersion,
                    ModelName,
                    stopwatch.ElapsedMilliseconds);

                throw;
            }
        }


        public async Task<string> GenerateStakeholderSummaryAsync(
            string releaseVersion,
            string releaseInformation)
        {
            const string operation = "StakeholderSummaryGeneration";
            var stopwatch = Stopwatch.StartNew();

            _logger.LogInformation(
                "AI operation started. Operation={Operation} ReleaseVersion={ReleaseVersion} Model={Model}",
                operation,
                releaseVersion,
                ModelName);

            try
            {
                var apiKey = _configuration["Groq:ApiKey"];

                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    _logger.LogError(
                        "AI operation failed because Groq API key is not configured. Operation={Operation} ReleaseVersion={ReleaseVersion}",
                        operation,
                        releaseVersion);

                    throw new InvalidOperationException(
                        "Groq API key is not configured.");
                }

                var prompt = $"""
                    You are a release communication assistant.

                    Generate a NON-TECHNICAL STAKEHOLDER RELEASE SUMMARY
                    for release version {releaseVersion}.

                    The audience includes product owners, managers,
                    business stakeholders, and affected users.

                    Use ONLY the supplied release package, release items,
                    and QA evidence.

                    Rules:
                    - Use simple business-friendly language.
                    - Do not invent features, benefits, metrics,
                      performance improvements, or test results.
                    - Explain what changed and who is affected.
                    - Include supporting evidence codes for claims.
                    - Mention known limitations or risks when supported
                      by the supplied information.
                    - Do not claim that the release is approved.
                    - Do not claim that the release has been deployed.

                    Release information:
                    {releaseInformation}

                    Return using this structure:

                    STAKEHOLDER RELEASE SUMMARY

                    What Changed:
                    - <plain-language change> [Evidence: <code>]

                    Who Is Affected:
                    - <affected user group>

                    QA Confidence:
                    - <supported QA result> [Evidence: <code>]

                    Risks / Limitations:
                    - <risk or limitation> [Evidence: <code>]

                    Overall Stakeholder Summary:
                    <short non-technical summary>
                    """;

                var requestBody = new
                {
                    model = ModelName,
                    messages = new[]
                    {
                        new
                        {
                            role = "user",
                            content = prompt
                        }
                    },
                    temperature = 0.2
                };

                var json = JsonSerializer.Serialize(requestBody);

                using var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    "https://api.groq.com/openai/v1/chat/completions");

                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", apiKey);

                request.Content = new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json");

                using var response = await _httpClient.SendAsync(request);

                var responseContent =
                    await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        "AI operation failed. Operation={Operation} ReleaseVersion={ReleaseVersion} Model={Model} StatusCode={StatusCode} DurationMs={DurationMs}",
                        operation,
                        releaseVersion,
                        ModelName,
                        (int)response.StatusCode,
                        stopwatch.ElapsedMilliseconds);

                    throw new InvalidOperationException(
                        $"Groq API request failed ({(int)response.StatusCode}): {responseContent}");
                }

                using var document =
                    JsonDocument.Parse(responseContent);

                var result =
                    document.RootElement
                        .GetProperty("choices")[0]
                        .GetProperty("message")
                        .GetProperty("content")
                        .GetString();

                stopwatch.Stop();

                _logger.LogInformation(
                    "AI operation completed successfully. Operation={Operation} ReleaseVersion={ReleaseVersion} Model={Model} DurationMs={DurationMs}",
                    operation,
                    releaseVersion,
                    ModelName,
                    stopwatch.ElapsedMilliseconds);

                return result ?? "No stakeholder summary was returned.";
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                _logger.LogError(
                    ex,
                    "AI operation failed unexpectedly. Operation={Operation} ReleaseVersion={ReleaseVersion} Model={Model} DurationMs={DurationMs}",
                    operation,
                    releaseVersion,
                    ModelName,
                    stopwatch.ElapsedMilliseconds);

                throw;
            }
        }
    }
}