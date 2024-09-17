namespace DurgerKing.Services;

public interface IBotResponseService
{
    ValueTask<(long ChatId, long MessageId)> SendGreetingAsync(
        long chatId, 
        long userId, 
        CancellationToken cancellationToken = default);
    ValueTask<(long ChatId, long MessageId)> SendLanguageSettingsAsync(
        long chatId, 
        long userId, 
        CancellationToken cancellationToken = default);
    ValueTask<(long ChatId, long MessageId)> SendMainMenuAsync(
        long chatId,
        CancellationToken cancellationToken = default);
    ValueTask<(long ChatId, long MessageId)> SendSettingsAsync(
        long chatId,
        CancellationToken cancellationToken = default);
    ValueTask<(long ChatId, long MessageId)> SendLocationKeyboardAsync(
        long chatId,
        long userId,
        CancellationToken cancellationToken = default);
    ValueTask<(long ChatId, long MessageId)> SendLocationRequestAsync(
        long chatId,
        CancellationToken cancellationToken = default);
    ValueTask<(long ChatId, IEnumerable<long> MessageIds)> SendLocationsAsync(
        long chatId,
        long userId,
        CancellationToken cancellationToken = default);
    ValueTask<(long ChatId, long MessageId)> SendLocationExceededErrorAsync(
        long chatId, 
        CancellationToken cancellationToken = default);
    ValueTask<(long ChatId, long MessageId)> SendContactAsync(
        long chatId, 
        long userId, 
        CancellationToken cancellationToken = default);
    ValueTask<(long ChatId, long MessageId)> SendContactRequestAsync(
        long chatId, 
        CancellationToken cancellationToken = default);
    ValueTask<(long ChatId, long MessageId)> SendMenuAsync(
        long chatId,
        CancellationToken cancellationToken = default);

    ValueTask<(long ChatId, long MessageId)> SendCategoriesAsync(
        long chatId,
        CancellationToken cancellationToken = default);

    ValueTask<(long ChatId, long MessageId)> SendFoodAsync(
        long chatId,
        CancellationToken cancellationToken = default);

    ValueTask<(long ChatId, long MessageId)> SendSnackAsync(
        long chatId,
        int messageId,
        string clickedNavigation,
        CancellationToken cancellationToken = default);
}
