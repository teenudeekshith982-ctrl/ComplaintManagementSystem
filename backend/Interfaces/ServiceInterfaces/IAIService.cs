using System.Collections.Generic;
using System.Threading.Tasks;
using ComplaintManagementSystem.Models;
using ComplaintManagementSystem.Models.Dtos.Responses;

namespace ComplaintManagementSystem.Interfaces
{
    public interface IAIService
    {
        Task<AICategoryValidationDto?> ValidateCategoryAsync(string title, string description, string selectedCategory);
        Task<AIPriorityRecommendationDto?> RecommendPriorityAsync(string title, string description);
        Task<string?> SummarizeComplaintAsync(string title, string description);
        Task<List<AISimilarComplaintDto>?> FindSimilarComplaintsAsync(int currentComplaintId, string currentTitle, string currentDescription, List<Complaint> otherComplaints);
    }
}
