using CareerPathRecommender.Application.Constants;
using CareerPathRecommender.Application.Interfaces;
using CareerPathRecommender.Domain.Entities;
using CareerPathRecommender.Domain.Enums;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;

namespace CareerPathRecommender.Infrastructure.Services
{
    public class ChatbotService : IChatbotService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IRecommendationRepository _recommendationRepository;
        private readonly string apiUrl;
        public ChatbotService(HttpClient httpClient, IConfiguration configuration, IRecommendationRepository recommendationRepository)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            apiUrl = _configuration["OpenAiUrl"] ?? throw new InvalidOperationException("OpenAI API key is not configured in appsettings.json");
            _recommendationRepository = recommendationRepository;
        }

        public async Task<string> GetResponseAsync(string userMessage, string systemMessage = ChatbotConsts.DefaultSystemMessage)
        {
            try
            {
                var requestBody = new
                {
                    //model = "gpt-3.5-turbo",
                    model = "CairoICT-AI",
                    messages = new[]
                    {
                        new { role = "system", content = systemMessage },
                        new { role = "user", content = userMessage }
                    },
                    max_tokens = 500,
                    temperature = 1,
                    top_p = 1,
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(requestBody),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync(apiUrl, content);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                using var document = JsonDocument.Parse(responseContent);
                var choices = document.RootElement.GetProperty("choices");
                var firstChoice = choices[0];
                var messageContent = firstChoice.GetProperty("message");
                var responseText = messageContent.GetProperty("content").GetString() ?? "I'm sorry, I couldn't process that request.";

                return responseText;
            }
            catch (HttpRequestException ex)
            {
                // Log the error
                return await new MockChatbotService().GetResponseAsync(userMessage);
            }
            catch (Exception ex)
            {
                // Log the error
                return "I encountered an error while processing your request. Please try again.";
            }
        }

        public async Task<string> GetRecommentationResponseAsync(string userMessage)
        {
            RecommendationType  recommendationType = GetRecommendationTypeInUserMessage(userMessage);

            var recommendations = await _recommendationRepository.GetByTypeAsync(recommendationType);
            string systemMessage = ChatbotConsts.DefaultSystemMessage;

            if (recommendations.Any())
            {
                systemMessage = GenerateSystemMessage(recommendations, recommendationType);
            }

            return await GetResponseAsync(userMessage, systemMessage);
        }

        private RecommendationType GetRecommendationTypeInUserMessage(string userMessage)
        {
            var enummNames = Enum.GetNames<RecommendationType>();

            foreach (var item in enummNames)
            {
                if (userMessage.Contains(item, StringComparison.OrdinalIgnoreCase))
                {
                    return (RecommendationType)Enum.Parse(typeof(RecommendationType), item);
                }
                continue;
            }

            return RecommendationType.None;
        }

        private string GenerateSystemMessage(IEnumerable<Recommendation> recommendations, RecommendationType recommendationType)
        {
            // Generate system message includes every recommendation name and description
            var systemMessage = new StringBuilder();
            systemMessage.AppendLine($"You are a career path assistant in SoftWare Development. Here are some {recommendationType} recommendations:");
            foreach (var recommendation in recommendations)
            {
                systemMessage.AppendLine($"Title: {recommendation.Title}-Description: {recommendation.Description}");
            }
            systemMessage.AppendLine($"return a links from web (mostly from youtube) for every recommendation, then the usual response of you");
            return systemMessage.ToString();
        }
    }
}
