namespace AccountDataAccess;

public class AccountSummaryProjection
{
    public required Guid Id { get; set; }
    public required string AccountHolder { get; set; }
    public required decimal Balance { get; set; }
    public required bool IsActive { get; set; }
    public required int Version { get; set; }
}