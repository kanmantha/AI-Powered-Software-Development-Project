using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;

namespace AIPoweredSoftwareDevelopment.Services
{
    public class AIServiceFactory
    {
        public IACodeAnalysisService CreateService(string technology, IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            return technology.ToLower() switch
            {
                "openai codex" => !string.IsNullOrEmpty(configuration["OpenAI:ApiKey"]) 
                    ? new OpenAICodexService(httpClientFactory.CreateClient("OpenAI"), configuration["OpenAI:ApiKey"]) 
                    : new CustomAIService(),
                "claude code" => !string.IsNullOrEmpty(configuration["Claude:ApiKey"]) 
                    ? new ClaudeCodeService(httpClientFactory.CreateClient("Claude"), configuration["Claude:ApiKey"]) 
                    : new CustomAIService(),
                "github copilot" => !string.IsNullOrEmpty(configuration["GitHub:CopilotToken"]) 
                    ? new GitHubCopilotService(httpClientFactory.CreateClient("GitHub"), configuration["GitHub:CopilotToken"]) 
                    : new CustomAIService(),
                "cursor ai" => !string.IsNullOrEmpty(configuration["Cursor:ApiKey"]) 
                    ? new CursorAIService(httpClientFactory.CreateClient("Cursor"), configuration["Cursor:ApiKey"]) 
                    : new CustomAIService(),
                "azure devops" => !string.IsNullOrEmpty(configuration["AzureDevOps:PatToken"]) 
                    ? new AzureDevOpsService(httpClientFactory.CreateClient("Azure"), configuration["AzureDevOps:Organization"], configuration["AzureDevOps:PatToken"]) 
                    : new CustomAIService(),
                "custom ai" => new CustomAIService(),
                _ => new CustomAIService() // Fallback to custom AI instead of throwing error
            };
        }
    }
}
