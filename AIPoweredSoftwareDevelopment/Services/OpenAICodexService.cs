using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AIPoweredSoftwareDevelopment.Services
{
    /// <summary>
    /// OpenAI Codex service implementation
    /// Provides AI-powered code analysis using OpenAI's API
    /// </summary>
    public class OpenAICodexService : IACodeAnalysisService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly ILogger<OpenAICodexService> _logger;

        /// <summary>
        /// Constructor - initializes HTTP client with OpenAI base address and API key
        /// </summary>
        public OpenAICodexService(HttpClient httpClient, string apiKey, ILogger<OpenAICodexService> logger)
        {
            _httpClient = httpClient;
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            _logger = logger;
            
            // OpenAI API base address
            _httpClient.BaseAddress = new Uri("https://api.openai.com/v1/");
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        }

        /// <summary>
        /// Analyzes code using OpenAI Codex model
        /// Returns analysis results with severity levels
        /// </summary>
        public async Task<string> AnalyzeCodeAsync(string code, string analysisType, string technology)
        {
            try
            {
                _logger.LogInformation("Starting OpenAI code analysis for {AnalysisType}", analysisType);
                
                var requestBody = new
                {
                    model = "gpt-4-turbo-preview", // Using GPT-4 for code analysis
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
                
                // Parse response to get AI-generated content
                var choices = document.RootElement.GetProperty("choices");
                var firstChoice = choices[0];
                var message = firstChoice.GetProperty("message");
                var contentStr = message.GetProperty("content");
                
                var result = contentStr.GetString() ?? "No response from AI";
                _logger.LogInformation("OpenAI analysis completed successfully");
                return result;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed while calling OpenAI API");
                return "Error: Unable to reach OpenAI API. Please check your internet connection and API key.";
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Request to OpenAI API timed out");
                return "Error: Request to OpenAI timed out. Please try again.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during OpenAI code analysis");
                return $"Error: {ex.Message}";
            }
        }

        /// <summary>
        /// Generates test cases using OpenAI Codex
        /// Returns generated test code
        /// </summary>
        public async Task<string> GenerateTestCasesAsync(string code, string testType)
        {
            try
            {
                _logger.LogInformation("Generating {TestType} test cases with OpenAI", testType);
                
                var requestBody = new
                {
                    model = "gpt-4-turbo-preview",
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
                _logger.LogInformation("OpenAI test generation completed successfully");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating test cases with OpenAI");
                return $"Error: {ex.Message}";
            }
        }

        /// <summary>
        /// Detects bugs using OpenAI Codex
        /// Returns list of bugs with severity and fixes
        /// </summary>
        public async Task<string> DetectBugsAsync(string code)
        {
            try
            {
                _logger.LogInformation("Detecting bugs with OpenAI");
                
                var requestBody = new
                {
                    model = "gpt-4-turbo-preview",
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
                _logger.LogInformation("OpenAI bug detection completed successfully");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error detecting bugs with OpenAI");
                return $"Error: {ex.Message}";
            }
        }

        /// <summary>
        /// Generates DevOps suggestions using OpenAI
        /// Returns best practices and implementation steps
        /// </summary>
        public async Task<string> GenerateDevOpsSuggestionAsync(string taskDescription, string taskType)
        {
            try
            {
                _logger.LogInformation("Generating DevOps suggestions with OpenAI for {TaskType}", taskType);
                
                var requestBody = new
                {
                    model = "gpt-4-turbo-preview",
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
                _logger.LogInformation("OpenAI DevOps suggestions completed successfully");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating DevOps suggestions with OpenAI");
                return $"Error: {ex.Message}";
            }
        }
    }
}
