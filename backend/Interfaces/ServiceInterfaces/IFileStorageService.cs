using ComplaintManagementSystem.Models.Dtos;

namespace ComplaintManagementSystem.Interfaces;

public interface IFileStorageService
{
    Task<List<FileUploadResult>> UploadFilesAsync(
        List<IFormFile> files,
        string folderName);

    Task<Stream> DownloadFileAsync(string filePath);

    Task<bool> DeleteFileAsync(string filePath);
}