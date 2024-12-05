namespace Durgerking.Interfaces;

public interface IDeleteMessageListService
{
    bool AddMessageToDelete (long chatId, int messageId, TimeSpan? expiration = default);
    
    IEnumerable<(int MessageId,  long ChatId)> GetExpiredMessagesToDelete();
}