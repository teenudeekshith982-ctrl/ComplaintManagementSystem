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

                    FilePath =
                        $"uploads/{folderName}/{uniqueFileName}"
                });

            _logger.LogInformation(
                "File {FileName} uploaded successfully",
                file.FileName);
        }

        return uploadedFiles;
    }

    private static void ValidateFile(
        IFormFile file)
    {
        if (file.Length == 0)
        {
            throw new BusinessRuleException(
                "Empty file uploaded.");
        }

        if (file.Length > 5 * 1024 * 1024)
        {
            throw new BusinessRuleException(
                "File size cannot exceed 5 MB.");
        }

        var extension =
            Path.GetExtension(file.FileName);

        if (!AllowedExtensions.Contains(
                extension.ToLower()))
        {
            throw new BusinessRuleException(
                $"File type {extension} is not allowed.");
        }
    }
}