using durgerking.Data.Migrations;
using DurgerKing.Resources;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DurgerKing.Services;

public partial class UpdateHandler
{
    private async Task HandleCallBackQueryAsync(ITelegramBotClient client, CallbackQuery query, CancellationToken cancellationToken = default)
    {
        var task = query.Data switch
        {
            _ when query.Data == Button.Menu || query.Data == Button.Category
                => responseService.SendMenuAsync(query.Message.Chat.Id, cancellationToken).AsTask(),
            _ when query.Data == Button.Settings
                => responseService.SendSettingsAsync(query.Message.Chat.Id, cancellationToken).AsTask(),
            _ when query.Data == Button.LanguageSettings
                => responseService.SendLanguageSettingsAsync(query.Message.Chat.Id, query.From.Id, cancellationToken).AsTask(),
            _ when query.Data == Button.LocationSettings
                => responseService.SendLocationKeyboardAsync(query.Message.Chat.Id, query.From.Id, cancellationToken).AsTask(),
            _ when query.Data.Contains("languages")
                => HandleLanguageCallbackAsync(query, cancellationToken),
            _ when query.Data == Button.AddLocation
                => responseService.SendLocationRequestAsync(query.Message.Chat.Id, cancellationToken).AsTask(),
            _ when query.Data == Button.ShowLocations
                => responseService.SendLocationsAsync(query.Message.Chat.Id, query.From.Id, cancellationToken).AsTask(),
            _ when query.Data.Contains(Button.DeleteAddress)
                => HandleDeleteAddressAsync(query.From.Id, query.Data, cancellationToken),
            _ when query.Data == Button.ContactSettings
                => responseService.SendContactAsync(query.Message.Chat.Id, query.From.Id, cancellationToken).AsTask(),
            _ when query.Data == Button.ContactUpdate
                => responseService.SendContactRequestAsync(query.Message.Chat.Id, cancellationToken).AsTask(),
            _ when query.Data == Category.Food || query.Data.Contains("pagination.foods")
                => responseService.SendFoodsAsync(query.Message.Chat.Id, query.Data, cancellationToken).AsTask(),
            _ when query.Data == Category.Snack || query.Data.Contains("pagination.snacks")
                => responseService.SendSnacksAsync(query.Message.Chat.Id, query.Data, cancellationToken).AsTask(),
            _ when query.Data == Category.Drink || query.Data.Contains("pagination.drinks")
                => responseService.SendDrinksAsync(query.Message.Chat.Id, query.Data, cancellationToken).AsTask(),
            _ when query.Data == Category.Salad || query.Data.Contains("pagination.salads")
                => responseService.SendSaladsAsync(query.Message.Chat.Id, query.Data, cancellationToken).AsTask(),
            _ when query.Data == Category.Set || query.Data.Contains("pagination.sets")
                => responseService.SendSetsAsync(query.Message.Chat.Id, query.Data, cancellationToken).AsTask(),
            _ => throw new NotImplementedException($"Call back query {query.Data} not supported!")
        };

        await client.DeleteMessageAsync(query.Message.Chat.Id, query.Message.MessageId, cancellationToken);
        await task;
    }

    private async Task HandleDeleteAddressAsync(long userId, string data, CancellationToken cancellationToken = default)
    {
        var locationIdString = data.Split(".", StringSplitOptions.RemoveEmptyEntries).Last();
        var locationId = Guid.Parse(locationIdString);
        await userService.RemoveLocationAsync(userId, locationId, cancellationToken);
    }

    private async Task HandleLanguageCallbackAsync(CallbackQuery query, CancellationToken cancellationToken)
    {
        var user = await userService.UpdateLanguageAsync(
            userId: query.From.Id, 
            language: query.Data[(query.Data.IndexOf(".") + 1)..], 
            cancellationToken: cancellationToken);

        SetRequestCulture(user.Language);

        await responseService.SendLanguageSettingsAsync(query.Message.Chat.Id, query.From.Id, cancellationToken);
    }
} 