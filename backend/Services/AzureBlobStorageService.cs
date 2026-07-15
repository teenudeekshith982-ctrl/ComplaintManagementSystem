using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using ComplaintManagementSystem.Interfaces;
using ComplaintManagementSystem.Models.Dtos;
using ComplaintManagementSystem.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ComplaintManagementSystem.Services
{
    public class AzureBlobStorageService : IFileStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName;
        private readonly ILogger<AzureBlobStorageService> _logger;

        private static readonly string[] AllowedExtensions =
        {
            ".pdf",
            ".jpg",
            ".jpeg",
            ".png",
            ".doc",
            ".docx"
        };

        public AzureBlobStorageService(
            BlobServiceClient blobServiceClient,
            IConfiguration configuration,
            ILogger<AzureBlobStorageService> logger)
        {
            _blobServiceClient = blobServiceClient ?? throw new ArgumentNullException(nameof(blobServiceClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _containerName = configuration["AzureBlobStorage:ContainerName"] 
                ?? throw new InvalidOperationException("Azure Blob Storage ContainerName is not configured.");
        }

        private BlobContainerClient GetContainerClient()
        {
            return _blobServiceClient.GetBlobContainerClient(_containerName);
        }

        public async Task<List<FileUploadResult>> UploadFilesAsync(List<IFormFile> files, string folderName)
        {
            var uploadedFiles = new List<FileUploadResult>();
            var containerClient = GetContainerClient();

            // Create container if it doesn't exist
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);

            foreach (var file in files)
            {
                ValidateFile(file);

                var extension = Path.GetExtension(file.FileName);
                var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                
                // folderName is usually "complaints/{complaintId}"
                // The combined path inside the container will be: complaints/{complaintId}/{GUID}.ext
                var blobName = $"{folderName}/{uniqueFileName}";

                var blobClient = containerClient.GetBlobClient(blobName);
                
                _logger.LogInformation("Uploading file {FileName} to Azure Blob {BlobName}", file.FileName, blobName);

                using (var stream = file.OpenReadStream())
                {
                    await blobClient.UploadAsync(stream, new BlobUploadOptions
                    {
                        HttpHeaders = new BlobHttpHeaders { ContentType = file.ContentType }
                    });
                }

                // For Azure, FilePath will store the absolute URL, and BlobName will store the unique blob path key
                uploadedFiles.Add(new FileUploadResult
                {
                    FileName = file.FileName,
                    FilePath = blobClient.Uri.AbsoluteUri,
                    ContentType = file.ContentType,
                    BlobName = blobName
                });
            }

            return uploadedFiles;
        }

        public async Task<Stream> DownloadFileAsync(string filePath)
        {
            try
            {
                var containerClient = GetContainerClient();
                var blobName = GetBlobNameFromPath(filePath);
                var blobClient = containerClient.GetBlobClient(blobName);

                if (!await blobClient.ExistsAsync())
                {
                    throw new FileNotFoundException($"The file '{blobName}' was not found in Azure Blob Storage.");
                }

                return await blobClient.OpenReadAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to download blob {BlobName}", filePath);
                throw;
            }
        }

        public async Task<bool> DeleteFileAsync(string filePath)
        {
            try
            {
                var containerClient = GetContainerClient();
                var blobName = GetBlobNameFromPath(filePath);
                var blobClient = containerClient.GetBlobClient(blobName);

                return await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete blob {BlobName}", filePath);
                return false;
            }
        }

        private string GetBlobNameFromPath(string path)
        {
            if (path.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || 
                path.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                var uri = new Uri(path);
                var containerSegment = $"/{_containerName}/";
                int index = uri.AbsolutePath.IndexOf(containerSegment, StringComparison.OrdinalIgnoreCase);
                if (index >= 0)
                {
                    return Uri.UnescapeDataString(uri.AbsolutePath.Substring(index + containerSegment.Length));
                }
            }
            return path;
        }

        private static void ValidateFile(IFormFile file)
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

            var extension = Path.GetExtension(file.FileName);

            if (!AllowedExtensions.Contains(extension.ToLower()))
            {
                throw new BusinessRuleException(
                    $"The file type '{extension}' is not supported. Please upload a file with one of the allowed extensions: PDF, PNG, JPG, JPEG, DOC, or DOCX.");
            }
        }
    }
}
