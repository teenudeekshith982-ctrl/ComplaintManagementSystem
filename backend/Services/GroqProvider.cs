using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ComplaintManagementSystem.Interfaces;
using ComplaintManagementSystem.Models;
using ComplaintManagementSystem.Models.Dtos.Requests;
using ComplaintManagementSystem.Models.Dtos.Responses;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ComplaintManagementSystem.Services
{
    public class GroqProvider : IAIService
    {
        private readonly HttpClient _httpClient;
        private readonly AISettings _settings;
        private readonly ILogger<GroqProvider> _logger;

        public GroqProvider(
            HttpClient httpClient,
            IOptions<AISettings> settings,
            ILogger<GroqProvider> logger)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _logger = logger;
        }

        private async Task<string?> CallGroqAsync(string prompt)
        {
            if (string.IsNullOrEmpty(_settings.GroqApiKey))
            {
                _logger.LogWarning("Groq API key is not configured.");
                return null;
            }

            var url = "https://api.groq.com/openai/v1/chat/completions";

            var requestBody = new
            {
                model = _settings.Model,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                },
                response_format = new { type = "json_object" }
            };

            var jsonPayload = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            int retries = 2;
            int delayMs = 1000;

            for (int i = 0; i <= retries; i++)
            {
                try
                {
                    using var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(30));
                    var request = new HttpRequestMessage(HttpMethod.Post, url)
                    {
                        Content = content
                    };

                    request.Headers.Add("Authorization", $"Bearer {_settings.GroqApiKey}");

                    var response = await _httpClient.SendAsync(request, cts.Token);

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonResponse = await response.Content.ReadAsStringAsync();
                        using var doc = JsonDocument.Parse(jsonResponse);
                        var root = doc.RootElement;

                        if (root.TryGetProperty("choices", out var choices) &&
                            choices.GetArrayLength() > 0 &&
                            choices[0].TryGetProperty("message", out var messageProp) &&
                            messageProp.TryGetProperty("content", out var contentProp))
                        {
                            return contentProp.GetString();
                        }
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        _logger.LogError("Groq API returned error status code: {StatusCode}. Details: {Details}", response.StatusCode, errorContent);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error calling Groq API. Attempt {Attempt} of {MaxAttempts}", i + 1, retries + 1);
                }

                if (i < retries)
                {
                    await Task.Delay(delayMs);
                    delayMs *= 2; // exponential backoff
                }
            }

            return null;
        }

        public async Task<AICategoryValidationDto?> ValidateCategoryAsync(string title, string description, string selectedCategory)
        {
            var prompt = $@"
You are an AI assistant validating complaint categories for a customer complaint ticketing system.
The user selected Category: ""{selectedCategory}"" for the following complaint:
Title: ""{title}""
Description: ""{description}""

Validate if the selected category is correct. The standard categories are: ""Technical"", ""Finance"", ""HR"".
If the selected category does not match the content, suggest the correct category from the list.
Respond strictly with a JSON object matching this structure:
{{
  ""SuggestedCategory"": ""Technical"" or ""Finance"" or ""HR"",
  ""Confidence"": ""XX%"",
  ""Reason"": ""A brief explanation of why this category is suggested""
}}
";

            var responseText = await CallGroqAsync(prompt);
            if (string.IsNullOrEmpty(responseText)) return null;

            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<AICategoryValidationDto>(responseText, options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deserialize AICategoryValidationDto. Raw response: {Raw}", responseText);
                return null;
            }
        }

        public async Task<AIPriorityRecommendationDto?> RecommendPriorityAsync(string title, string description)
        {
            var prompt = $@"
You are an AI assistant recommending priority levels for customer complaints.
Complaint Details:
Title: ""{title}""
Description: ""{description}""

Recommend a priority from: ""Critical"", ""High"", ""Medium"", ""Low"".
Also recommend an SLA duration (e.g. ""6 Hours"", ""12 Hours"", ""24 Hours"", ""48 Hours"").
Respond strictly with a JSON object matching this structure:
{{
  ""Priority"": ""Critical"" or ""High"" or ""Medium"" or ""Low"",
  ""Confidence"": ""XX%"",
  ""SuggestedSla"": ""X Hours"",
  ""Reason"": ""A brief explanation of why this priority is recommended""
}}
";

            var responseText = await CallGroqAsync(prompt);
            if (string.IsNullOrEmpty(responseText)) return null;

            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<AIPriorityRecommendationDto>(responseText, options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deserialize AIPriorityRecommendationDto. Raw response: {Raw}", responseText);
                return null;
            }
        }

        public async Task<string?> SummarizeComplaintAsync(string title, string description)
        {
            var prompt = $@"
You are an AI assistant generating concise summaries for customer complaints.
Complaint Details:
Title: ""{title}""
Description: ""{description}""

Generate a short bullet-point summary (maximum 3-4 bullet points, under 100 words) summarizing the main issues.
Respond strictly with a JSON object matching this structure:
{{
  ""Summary"": ""bullet 1\nbullet 2\nbullet 3""
}}
";

            var responseText = await CallGroqAsync(prompt);
            if (string.IsNullOrEmpty(responseText)) return null;

            try
            {
                using var doc = JsonDocument.Parse(responseText);
                if (doc.RootElement.TryGetProperty("Summary", out var summaryProp))
                {
                    return summaryProp.GetString();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to extract summary from JSON response. Raw response: {Raw}", responseText);
            }

            return null;
        }

        public async Task<List<AISimilarComplaintDto>?> FindSimilarComplaintsAsync(
            int currentComplaintId,
            string currentTitle,
            string currentDescription,
            List<Complaint> otherComplaints)
        {
            if (otherComplaints == null || otherComplaints.Count == 0)
            {
                return new List<AISimilarComplaintDto>();
            }

            var historicList = new List<object>();
            foreach (var c in otherComplaints)
            {
                var resolutionStr = "";
                if (c.Comments != null && c.Comments.Count > 0)
                {
                    var commentsList = new List<string>();
                    foreach (var comm in c.Comments)
                    {
                        commentsList.Add(comm.Message);
                    }
                    resolutionStr = string.Join(" | ", commentsList);
                }

                historicList.Add(new
                {
                    Id = c.ComplaintId,
                    Title = c.Title,
                    Desc = c.Description,
                    Status = c.ComplaintStatus != null ? c.ComplaintStatus.StatusName : "Closed",
                    Resolution = resolutionStr
                });
            }

            var historicJson = JsonSerializer.Serialize(historicList);

            var prompt = $@"
You are an AI assistant identifying similar historic complaints from a database.
Current Complaint:
Title: ""{currentTitle}""
Description: ""{currentDescription}""

Historic Complaints database logs:
{historicJson}

Compare the current complaint against the historic logs.
Find the top most similar complaints (up to 3). For each match, assign a similarity percentage (e.g. ""85%"") and summarize how it was resolved (ResolutionSummary).
Respond strictly with a JSON object matching this structure:
{{
  ""SimilarComplaints"": [
    {{
      ""ComplaintId"": 123,
      ""Similarity"": ""85%"",
      ""ComplaintTitle"": ""The title of the match"",
      ""Status"": ""Resolved"",
      ""ResolutionSummary"": ""A summary of comments, fixes, or actions taken to resolve it""
    }}
  ]
}}
If no similar complaints exist, return an empty array.
";

            var responseText = await CallGroqAsync(prompt);
            if (string.IsNullOrEmpty(responseText)) return null;

            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var responseObj = JsonSerializer.Deserialize<SimilarComplaintsWrapper>(responseText, options);
                return responseObj?.SimilarComplaints;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deserialize SimilarComplaintsWrapper. Raw response: {Raw}", responseText);
                return null;
            }
        }

        private class SimilarComplaintsWrapper
        {
            [JsonPropertyName("similarComplaints")]
            public List<AISimilarComplaintDto> SimilarComplaints { get; set; } = new();
        }
    }
}
