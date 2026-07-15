using ComplaintManagementSystem.Models.Dtos;

namespace ComplaintManagementSystem.Interfaces;

public interface IEscalationService
{
    Task<CreateEscalationResponseDto> CreateEscalationAsync(CreateEscalationRequestDto request);

    Task AutoEscalateComplaintsAsync();

    Task<PagedResponseDto<EscalationResponseDto>> GetEscalationsAsync(EscalationFilterDto filter);

    Task<List<EscalationResponseDto>> GetComplaintEscalationsAsync(int complaintId);

    Task ResolveEscalationAsync(int escalatedId, EscalationActionRequestDto request);

    Task<int> GetPendingEscalationCountAsync();
}
