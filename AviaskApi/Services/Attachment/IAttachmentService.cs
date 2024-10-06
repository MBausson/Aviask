namespace AviaskApi.Services.Attachment;

public interface IAttachmentService
{
    public Task<Entities.Attachment?> GetAttachmentByIdAsync(Guid id);
    public Task<Entities.Attachment> FileToAttachmentAsync(IFormFile file);
    public Task CreateOrUpdateAsync(Entities.Attachment attachment);
    public Task DeleteAttachmentAsync(Entities.Attachment attachment);
    public bool ValidateFile(IFormFile file, long maxSizeInKb, params string[] allowedTypes);
}