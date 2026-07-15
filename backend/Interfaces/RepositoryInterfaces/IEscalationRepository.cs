using ComplaintManagementSystem.Models;
using ComplaintManagementSystem.Models.Dtos;

namespace ComplaintManagementSystem.Interfaces;

public interface IEscalationRepository
{
    Task<EscalatedComplaint?> GetLatestEscalationAsync(int complaintId);

    Task<bool> HasPendingEscalationAsync(int complaintId);

    Task<string> GetByIdAsync(int id);

    Task<EscalatedComplaint> AddAsync(EscalatedComplaint escalation);

    Task<(List<EscalatedComplaint>, int)> GetEscalationsAsync(EscalationFilterDto filter);

    Task<int> GetEscalationCountAsync();
}
