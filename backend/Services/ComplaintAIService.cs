using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ComplaintManagementSystem.Interfaces;
using ComplaintManagementSystem.Models.Dtos.Responses;
using ComplaintManagementSystem.Exceptions;

namespace ComplaintManagementSystem.Services
{
    public class ComplaintAIService : IComplaintAIService
    {
        private readonly IAIService _aiService;
        private readonly IComplaintRepository _complaintRepository;

        public ComplaintAIService(
            IAIService aiService,
            IComplaintRepository complaintRepository)
        {
            _aiService = aiService ?? throw new ArgumentNullException(nameof(aiService));
            _complaintRepository = complaintRepository ?? throw new ArgumentNullException(nameof(complaintRepository));
        }

        public async Task<AICategoryValidationDto> GetCategoryValidationAsync(int complaintId)
        {
            var complaint = await _complaintRepository.GetByIdAsync(complaintId);
            if (complaint == null)
            {
                throw new NotFoundException("Complaint not found.");
            }

            var categoryName = complaint.ComplaintCategory?.Categoryname ?? "General";
            var result = await _aiService.ValidateCategoryAsync(complaint.Title, complaint.Description, categoryName);
            if (result == null)
            {
                throw new ServiceUnavailableException("AI validation service is currently unavailable");
            }

            return result;
        }

        public async Task<AIPriorityRecommendationDto> GetPriorityRecommendationAsync(int complaintId)
        {
            var complaint = await _complaintRepository.GetByIdAsync(complaintId);
            if (complaint == null)
            {
                throw new NotFoundException("Complaint not found.");
            }

            var result = await _aiService.RecommendPriorityAsync(complaint.Title, complaint.Description);
            if (result == null)
            {
                throw new ServiceUnavailableException("AI recommendation service is currently unavailable");
            }

            return result;
        }

        public async Task<string> GetComplaintSummaryAsync(int complaintId)
        {
            var complaint = await _complaintRepository.GetByIdAsync(complaintId);
            if (complaint == null)
            {
                throw new NotFoundException("Complaint not found.");
            }

            var summary = await _aiService.SummarizeComplaintAsync(complaint.Title, complaint.Description);
            if (summary == null)
            {
                throw new ServiceUnavailableException("AI summary service is currently unavailable");
            }

            return summary;
        }

        public async Task<IEnumerable<AISimilarComplaintDto>> GetSimilarComplaintsAsync(int complaintId)
        {
            var complaint = await _complaintRepository.GetByIdAsync(complaintId);
            if (complaint == null)
            {
                throw new NotFoundException("Complaint not found.");
            }

            var otherComplaints = await _complaintRepository.GetResolvedOrClosedComplaintsAsync(complaintId);
            var result = await _aiService.FindSimilarComplaintsAsync(complaintId, complaint.Title, complaint.Description, otherComplaints);
            if (result == null)
            {
                throw new ServiceUnavailableException("AI similarity service is currently unavailable");
            }

            return result;
        }
    }
}
