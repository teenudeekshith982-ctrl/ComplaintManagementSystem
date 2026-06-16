namespace ComplaintManagementSystem.Models.Dtos;

public class PagedComplaintResponseDto
{
    public List<ComplaintItemDto> Items { get; set; } = [];
    public int PageNumber { get; set; } 
    public int PageSize { get; set; } 
    public int TotalRecords { get; set; }
}