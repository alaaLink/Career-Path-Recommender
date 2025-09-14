namespace CareerPathRecommender.Application.Interfaces
{
    public interface IChatbotService
    {
        Task<string> GetResponseAsync(string message);
    }
}
