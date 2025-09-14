namespace CareerPathRecommender.Application.DTOs
{
    public class ChatMessageModel
    {
        public string Message { get; set; } = string.Empty;
        public bool RecommendedFromDatabase { get; set; } = false;
    }
}
