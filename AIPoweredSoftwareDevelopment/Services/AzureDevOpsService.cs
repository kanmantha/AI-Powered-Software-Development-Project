using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace AIPoweredSoftwareDevelopment.Services
{
    /// <summary>
    /// Azure DevOps service implementation
    /// Provides AI-powered code analysis using Azure DevOps AI services
    /// Note: This is a simplified example - real implementation would use Azure AI services
    /// </summary>
    public class AzureDevOpsService : IACodeAnalysisService
    {
        private readonly HttpClient _httpClient;
        private readonly string _organization;
        private readonly string _patToken;
        private readonly ILogger<AzureDevOpsService> _logger;

        /// <summary>
        /// Constructor - initializes HTTP client with Azure DevOps configuration
        /// Uses Personal Access Token (PAT) for authentication
        /// </summary>
        public AzureDevOpsService(HttpClient httpClient, string organization, string patToken, ILogger<AzureDevOpsService> logger)
        {
            _httpClient = httpClient;
            _organization = organization ?? throw new ArgumentNullException(nameof(organization));
            _patToken = patToken ?? throw new ArgumentNullException(nameof(patToken));
            _logger = logger;
            
            // Azure DevOps API base address
            _httpClient.BaseAddress = new Uri($"https://dev.azure.com/{_organization}/");
            // Azure DevOps uses Basic auth with PAT token
            var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{_patToken}"));
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {authToken}");
        }

        /// <summary>
        /// Analyzes code using Azure DevOps AI services
        /// Returns analysis results with severity levels
        /// </summary>
        public async Task<string> AnalyzeCodeAsync(string code, string analysisType, string technology)
        {
            try
            {
                _logger.LogInformation("Starting Azure DevOps code analysis for {AnalysisType}", analysisType);
                
                var requestBody = new
                {
                    prompt = $"You are an expert code reviewer using {technology}. Analyze the following code for {analysisType} issues:\n\n{code}\n\nProvide specific findings with severity levels (Critical, High, Medium, Low).",
                    max_tokens = 2048
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Call Azure AI service (this would be your Azure OpenAI endpoint)
                var response = await _httpClient.PostAsync("_apis/ai/code-review", content);
                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync();
                using var document = JsonDocument.Parse(responseJson);
                
                var analysis = document.RootElement.GetProperty("analysis");
                var result = analysis.GetString() ?? "No response from AI";
                
                _logger.LogInformation("Azure DevOps analysis completed successfully");
                return result;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed while calling Azure DevOps API");
                return "Error: Unable to reach Azure DevOps API. Please check your internet connection and PAT token.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during Azure DevOps code analysis");
                return $"Error: {ex.Message}";
            }
        }

        /// <summary>
        /// Generates test cases using Azure DevOps AI
        /// Returns generated test code
        /// </summary>
        public async Task<string> GenerateTestCasesAsync(string code, string testType)
        {
            try
            {
                _logger.LogInformation("Generating {TestType} test cases with Azure DevOps", testType);
                
                var requestBody = new
                {
                    prompt = $"Generate {testType} test cases for the following code:\n\n{code}\n\nProvide complete test code with assertions.",
                    max_tokens = 2048
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("_apis/ai/test-generation", content);
                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync();
                using var document = JsonDocument.Parse(responseJson);
                
                var testCode = document.RootElement.GetProperty("testCode");
                var result = testCode.GetString() ?? "No response from AI";
                
                _logger.LogInformation("Azure DevOps test generation completed successfully");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating test cases with Azure DevOps");
                return $"Error: {ex.Message}";
            }
        }

        /// <summary>
        /// Detects bugs using Azure DevOps AI
        /// Returns list of bugs with severity and fixes
        /// </summary>
        public async Task<string> DetectBugsAsync(string code)
        {
            try
            {
                _logger.LogInformation("Detecting bugs with Azure DevOps");
                
                var requestBody = new
                {
                    prompt = $"Analyze the following code for bugs, security vulnerabilities, and potential issues:\n\n{code}\n\nList each issue with severity and suggested fix.",
                    max_tokens = 2048
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("_apis/ai/bug-detection", content);
                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync();
                using var document = JsonDocument.Parse(responseJson);
                
                var bugs = document.RootElement.GetProperty("bugs");
                var result = bugs.GetString() ?? "No response from AI";
                
                _logger.LogInformation("Azure DevOps bug detection completed successfully");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error detecting bugs with Azure DevOps");
                return $"Error: {ex.Message}";
            }
        }

        /// <summary>
        /// Generates DevOps suggestions using Azure DevOps AI
        /// Returns best practices and implementation steps
        /// </summary>
        public async Task<string> GenerateDevOpsSuggestionAsync(string taskDescription, string taskType)
        {
            try
            {
                _logger.LogInformation("Generating DevOps suggestions with Azure DevOps for {TaskType}", taskType);
                
                var requestBody = new
                {
                    prompt = $"You are a DevOps expert. Provide AI-powered suggestions for the following task:\n\nTask Type: {taskType}\nDescription: {taskDescription}\n\nInclude best practices, tools to use, and implementation steps.",
                    max_tokens = 2048
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("_apis/ai/devops-suggestions", content);
                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync();
                using var document = JsonDocument.Parse(responseJson);
                
                var suggestions = document.RootElement.GetProperty("suggestions");
                var result = suggestions.GetString() ?? "No response from AI";
                
                _logger.LogInformation("Azure DevOps suggestions completed successfully");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating DevOps suggestions with Azure DevOps");
                return $"Error: {ex.Message}";
            }
        }
    }
}
