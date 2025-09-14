using CareerPathRecommender.Application.Constants;
using CareerPathRecommender.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;

namespace CareerPathRecommender.Infrastructure.Services
{
    public class ChatbotService : IChatbotService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string apiUrl;
        public ChatbotService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            apiUrl = _configuration["OpenAiUrl"] ?? throw new InvalidOperationException("OpenAI API key is not configured in appsettings.json");
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
    }
}
