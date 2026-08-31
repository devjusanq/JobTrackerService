namespace JobTrackerService.Jobs.Domain;

public sealed class JobPhoto
{
    private JobPhoto() { }
    internal JobPhoto(Guid id, string url, DateTime capturedAt, string? caption)
    { Id = id; Url = url; CapturedAt = capturedAt; Caption = caption; }
    public Guid Id { get; private set; }
    public string Url { get; private set; } = string.Empty;
    public DateTime CapturedAt { get; private set; }
    public string? Caption { get; private set; }
}