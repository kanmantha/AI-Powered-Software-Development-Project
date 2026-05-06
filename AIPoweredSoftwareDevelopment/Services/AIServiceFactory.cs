using Microsoft.Extensions.Configuration;
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AIPoweredSoftwareDevelopment.Services
{
    /// <summary>
    /// Factory class for creating AI service instances
    /// Implements factory pattern to instantiate appropriate AI service based on technology
    /// Falls back to CustomAIService if API keys are not configured
    /// </summary>
    public class AIServiceFactory
    {
        private readonly ILogger<AIServiceFactory> _logger;

        /// <summary>
        /// Constructor - initializes logger for tracking service creation
        /// </summary>
        public AIServiceFactory(ILogger<AIServiceFactory> logger = null!)
        {
            _logger = logger;
        }

        /// <summary>
        /// Creates an AI service instance based on the specified technology
        /// Returns appropriate service or falls back to Custom AI if keys are missing
        /// </summary>
        public IACodeAnalysisService? CreateService(string technology, IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            try
            {
                _logger?.LogInformation("Creating AI service for technology: {Technology}", technology);

                return technology?.ToLower() switch
                {
                    "openai codex" => !string.IsNullOrEmpty(configuration["OpenAI:ApiKey"])
                        ? new OpenAICodexService(httpClientFactory.CreateClient("OpenAI"), configuration["OpenAI:ApiKey"] ?? "", 
                            httpClientFactory.CreateClient("OpenAI").GetLogger<OpenAICodexService>())
                        : FallbackToCustomAI("OpenAI Codex"),
                    
                    "claude code" => !string.IsNullOrEmpty(configuration["Claude:ApiKey"])
                        ? new ClaudeCodeService(httpClientFactory.CreateClient("Claude"), configuration["Claude:ApiKey"] ?? "",
                            httpClientFactory.CreateClient("Claude").GetLogger<ClaudeCodeService>())
                        : FallbackToCustomAI("Claude Code"),
                    
                    "github copilot" => !string.IsNullOrEmpty(configuration["GitHub:CopilotToken"])
                        ? new GitHubCopilotService(httpClientFactory.CreateClient("GitHub"), configuration["GitHub:CopilotToken"] ?? "",
                            httpClientFactory.CreateClient("GitHub").GetLogger<GitHubCopilotService>())
                        : FallbackToCustomAI("GitHub Copilot"),
                    
                    "cursor ai" => !string.IsNullOrEmpty(configuration["Cursor:ApiKey"])
                        ? new CursorAIService(httpClientFactory.CreateClient("Cursor"), configuration["Cursor:ApiKey"] ?? "",
                            httpClientFactory.CreateClient("Cursor").GetLogger<CursorAIService>())
                        : FallbackToCustomAI("Cursor AI"),
                    
                    "azure devops" => !string.IsNullOrEmpty(configuration["AzureDevOps:PatToken"])
                        ? new AzureDevOpsService(
                            httpClientFactory.CreateClient("Azure"), 
                            configuration["AzureDevOps:Organization"] ?? "", 
                            configuration["AzureDevOps:PatToken"] ?? "",
                            httpClientFactory.CreateClient("Azure").GetLogger<AzureDevOpsService>())
                        : FallbackToCustomAI("Azure DevOps"),
                    
                    "custom ai" => new CustomAIService(httpClientFactory.CreateClient("Custom").GetLogger<CustomAIService>()),
                    
                    _ => FallbackToCustomAI(technology ?? "Unknown")
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating AI service for technology: {Technology}", technology);
                // Return Custom AI as fallback on error
                return new CustomAIService();
            }
        }

        /// <summary>
        /// Falls back to Custom AI service when API keys are not configured
        /// Logs warning about missing configuration
        /// </summary>
        private IACodeAnalysisService FallbackToCustomAI(string technology)
        {
            _logger?.LogWarning("API key not configured for {Technology}. Falling back to Custom AI.", technology);
            return new CustomAIService();
        }
    }

    /// <summary>
    /// Extension method to get logger from HttpClient (helper for creating loggers)
    /// </summary>
    public static class HttpClientExtensions
    {
        public static ILogger<T> GetLogger<T>(this HttpClient client) where T : class
        {
            // This is a simplified approach - in real implementation, use ILoggerFactory
            return null; // Placeholder - actual implementation would use dependency injection
        }
    }
}
