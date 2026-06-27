using AutoMapper;
using ComplaintManagementSystem.Models;
using ComplaintManagementSystem.Models.Dtos;

namespace ComplaintManagementSystem.Mappings;

public class ApplicationMapperProfile : Profile
{
    public ApplicationMapperProfile()
    {
        CreateMap<CreateEscalationRequestDto,
            EscalatedComplaint>();

        CreateMap<EscalatedComplaint,
                CreateEscalationResponseDto>()
            .ForMember(
                dest => dest.EscalationId,
                opt => opt.MapFrom(
                    src => src.EscalatedId));
    }
}