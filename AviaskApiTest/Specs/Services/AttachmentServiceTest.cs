using AviaskApi.Services.Attachment;
using Microsoft.AspNetCore.Http;

namespace AviaskApiTest.Specs.Services;

public class AttachmentServiceTest
{
    private readonly Mock<AttachmentRepository> _mockRepository;
    private readonly IAttachmentService _service;

    public AttachmentServiceTest()
    {
        _mockRepository = new Mock<AttachmentRepository>(null!);

        _service = new AttachmentService(_mockRepository.Object);
    }

    [Fact]
    public async Task GetAttachmentByIdAsync_NotFound()
    {
        var result = await _service.GetAttachmentByIdAsync(new Guid());

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAttachmentByIdAsync_ReturnsAttachment()
    {
        var attachment = new AttachmentFactory().GetOne();

        _mockRepository.Setup(m => m.GetByIdAsync(attachment.Id)).ReturnsAsync(attachment);

        var result = await _service.GetAttachmentByIdAsync(attachment.Id);

        Assert.Equal(attachment, result);
    }

    [Fact]
    public async Task FileToAttachmentAsync_CreatesAttachment()
    {
        var byteArray = new byte[] { 0xF, 0xF };
        using var memoryStream = new MemoryStream(byteArray);

        var formFile = new FormFile(memoryStream, 0, byteArray.Length, "name", "image/jpg")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/jpg"
        };

        var result = await _service.FileToAttachmentAsync(formFile);

        Assert.NotNull(result);

        Assert.Equal(byteArray, result.Data);
        Assert.Equal("image/jpg", result.FileType);
    }

    [Fact]
    public async Task CreateOrUpdateAsync_CreatesAttachment()
    {
        var attachment = new AttachmentFactory().GetOne();

        _mockRepository.Setup(m => m.ExistsByIdAsync(attachment.Id)).ReturnsAsync(false);
        _mockRepository.Setup(m => m.CreateAsync(attachment)).Verifiable();
        _mockRepository.Setup(m => m.UpdateAsync(attachment)).Verifiable();

        await _service.CreateOrUpdateAsync(attachment);

        _mockRepository.Verify(m => m.CreateAsync(attachment), Times.Once);
        _mockRepository.Verify(m => m.UpdateAsync(attachment), Times.Never);
    }

    [Fact]
    public async Task CreateOrUpdateAsync_UpdatesAttachment()
    {
        var attachment = new AttachmentFactory().GetOne();

        _mockRepository.Setup(m => m.ExistsByIdAsync(attachment.Id)).ReturnsAsync(true);
        _mockRepository.Setup(m => m.CreateAsync(attachment)).Verifiable();
        _mockRepository.Setup(m => m.UpdateAsync(attachment)).Verifiable();

        await _service.CreateOrUpdateAsync(attachment);

        _mockRepository.Verify(m => m.CreateAsync(attachment), Times.Never);
        _mockRepository.Verify(m => m.UpdateAsync(attachment), Times.Once);
    }

    [Fact]
    public async Task DeleteAttachmentAsync_DeletesAttachment()
    {
        var attachment = new AttachmentFactory().GetOne();

        _mockRepository.Setup(m => m.DeleteAsync(attachment)).Verifiable();

        await _service.DeleteAttachmentAsync(attachment);

        _mockRepository.Verify(m => m.DeleteAsync(attachment), Times.Once);
    }

    [Fact]
    public async Task ValidateFile_BadFileType()
    {
        var byteArray = new byte[] { 0xF, 0xF };
        using var memoryStream = new MemoryStream(byteArray);

        var formFile = new FormFile(memoryStream, 0, byteArray.Length, "name", "image/jpg")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/jpg"
        };

        var result = _service.ValidateFile(formFile, 3, "image/png");

        Assert.False(result);
    }

    [Fact]
    public async Task ValidateFile_TooLarge()
    {
        //  1 KB of data + 1 B to make the validation fail
        var kiloByteArray = Enumerable.Range(0, 1025).Select(_ => (byte)0xF).ToArray();
        using var memoryStream = new MemoryStream(kiloByteArray);

        var formFile = new FormFile(memoryStream, 0, kiloByteArray.Length, "name", "image/jpg")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/jpg"
        };

        var result = _service.ValidateFile(formFile, 1, "image/jpg");

        Assert.False(result);
    }

    [Fact]
    public async Task ValidateFile_Valid()
    {
        var kiloByteArray = Enumerable.Range(0, 1023).Select(_ => (byte)0xF).ToArray();
        using var memoryStream = new MemoryStream(kiloByteArray);

        var formFile = new FormFile(memoryStream, 0, kiloByteArray.Length, "name", "image/jpg")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/jpg"
        };

        var result = _service.ValidateFile(formFile, 1, "image/jpg");

        Assert.True(result);
    }
}