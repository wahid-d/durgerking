using Durgerking.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Polling;

namespace DurgerKing.Services;

public class BotStartingBackgroundService : BackgroundService
{
    private readonly ILogger<BotStartingBackgroundService> logger;
    private readonly IDeleteMessageListService deleteMessageListService;
    private readonly ITelegramBotClient botClient;
    private readonly IUpdateHandler updateHandler;

    public BotStartingBackgroundService(
        ILogger<BotStartingBackgroundService> logger,
        ITelegramBotClient botClient,
        IUpdateHandler updateHandler,
        IDeleteMessageListService deleteMessageListService)
    {
        this.logger = logger;
        this.botClient = botClient;
        this.updateHandler = updateHandler;
        this.deleteMessageListService = deleteMessageListService;
    }
    
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        try
        {
            var bot = await botClient.GetMeAsync(cancellationToken);
            logger.LogInformation("Bot {username} started polling updates.", bot.Username);

            botClient.StartReceiving(
                updateHandler : updateHandler,
                receiverOptions : default,
                cancellationToken : cancellationToken
            );
            
            var expiredMessages = deleteMessageListService.GetExpiredMessagesToDelete();

            foreach (var (messageId, chatId) in expiredMessages)
            {
                try
                {
                    await botClient.DeleteMessageAsync(chatId, messageId, cancellationToken: cancellationToken);
                    logger.LogInformation($"Deleted expired message with ID {messageId} in chat {chatId}.");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Error deleting message with ID {messageId} in chat {chatId}.");
                }
            }
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Failed to connect to bot server.");
        }

        await Task.Delay(TimeSpan.FromMinutes(5), cancellationToken);
    }
}