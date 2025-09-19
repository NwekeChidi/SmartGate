namespace SmartGate.Infrastructure.Database.Setup;

public class IdempotencyKey
{
    public string Key { get; set; } = default!;
    public Guid VisitId { get; set; }
    public DateTime CreatedAtUTC { get; set; }
}
