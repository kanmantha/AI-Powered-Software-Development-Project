using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace AIPoweredSoftwareDevelopment.Services
{
    /// <summary>
    /// GitHub Copilot service implementation
    /// Provides AI-powered code analysis using GitHub Copilot API
    /// </summary>
    public class GitHubCopilotService : IACodeAnalysisService
    {
        private readonly HttpClient _httpClient;
        private readonly string _token;
        private readonly ILogger<GitHubCopilotService> _logger;

        /// <summary>
        /// Constructor - initializes HTTP client with GitHub API configuration
        /// </summary>
        public GitHubCopilotService(HttpClient httpClient, string token, ILogger<GitHubCopilotService> logger)
        {
            _httpClient = httpClient;
            _token = token ?? throw new ArgumentNullException(nameof(token));
            _logger = logger;
            
            // GitHub Copilot API configuration
            _httpClient.BaseAddress = new Uri("https://api.github.com/");
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"token {_token}");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "AIPoweredSoftwareDevelopment");
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github.copilot-preview+json");
        }

        /// <summary>
        /// Analyzes code using GitHub Copilot Chat API
        /// Returns analysis results with severity levels
        /// </summary>
        public async Task<string> AnalyzeCodeAsync(string code, string analysisType, string technology)
        {
            try
            {
                _logger.LogInformation("Starting GitHub Copilot code analysis for {AnalysisType}", analysisType);
                
                var requestBody = new
                {
                    model = "copilot-chat",
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

                var response = await _httpClient.PostAsync("copilot/chat/completions", content);
                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync();
                using var document = JsonDocument.Parse(responseJson);
                
                var choices = document.RootElement.GetProperty("choices");
                var firstChoice = choices[0];
                var message = firstChoice.GetProperty("message");
                var contentStr = message.GetProperty("content");
                
                var result = contentStr.GetString() ?? "No response from AI";
                _logger.LogInformation("GitHub Copilot analysis completed successfully");
                return result;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed while calling GitHub Copilot API");
                return "Error: Unable to reach GitHub Copilot API. Please check your internet connection and token.";
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Request to GitHub Copilot API timed out");
                return "Error: Request to GitHub Copilot timed out. Please try again.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during GitHub Copilot code analysis");
                return $"Error: {ex.Message}";
            }
        }

        /// <summary>
        /// Generates test cases using GitHub Copilot
        /// Returns generated test code
        /// </summary>
        public async Task<string> GenerateTestCasesAsync(string code, string testType)
        {
            try
            {
                _logger.LogInformation("Generating {TestType} test cases with GitHub Copilot", testType);
                
                var requestBody = new
                {
                    model = "copilot-chat",
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

                var response = await _httpClient.PostAsync("copilot/chat/completions", content);
                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync();
                using var document = JsonDocument.Parse(responseJson);
                
                var choices = document.RootElement.GetProperty("choices");
                var firstChoice = choices[0];
                var message = firstChoice.GetProperty("message");
                var contentStr = message.GetProperty("content");
                
                var result = contentStr.GetString() ?? "No response from AI";
                _logger.LogInformation("GitHub Copilot test generation completed successfully");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating test cases with GitHub Copilot");
                return $"Error: {ex.Message}";
            }
        }

        /// <summary>
        /// Detects bugs using GitHub Copilot
        /// Returns list of bugs with severity and fixes
        /// </summary>
        public async Task<string> DetectBugsAsync(string code)
        {
            try
            {
                _logger.LogInformation("Detecting bugs with GitHub Copilot");
                
                var requestBody = new
                {
                    model = "copilot-chat",
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

                var response = await _httpClient.PostAsync("copilot/chat/completions", content);
                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync();
                using var document = JsonDocument.Parse(responseJson);
                
                var choices = document.RootElement.GetProperty("choices");
                var firstChoice = choices[0];
                var message = firstChoice.GetProperty("message");
                var contentStr = message.GetProperty("content");
                
                var result = contentStr.GetString() ?? "No response from AI";
                _logger.LogInformation("GitHub Copilot bug detection completed successfully");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error detecting bugs with GitHub Copilot");
                return $"Error: {ex.Message}";
            }
        }

        /// <summary>
        /// Generates DevOps suggestions using GitHub Copilot
        /// Returns best practices and implementation steps
        /// </summary>
        public async Task<string> GenerateDevOpsSuggestionAsync(string taskDescription, string taskType)
        {
            try
            {
                _logger.LogInformation("Generating DevOps suggestions with GitHub Copilot for {TaskType}", taskType);
                
                var requestBody = new
                {
                    model = "copilot-chat",
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

                var response = await _httpClient.PostAsync("copilot/chat/completions", content);
                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync();
                using var document = JsonDocument.Parse(responseJson);
                
                var choices = document.RootElement.GetProperty("choices");
                var firstChoice = choices[0];
                var message = firstChoice.GetProperty("message");
                var contentStr = message.GetProperty("content");
                
                var result = contentStr.GetString() ?? "No response from AI";
                _logger.LogInformation("GitHub Copilot DevOps suggestions completed successfully");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating DevOps suggestions with GitHub Copilot");
                return $"Error: {ex.Message}";
            }
        }
    }
}
