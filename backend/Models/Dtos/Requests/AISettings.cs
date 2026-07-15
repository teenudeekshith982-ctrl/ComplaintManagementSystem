namespace ComplaintManagementSystem.Models.Dtos.Requests
{
    public class AISettings
    {
        public string Provider { get; set; } = "Groq";
        public string GroqApiKey { get; set; } = string.Empty;
        public string Model { get; set; } = "llama-3.3-70b-versatile";
    }
}
