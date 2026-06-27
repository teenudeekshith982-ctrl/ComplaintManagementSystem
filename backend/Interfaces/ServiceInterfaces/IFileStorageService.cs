using ComplaintManagementSystem.Models.Dtos;

namespace ComplaintManagementSystem.Interfaces;

public interface IFileStorageService
{
    Task<List<FileUploadResult>> UploadFilesAsync(
        List<IFormFile> files,
        string folderName);
}