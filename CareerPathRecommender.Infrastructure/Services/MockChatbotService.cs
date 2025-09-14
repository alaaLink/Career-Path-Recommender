using CareerPathRecommender.Application.Interfaces;

namespace CareerPathRecommender.Infrastructure.Services
{
    public class MockChatbotService : IChatbotService
    {
        private readonly Dictionary<string, string> _responses = new(StringComparer.OrdinalIgnoreCase)
        {
            ["hello"] = "Hello! I'm your Career Path Assistant. How can I help you today?",
            ["hi"] = "Hi there! I'm here to help with your career development. What would you like to know?",
            ["skills"] = "Based on your profile, you might want to develop skills in: C#, .NET Core, and cloud technologies. Would you like specific learning resources?",
            ["career path"] = "Based on your current skills and experience, you might want to consider these career paths: Senior Software Engineer, Technical Lead, or Solutions Architect. Would you like more details about any of these?",
            ["salary"] = "Salaries can vary based on location, experience, and company. For example, in the US, the average salary for a Senior Software Engineer is around $120,000 per year. Would you like more specific information?",
            ["thank"] = "You're welcome! Is there anything else I can help you with?",
            ["bye"] = "Goodbye! Feel free to come back if you have more questions about your career development.",
        };

        public Task<string> GetResponseAsync(string message, string systemMessag = "")
        {
            var response = _responses
                .FirstOrDefault(kvp => message.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase))
                .Value ?? "I'm here to help with your career development. Could you please rephrase your question or ask about career paths, skills, or job opportunities?";

            return Task.FromResult(response);
        }
    }
}
