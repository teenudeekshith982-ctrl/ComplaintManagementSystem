using ComplaintManagementSystem.Models.Dtos;
using ComplaintManagementSystem.Exceptions;
using ComplaintManagementSystem.Interfaces;

namespace ComplaintManagementSystem.Services;

public class FileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _environment;

    private readonly ILogger<FileStorageService> _logger;

    private static readonly string[] AllowedExtensions =
    {
        ".pdf",
        ".jpg",
        ".jpeg",
        ".png",
        ".doc",
        ".docx"
    };

    public FileStorageService(
        IWebHostEnvironment environment,
        ILogger<FileStorageService> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    public async Task<List<FileUploadResult>>
        UploadFilesAsync(
            List<IFormFile> files,
            string folderName)
    {
        var uploadedFiles =
            new List<FileUploadResult>();

        var uploadFolder =
            Path.Combine(
                _environment.WebRootPath,
                "uploads",
                folderName);

        Directory.CreateDirectory(uploadFolder);

        foreach (var file in files)
        {
            ValidateFile(file);

            var extension =
                Path.GetExtension(file.FileName);

            var uniqueFileName =
                $"{Guid.NewGuid()}{extension}";

            var filePath =
                Path.Combine(
                    uploadFolder,
                    uniqueFileName);

            using var stream =
                new FileStream(
                    filePath,
                    FileMode.Create);

            await file.CopyToAsync(stream);

            uploadedFiles.Add(
                new FileUploadResult
                {
                    FileName = file.FileName,
                    FilePath = $"uploads/{folderName}/{uniqueFileName}",
                    ContentType = file.ContentType,
                    BlobName = uniqueFileName
                });

            _logger.LogInformation(
                "File {FileName} uploaded successfully",
                file.FileName);
        }

        return uploadedFiles;
    }

    public Task<Stream> DownloadFileAsync(string filePath)
    {
        var fullPath = Path.Combine(_environment.WebRootPath, filePath);
        if (!System.IO.File.Exists(fullPath))
        {
            throw new FileNotFoundException("Physical file not found on server.");
        }
        return Task.FromResult<Stream>(new FileStream(fullPath, FileMode.Open, FileAccess.Read));
    }

    public Task<bool> DeleteFileAsync(string filePath)
    {
        var fullPath = Path.Combine(_environment.WebRootPath, filePath);
        if (System.IO.File.Exists(fullPath))
        {
            System.IO.File.Delete(fullPath);
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    private static void ValidateFile(
        IFormFile file)
    {
        if (file.Length == 0)
        {
            throw new BusinessRuleException(
                "The uploaded file is empty. Please select a valid file with content.");
        }

        if (file.Length > 5 * 1024 * 1024)
        {
            throw new BusinessRuleException(
                "The selected file exceeds the maximum size limit of 5 MB. Please upload a smaller file.");
        }

        var extension =
            Path.GetExtension(file.FileName);

        if (!AllowedExtensions.Contains(
                extension.ToLower()))
        {
            throw new BusinessRuleException(
                $"The file type '{extension}' is not supported. Please upload a file with one of the allowed extensions: PDF, PNG, JPG, JPEG, DOC, or DOCX.");
        }
    }
}