using CareerPathRecommender.Application.Constants;

namespace CareerPathRecommender.Application.Interfaces
{
    public interface IChatbotService
    {
        Task<string> GetRecommentationResponseAsync(string userMessage);
        Task<string> GetResponseAsync(string userMessage, string systemMessage = ChatbotConsts.DefaultSystemMessage);
    }
}
