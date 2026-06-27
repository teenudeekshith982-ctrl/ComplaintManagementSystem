using ComplaintManagementSystem.Models.Dtos;

namespace ComplaintManagementSystem.Interfaces;

public interface IEscalationService
{
    public Task<CreateEscalationResponseDto>
        CreateEscalationAsync(
            CreateEscalationRequestDto request);
    
    Task AutoEscalateComplaintsAsync();
    
    Task<PagedResponseDto<EscalationResponseDto>> GetEscalationsAsync(EscalationFilterDto filter);
    Task<List<EscalationResponseDto>> GetComplaintEscalationsAsync(int complaintId);
    Task ResolveEscalationAsync(int escalatedId, string action, string comments);
}