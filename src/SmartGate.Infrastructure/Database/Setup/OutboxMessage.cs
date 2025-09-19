namespace SmartGate.Infrastructure.Database.Setup;

public class OutboxMessage
{
    public Guid Id { get; set; }
    public DateTime OccurredAtUTC { get; set; }
    public string Type { get; set; } = default!;
    public string Content { get; set; } = default!;
    public DateTime? ProcessedAtUTC { get; set; }
}