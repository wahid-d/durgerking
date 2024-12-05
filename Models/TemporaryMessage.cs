namespace durgerking.Models;

public class TemporaryMessage
{
    public long ChatId { get; set; }
    public int MessageId { get; set; }
    public DateTime ExpirationTime { get; set; }
}