namespace ComplaintManagementSystem.Models.Dtos;

public class AttachmentDto
{
    public int AttachmentId { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string FilePath { get; set; } = string.Empty;

    public DateTime UploadedAt { get; set; }
}