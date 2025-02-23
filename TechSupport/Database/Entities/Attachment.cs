namespace TechSupport.Database.Entities;

public class Attachment
{
    public Guid Id { get; set; }
    public byte[] Bytes { get; set; }
    public string FileName { get; set; }
    public string FileExtension { get; set; }
    public string ContentType { get; set; }
    public int BytesLength => Bytes.Length;
}