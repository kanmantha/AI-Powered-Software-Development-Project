using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AIPoweredSoftwareDevelopment.Services
{
    public class ClaudeCodeService : IACodeAnalysisService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public ClaudeCodeService(HttpClient httpClient, string apiKey)
        {
            _httpClient = httpClient;
            _apiKey = apiKey;
            _httpClient.BaseAddress = new Uri("https://api.anthropic.com/v1/");
            _httpClient.DefaultRequestHeaders.Add("x-api-key", _apiKey);
            _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
        }

        public async Task<string> AnalyzeCodeAsync(string code, string analysisType, string technology)
        {
            var requestBody = new
            {
                model = "claude-3-5-sonnet-20241022",
                max_tokens = 2048,
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = $@"You are an expert code reviewer using {technology}. 
Analyze the following code for {analysisType} issues:

```{code}
```

Provide specific findings with severity levels (Critical, High, Medium, Low)."
                    }
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("messages", content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(responseJson);
            
            return document.RootElement
                .GetProperty("content")[0]
                .GetProperty("text")
                .GetString();
        }

        public async Task<string> GenerateTestCasesAsync(string code, string testType)
        {
            var requestBody = new
            {
                model = "claude-3-5-sonnet-20241022",
                max_tokens = 2048,
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

            var response = await _httpClient.PostAsync("messages", content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(responseJson);
            
            return document.RootElement
                .GetProperty("content")[0]
                .GetProperty("text")
                .GetString();
        }

        public async Task<string> DetectBugsAsync(string code)
        {
            var requestBody = new
            {
                model = "claude-3-5-sonnet-20241022",
                max_tokens = 2048,
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

            var response = await _httpClient.PostAsync("messages", content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(responseJson);
            
            return document.RootElement
                .GetProperty("content")[0]
                .GetProperty("text")
                .GetString();
        }

        public async Task<string> GenerateDevOpsSuggestionAsync(string taskDescription, string taskType)
        {
            var requestBody = new
            {
                model = "claude-3-5-sonnet-20241022",
                max_tokens = 2048,
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

            var response = await _httpClient.PostAsync("messages", content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(responseJson);
            
            return document.RootElement
                .GetProperty("content")[0]
                .GetProperty("text")
                .GetString();
        }
    }
}
