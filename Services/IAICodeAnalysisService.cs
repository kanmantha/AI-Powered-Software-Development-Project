using System.Threading.Tasks;

namespace AIPoweredSoftwareDevelopment.Services
{
    public interface IACodeAnalysisService
    {
        Task<string> AnalyzeCodeAsync(string code, string analysisType, string technology);
        Task<string> GenerateTestCasesAsync(string code, string testType);
        Task<string> DetectBugsAsync(string code);
        Task<string> GenerateDevOpsSuggestionAsync(string taskDescription, string taskType);
    }
}
