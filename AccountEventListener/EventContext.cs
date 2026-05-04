using AccountProjection;

namespace AccountEventListener;

public class EventContext
{
    public AccountSummaryProjection Account { get; set; } = null!;
    public string MessageId { get; set; } = string.Empty;
    public int ReceiveCount { get; set; }
    public string Body { get; set; } = string.Empty;
}
