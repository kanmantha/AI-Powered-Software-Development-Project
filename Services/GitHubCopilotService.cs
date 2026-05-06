using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AIPoweredSoftwareDevelopment.Services
{
    public class GitHubCopilotService : IACodeAnalysisService
    {
        private readonly HttpClient _httpClient;
        private readonly string _token;

        public GitHubCopilotService(HttpClient httpClient, string token)
        {
            _httpClient = httpClient;
            _token = token;
            _httpClient.BaseAddress = new Uri("https://api.github.com/");
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"token {_token}");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "AIPoweredSoftwareDevelopment");
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github.copilot-preview+json");
        }

        public async Task<string> AnalyzeCodeAsync(string code, string analysisType, string technology)
        {
            // GitHub Copilot Chat API integration
            var requestBody = new
            {
                model = "copilot-chat",
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = $@"Analyze the following code for {analysisType} issues:

```{code}
```

Provide specific findings with severity levels (Critical, High, Medium, Low)."
                    }
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://api.github.com/copilot/chat/completions", content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(responseJson);
            
            return document.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();
        }

        public async Task<string> GenerateTestCasesAsync(string code, string testType)
        {
            var requestBody = new
            {
                model = "copilot-chat",
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = $@"Generate {testType} test cases for the following code:

```{code}
```

Provide complete test code with assertions."
                    }
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://api.github.com/copilot/chat/completions", content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(responseJson);
            
            return document.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();
        }

        public async Task<string> DetectBugsAsync(string code)
        {
            var requestBody = new
            {
                model = "copilot-chat",
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = $@"Analyze the following code for bugs, security vulnerabilities, and potential issues:

```{code}
```

List each issue with severity and suggested fix."
                    }
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://api.github.com/copilot/chat/completions", content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(responseJson);
            
            return document.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();
        }

        public async Task<string> GenerateDevOpsSuggestionAsync(string taskDescription, string taskType)
        {
            var requestBody = new
            {
                model = "copilot-chat",
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = $@"You are a DevOps expert. Provide AI-powered suggestions for the following task:

Task Type: {taskType}
Description: {taskDescription}

Include best practices, tools to use, and implementation steps."
                    }
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://api.github.com/copilot/chat/completions", content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(responseJson);
            
            return document.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();
        }
    }
}
