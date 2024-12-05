using Durgerking.Interfaces;
using durgerking.Models;

namespace DurgerKing.Services;

public class DeleteMessageListService : IDeleteMessageListService
{
    private readonly List<TemporaryMessage> messageList = new ();
    private readonly object lockObject = new object();
    
    public bool AddMessageToDelete(long chatId, int messageId, TimeSpan? expiration = null)
    {
        var expirationTime = DateTime.UtcNow.Add(expiration ?? TimeSpan.FromMinutes(10));

        lock (lockObject)
        {
            messageList.Add(new TemporaryMessage
            {
                ChatId = chatId,
                MessageId = messageId,
                ExpirationTime = expirationTime
            });
        }

        return true;
    }

    public IEnumerable<(int MessageId, long ChatId)> GetExpiredMessagesToDelete()
    {
        var now = DateTime.UtcNow;
        var expiredMessages = new List<(int MessageId, long ChatId)>();

        lock (lockObject)
        {
            foreach (var message in messageList)
            {
                if (message.ExpirationTime <= now)
                {
                    expiredMessages.Add((message.MessageId, message.ChatId));
                }
            }

            // Remove the expired messages from the list
            messageList.RemoveAll(message => message.ExpirationTime <= now);
        }

        return expiredMessages;
    }
}