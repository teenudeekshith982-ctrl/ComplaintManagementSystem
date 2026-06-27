using System.ComponentModel.DataAnnotations;

namespace ComplaintManagementSystem.Models;

public class ComplaintAttachment
{
    [Key]
    public int AttachmentId { get; set; }

    public int ComplaintId { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string FilePath { get; set; } = string.Empty;

    public DateTime UploadedAt { get; set; }

    public Complaint? Complaint { get; set; }
}