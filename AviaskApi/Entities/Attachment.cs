namespace AviaskApi.Entities;

public class Attachment
{
    public Guid Id { get; set; }
    public byte[] Data { get; set; }
    public string FileType { get; set; }
}