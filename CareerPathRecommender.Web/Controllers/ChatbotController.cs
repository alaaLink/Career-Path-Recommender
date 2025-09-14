using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CareerPathRecommender.Application.Interfaces;

namespace CareerPathRecommender.Web.Controllers
{
    //[Authorize]
    public class ChatbotController : Controller
    {
        private readonly IChatbotService _chatbotService;

        public ChatbotController(IChatbotService chatbotService)
        {
            _chatbotService = chatbotService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] ChatMessageModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Message))
            {
                return BadRequest("Message cannot be empty");
            }

            try
            {
                var response = await _chatbotService.GetResponseAsync(model.Message);
                return Ok(new { response });
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
    }

    public class ChatMessageModel
    {
        public string Message { get; set; } = string.Empty;
    }
}
