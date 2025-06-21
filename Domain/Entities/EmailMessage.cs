namespace Domain.Entities;

public class EmailMessage
{
    public Guid Id { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string From { get; set; } = string.Empty;
    public byte[] EmlContent { get; set; } = [];
}
