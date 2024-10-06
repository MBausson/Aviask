using AviaskApi.Repositories;

namespace AviaskApi.Services.Attachment;

public class AttachmentService : IAttachmentService
{
    private readonly AttachmentRepository _repository;

    public AttachmentService(IAviaskRepository<Entities.Attachment, Guid> repository)
    {
        _repository = (AttachmentRepository)repository;
    }

    public async Task<Entities.Attachment?> GetAttachmentByIdAsync(Guid id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<Entities.Attachment> FileToAttachmentAsync(IFormFile file)
    {
        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);

        var data = stream.ToArray();

        return new Entities.Attachment
        {
            Id = new Guid(),
            Data = data,
            FileType = file.ContentType
        };
    }

    public async Task CreateOrUpdateAsync(Entities.Attachment attachment)
    {
        if (await _repository.ExistsByIdAsync(attachment.Id))
            await _repository.UpdateAsync(attachment);
        else
            await _repository.CreateAsync(attachment);
    }

    public async Task DeleteAttachmentAsync(Entities.Attachment attachment)
    {
        await _repository.DeleteAsync(attachment);
    }

    public bool ValidateFile(IFormFile file, long maxSizeInKb, params string[] allowedTypes)
    {
        if (allowedTypes.Length != 0 && !allowedTypes.Contains(file.ContentType)) return false;

        return (float)file.Length / 1024 <= maxSizeInKb;
    }
}