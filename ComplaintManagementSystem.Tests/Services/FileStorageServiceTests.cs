using ComplaintManagementSystem.Exceptions;
using ComplaintManagementSystem.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace ComplaintManagementSystem.Tests.Services;

public class FileStorageServiceTests
{
    private readonly Mock<IWebHostEnvironment> _environmentMock;
    private readonly Mock<ILogger<FileStorageService>> _loggerMock;

    private readonly FileStorageService _service;

    public FileStorageServiceTests()
    {
        _environmentMock =
            new Mock<IWebHostEnvironment>();

        _loggerMock =
            new Mock<ILogger<FileStorageService>>();

        _environmentMock
            .Setup(x => x.WebRootPath)
            .Returns(Path.GetTempPath());

        _service =
            new FileStorageService(
                _environmentMock.Object,
                _loggerMock.Object);
    }

    [Fact]
    public async Task UploadFilesAsync_ShouldThrowBusinessRuleException_WhenFileIsEmpty()
    {
        // Arrange

        var file = new Mock<IFormFile>();

        file.Setup(f => f.Length)
            .Returns(0);

        file.Setup(f => f.FileName)
            .Returns("test.pdf");

        var files = new List<IFormFile>
        {
            file.Object
        };

        // Act

        Func<Task> act =
            async () => await _service.UploadFilesAsync(
                files,
                "complaints");

        // Assert

        await act.Should()
            .ThrowAsync<BusinessRuleException>()
            .WithMessage("The uploaded file is empty. Please select a valid file with content.");
    }

    [Fact]
    public async Task UploadFilesAsync_ShouldThrowBusinessRuleException_WhenFileSizeExceedsLimit()
    {
        // Arrange

        var file = new Mock<IFormFile>();

        file.Setup(f => f.Length)
            .Returns(6 * 1024 * 1024);

        file.Setup(f => f.FileName)
            .Returns("large.pdf");

        var files = new List<IFormFile>
        {
            file.Object
        };

        // Act

        Func<Task> act =
            async () => await _service.UploadFilesAsync(
                files,
                "complaints");

        // Assert

        await act.Should()
            .ThrowAsync<BusinessRuleException>()
            .WithMessage("The selected file exceeds the maximum size limit of 5 MB. Please upload a smaller file.");
    }

    [Fact]
    public async Task UploadFilesAsync_ShouldThrowBusinessRuleException_WhenExtensionIsInvalid()
    {
        // Arrange

        var file = new Mock<IFormFile>();

        file.Setup(f => f.Length)
            .Returns(100);

        file.Setup(f => f.FileName)
            .Returns("virus.exe");

        var files = new List<IFormFile>
        {
            file.Object
        };

        // Act

        Func<Task> act =
            async () => await _service.UploadFilesAsync(
                files,
                "complaints");

        // Assert

        await act.Should()
            .ThrowAsync<BusinessRuleException>()
            .WithMessage("The file type '.exe' is not supported. Please upload a file with one of the allowed extensions: PDF, PNG, JPG, JPEG, DOC, or DOCX.");
    }

    [Fact]
    public async Task UploadFilesAsync_ShouldUploadPdfSuccessfully()
    {
        // Arrange

        var content =
            new MemoryStream(new byte[] { 1, 2, 3 });

        var file = new FormFile(
            content,
            0,
            content.Length,
            "Data",
            "test.pdf")
        {
            Headers = new HeaderDictionary(),
            ContentType = "application/pdf"
        };

        var files = new List<IFormFile>
        {
            file
        };

        // Act

        var result =
            await _service.UploadFilesAsync(
                files,
                "complaints");

        // Assert

        result.Should().NotBeNull();

        result.Count.Should().Be(1);

        result[0].FileName.Should().Be("test.pdf");

        result[0].FilePath.Should()
            .Contain("uploads/complaints/");
    }

    [Fact]
    public async Task UploadFilesAsync_ShouldUploadMultipleFilesSuccessfully()
    {
        // Arrange

        var file1 =
            new FormFile(
                new MemoryStream(new byte[] { 1 }),
                0,
                1,
                "file1",
                "test1.pdf")
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/pdf"
            };

        var file2 =
            new FormFile(
                new MemoryStream(new byte[] { 1 }),
                0,
                1,
                "file2",
                "test2.jpg")
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg"
            };

        var files = new List<IFormFile>
        {
            file1,
            file2
        };

        // Act

        var result =
            await _service.UploadFilesAsync(
                files,
                "complaints");

        // Assert

        result.Count.Should().Be(2);
    }

    [Fact]
    public async Task UploadFilesAsync_ShouldGenerateUniqueFileNames()
    {
        // Arrange

        var file =
            new FormFile(
                new MemoryStream(new byte[] { 1 }),
                0,
                1,
                "file",
                "sample.pdf")
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/pdf"
            };

        var files = new List<IFormFile>
        {
            file
        };

        // Act

        var result =
            await _service.UploadFilesAsync(
                files,
                "complaints");

        // Assert

        result[0].FilePath.Should()
            .NotContain("sample.pdf");
    }

    [Fact]
    public async Task UploadFilesAsync_ShouldCreateUploadFolder()
    {
        // Arrange

        var file =
            new FormFile(
                new MemoryStream(new byte[] { 1 }),
                0,
                1,
                "file",
                "sample.pdf")
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/pdf"
            };

        var files = new List<IFormFile>
        {
            file
        };

        // Act

        await _service.UploadFilesAsync(
            files,
            "complaints");

        // Assert

        var expectedFolder =
            Path.Combine(
                Path.GetTempPath(),
                "uploads",
                "complaints");

        Directory.Exists(expectedFolder)
            .Should()
            .BeTrue();
    }
}