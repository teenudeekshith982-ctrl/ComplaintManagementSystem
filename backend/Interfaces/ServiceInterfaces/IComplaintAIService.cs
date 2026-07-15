using System.Collections.Generic;
using System.Threading.Tasks;
using ComplaintManagementSystem.Models.Dtos.Responses;

namespace ComplaintManagementSystem.Interfaces
{
    public interface IComplaintAIService
    {
        Task<AICategoryValidationDto> GetCategoryValidationAsync(int complaintId);
        Task<AIPriorityRecommendationDto> GetPriorityRecommendationAsync(int complaintId);
        Task<string> GetComplaintSummaryAsync(int complaintId);
        Task<IEnumerable<AISimilarComplaintDto>> GetSimilarComplaintsAsync(int complaintId);
    }
}
