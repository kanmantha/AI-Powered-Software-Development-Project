using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace AIPoweredSoftwareDevelopment.Services
{
    /// <summary>
    /// Claude Code service implementation
    /// Provides AI-powered code analysis using Anthropic's Claude API
    /// </summary>
    public class ClaudeCodeService : IACodeAnalysisService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly ILogger<ClaudeCodeService> _logger;

        /// <summary>
        /// Constructor - initializes HTTP client with Claude API configuration
        /// </summary>
        public ClaudeCodeService(HttpClient httpClient, string apiKey, ILogger<ClaudeCodeService> logger)
        {
            _httpClient = httpClient;
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            _logger = logger;
            
            // Claude API base address and headers
            _httpClient.BaseAddress = new Uri("https://api.anthropic.com/v1/");
            _httpClient.DefaultRequestHeaders.Add("x-api-key", _apiKey);
            _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
        }

        /// <summary>
        /// Analyzes code using Claude 3.5 Sonnet model
        /// Returns detailed analysis with severity levels
        /// </summary>
        public async Task<string> AnalyzeCodeAsync(string code, string analysisType, string technology)
        {
            try
            {
                _logger.LogInformation("Starting Claude code analysis for {AnalysisType}", analysisType);
                
                var requestBody = new
                {
                    model = "claude-3-5-sonnet-20241022",
                    max_tokens = 2048,
                    messages = new[]
                    {
                        new
                        {
                            role = "user",
                            content = $"You are an expert code reviewer using {technology}. Analyze the following code for {analysisType} issues:\n\n{code}\n\nProvide specific findings with severity levels (Critical, High, Medium, Low)."
                        }
                    }
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("messages", content);
                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync();
                using var document = JsonDocument.Parse(responseJson);
                
                // Claude API returns content array with text
                var contentArray = document.RootElement.GetProperty("content");
                var firstContent = contentArray[0];
                var text = firstContent.GetProperty("text");
                
                var result = text.GetString() ?? "No response from AI";
                _logger.LogInformation("Claude analysis completed successfully");
                return result;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed while calling Claude API");
                return "Error: Unable to reach Claude API. Please check your internet connection and API key.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during Claude code analysis");
                return $"Error: {ex.Message}";
            }
        }

        /// <summary>
        /// Generates test cases using Claude API
        /// Returns complete test code with assertions
        /// </summary>
        public async Task<string> GenerateTestCasesAsync(string code, string testType)
        {
            try
            {
                _logger.LogInformation("Generating {TestType} test cases with Claude", testType);
                
                var requestBody = new
                {
                    model = "claude-3-5-sonnet-20241022",
                    max_tokens = 2048,
                    messages = new[]
                    {
                        new
                        {
                            role = "user",
                            content = $"Generate {testType} test cases for the following code:\n\n{code}\n\nProvide complete test code with assertions."
                        }
                    }
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("messages", content);
                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync();
                using var document = JsonDocument.Parse(responseJson);
                
                var contentArray = document.RootElement.GetProperty("content");
                var firstContent = contentArray[0];
                var text = firstContent.GetProperty("text");
                
                var result = text.GetString() ?? "No response from AI";
                _logger.LogInformation("Claude test generation completed successfully");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating test cases with Claude");
                return $"Error: {ex.Message}";
            }
        }

        /// <summary>
        /// Detects bugs using Claude API
        /// Returns bugs with severity and suggested fixes
        /// </summary>
        public async Task<string> DetectBugsAsync(string code)
        {
            try
            {
                _logger.LogInformation("Detecting bugs with Claude");
                
                var requestBody = new
                {
                    model = "claude-3-5-sonnet-20241022",
                    max_tokens = 2048,
                    messages = new[]
                    {
                        new
                        {
                            role = "user",
                            content = $"Analyze the following code for bugs, security vulnerabilities, and potential issues:\n\n{code}\n\nList each issue with severity and suggested fix."
                        }
                    }
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("messages", content);
                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync();
                using var document = JsonDocument.Parse(responseJson);
                
                var contentArray = document.RootElement.GetProperty("content");
                var firstContent = contentArray[0];
                var text = firstContent.GetProperty("text");
                
                var result = text.GetString() ?? "No response from AI";
                _logger.LogInformation("Claude bug detection completed successfully");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error detecting bugs with Claude");
                return $"Error: {ex.Message}";
            }
        }

        /// <summary>
        /// Generates DevOps suggestions using Claude API
        /// Returns best practices and implementation steps
        /// </summary>
        public async Task<string> GenerateDevOpsSuggestionAsync(string taskDescription, string taskType)
        {
            try
            {
                _logger.LogInformation("Generating DevOps suggestions with Claude for {TaskType}", taskType);
                
                var requestBody = new
                {
                    model = "claude-3-5-sonnet-20241022",
                    max_tokens = 2048,
                    messages = new[]
                    {
                        new
                        {
                            role = "user",
                            content = $"You are a DevOps expert. Provide AI-powered suggestions for the following task:\n\nTask Type: {taskType}\nDescription: {taskDescription}\n\nInclude best practices, tools to use, and implementation steps."
                        }
                    }
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("messages", content);
                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync();
                using var document = JsonDocument.Parse(responseJson);
                
                var contentArray = document.RootElement.GetProperty("content");
                var firstContent = contentArray[0];
                var text = firstContent.GetProperty("text");
                
                var result = text.GetString() ?? "No response from AI";
                _logger.LogInformation("Claude DevOps suggestions completed successfully");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating DevOps suggestions with Claude");
                return $"Error: {ex.Message}";
            }
        }
    }
}
