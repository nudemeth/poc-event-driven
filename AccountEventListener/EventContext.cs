using AccountProjection;

namespace AccountEventListener;

public class EventContext
{
    public AccountSummaryProjection Account { get; set; } = null!;
}
