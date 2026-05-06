using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace AIPoweredSoftwareDevelopment.Services
{
    /// <summary>
    /// Cursor AI service implementation
    /// Provides AI-powered code analysis using Cursor AI API (OpenAI-compatible)
    /// </summary>
    public class CursorAIService : IACodeAnalysisService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly ILogger<CursorAIService> _logger;

        /// <summary>
        /// Constructor - initializes HTTP client with Cursor AI configuration
        /// Uses OpenAI-compatible API format
        /// </summary>
        public CursorAIService(HttpClient httpClient, string apiKey, ILogger<CursorAIService> logger)
        {
            _httpClient = httpClient;
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            _logger = logger;
            
            // Cursor AI uses OpenAI-compatible API
            _httpClient.BaseAddress = new Uri("https://api.cursor.sh/v1/");
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        }

        /// <summary>
        /// Analyzes code using Cursor AI
        /// Returns analysis results with severity levels
        /// </summary>
        public async Task<string> AnalyzeCodeAsync(string code, string analysisType, string technology)
        {
            try
            {
                _logger.LogInformation("Starting Cursor AI code analysis for {AnalysisType}", analysisType);
                
                var requestBody = new
                {
                    model = "cursor-fast",
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

                var response = await _httpClient.PostAsync("chat/completions", content);
                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync();
                using var document = JsonDocument.Parse(responseJson);
                
                var choices = document.RootElement.GetProperty("choices");
                var firstChoice = choices[0];
                var message = firstChoice.GetProperty("message");
                var contentStr = message.GetProperty("content");
                
                var result = contentStr.GetString() ?? "No response from AI";
                _logger.LogInformation("Cursor AI analysis completed successfully");
                return result;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed while calling Cursor AI API");
                return "Error: Unable to reach Cursor AI API. Please check your internet connection and API key.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during Cursor AI code analysis");
                return $"Error: {ex.Message}";
            }
        }

        /// <summary>
        /// Generates test cases using Cursor AI
        /// Returns generated test code
        /// </summary>
        public async Task<string> GenerateTestCasesAsync(string code, string testType)
        {
            try
            {
                _logger.LogInformation("Generating {TestType} test cases with Cursor AI", testType);
                
                var requestBody = new
                {
                    model = "cursor-fast",
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

                var response = await _httpClient.PostAsync("chat/completions", content);
                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync();
                using var document = JsonDocument.Parse(responseJson);
                
                var choices = document.RootElement.GetProperty("choices");
                var firstChoice = choices[0];
                var message = firstChoice.GetProperty("message");
                var contentStr = message.GetProperty("content");
                
                var result = contentStr.GetString() ?? "No response from AI";
                _logger.LogInformation("Cursor AI test generation completed successfully");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating test cases with Cursor AI");
                return $"Error: {ex.Message}";
            }
        }

        /// <summary>
        /// Detects bugs using Cursor AI
        /// Returns list of bugs with severity and fixes
        /// </summary>
        public async Task<string> DetectBugsAsync(string code)
        {
            try
            {
                _logger.LogInformation("Detecting bugs with Cursor AI");
                
                var requestBody = new
                {
                    model = "cursor-fast",
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

                var response = await _httpClient.PostAsync("chat/completions", content);
                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync();
                using var document = JsonDocument.Parse(responseJson);
                
                var choices = document.RootElement.GetProperty("choices");
                var firstChoice = choices[0];
                var message = firstChoice.GetProperty("message");
                var contentStr = message.GetProperty("content");
                
                var result = contentStr.GetString() ?? "No response from AI";
                _logger.LogInformation("Cursor AI bug detection completed successfully");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error detecting bugs with Cursor AI");
                return $"Error: {ex.Message}";
            }
        }

        /// <summary>
        /// Generates DevOps suggestions using Cursor AI
        /// Returns best practices and implementation steps
        /// </summary>
        public async Task<string> GenerateDevOpsSuggestionAsync(string taskDescription, string taskType)
        {
            try
            {
                _logger.LogInformation("Generating DevOps suggestions with Cursor AI for {TaskType}", taskType);
                
                var requestBody = new
                {
                    model = "cursor-fast",
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

                var response = await _httpClient.PostAsync("chat/completions", content);
                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync();
                using var document = JsonDocument.Parse(responseJson);
                
                var choices = document.RootElement.GetProperty("choices");
                var firstChoice = choices[0];
                var message = firstChoice.GetProperty("message");
                var contentStr = message.GetProperty("content");
                
                var result = contentStr.GetString() ?? "No response from AI";
                _logger.LogInformation("Cursor AI DevOps suggestions completed successfully");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating DevOps suggestions with Cursor AI");
                return $"Error: {ex.Message}";
            }
        }
    }
}
