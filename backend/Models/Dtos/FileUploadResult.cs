namespace ComplaintManagementSystem.Models.Dtos;

public class FileUploadResult
{
    public string FileName { get; set; } = string.Empty;

    public string FilePath { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public string BlobName { get; set; } = string.Empty;
}