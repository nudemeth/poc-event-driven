namespace AccountEventListener;

public class InboxContext
{
    public string MessageId { get; set; } = string.Empty;
    public int ReceiveCount { get; set; }
    public string Body { get; set; } = string.Empty;
}
