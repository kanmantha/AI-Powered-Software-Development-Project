using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;

namespace AIPoweredSoftwareDevelopment.Services
{
    public class AzureDevOpsService : IACodeAnalysisService
    {
        private readonly HttpClient _httpClient;
        private readonly string _organization;
        private readonly string _patToken; // Personal Access Token

        public AzureDevOpsService(HttpClient httpClient, string organization, string patToken)
        {
            _httpClient = httpClient;
            _organization = organization;
            _patToken = patToken;
            _httpClient.BaseAddress = new Uri($"https://dev.azure.com/{_organization}/");
            var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{_patToken}"));
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {authToken}");
        }

        public async Task<string> AnalyzeCodeAsync(string code, string analysisType, string technology)
        {
            // Azure DevOps uses AI through Azure OpenAI Service
            // This is a simplified example - real implementation would use Azure AI services
            var requestBody = new
            {
                prompt = $@"You are an expert code reviewer using {technology}. 
Analyze the following code for {analysisType} issues:

```{code}
```

Provide specific findings with severity levels (Critical, High, Medium, Low).",
                max_tokens = 2048
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Call Azure AI service (this would be your Azure OpenAI endpoint)
            var response = await _httpClient.PostAsync("_apis/ai/code-review", content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(responseJson);
            
            return document.RootElement
                .GetProperty("analysis")
                .GetString();
        }

        public async Task<string> GenerateTestCasesAsync(string code, string testType)
        {
            var requestBody = new
            {
                prompt = $@"Generate {testType} test cases for the following code:

```{code}
```

Provide complete test code with assertions.",
                max_tokens = 2048
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("_apis/ai/test-generation", content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(responseJson);
            
            return document.RootElement
                .GetProperty("testCode")
                .GetString();
        }

        public async Task<string> DetectBugsAsync(string code)
        {
            var requestBody = new
            {
                prompt = $@"Analyze the following code for bugs, security vulnerabilities, and potential issues:

```{code}
```

List each issue with severity and suggested fix.",
                max_tokens = 2048
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("_apis/ai/bug-detection", content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(responseJson);
            
            return document.RootElement
                .GetProperty("bugs")
                .GetString();
        }

        public async Task<string> GenerateDevOpsSuggestionAsync(string taskDescription, string taskType)
        {
            var requestBody = new
            {
                prompt = $@"You are a DevOps expert. Provide AI-powered suggestions for the following task:

Task Type: {taskType}
Description: {taskDescription}

Include best practices, tools to use, and implementation steps.",
                max_tokens = 2048
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("_apis/ai/devops-suggestions", content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(responseJson);
            
            return document.RootElement
                .GetProperty("suggestions")
                .GetString();
        }
    }
}
