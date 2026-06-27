namespace ComplaintManagementSystem.Models.Dtos;

public class ComplaintListResponseDto
{
    public List<ComplaintListItemDto> Complaints { get; set; }

    public int TotalRecords { get; set; }

    public int PageNumber { get; set; }

    public int PageSize { get; set; }
}