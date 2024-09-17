using Durgerking.Services;
using DurgerKing.Resources;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using DurgerKing.Extensions;
using DurgerKing.Models;

namespace DurgerKing.Services;

public class BotResponseService : IBotResponseService
{
    private readonly ILogger<BotResponseService> logger;
    private readonly IUserService userService;
    private readonly ILocalizationHandler localization;
    private readonly ITelegramBotClient botClient;
    private readonly IProductService productService ;

    public BotResponseService(
        ILogger<BotResponseService> logger,
        IUserService userService,
        ILocalizationHandler localization,
        ITelegramBotClient botClient,
        IProductService productService)
    {
        this.logger = logger;
        this.userService = userService;
        this.localization = localization;
        this.botClient = botClient;
        this.productService = productService;
    }

    public async ValueTask<(long ChatId, long MessageId)> SendGreetingAsync(
        long chatId, 
        long userId, 
        CancellationToken cancellationToken = default)
    {
        var user = await userService.GetUserOrDefaultAsync(userId, cancellationToken);
        var name = user.Username ?? user.Fullname;

        logger.LogTrace("Sending a greeting to {name}", name);

        var message = await botClient.SendTextMessageAsync(
            text: localization.GetValue(Message.Greeting, name),
            chatId: chatId,
            cancellationToken: cancellationToken);

        return (chatId, message.MessageId);
    }

    public async ValueTask<(long ChatId, long MessageId)> SendLanguageSettingsAsync(
        long chatId, 
        long userId, 
        CancellationToken cancellationToken = default)
    {
        var user = await userService.GetUserOrDefaultAsync(userId, cancellationToken);
        var languagesKeyboard = new Dictionary<string, string>
        {
            { Button.LanguagesUz, $"{GetCheckmarkOrEmpty(user.Language, "uz")}O'zbekcha🇺🇿" },
            { Button.LanguagesEn, $"{GetCheckmarkOrEmpty(user.Language, "en")}English🇬🇧" },
            { Button.LanguagesRu, $"{GetCheckmarkOrEmpty(user.Language, "ru")}Русский🇷🇺" }
        }.Select(k => InlineKeyboardButton.WithCallbackData(k.Value, k.Key));

        var settingsButton = InlineKeyboardButton.WithCallbackData(
            text: $"🔙 {localization.GetValue(Button.Settings)}",
            callbackData: Button.Settings);

        var keyboardMatrix = new[]
        {
            languagesKeyboard,
            new[] { settingsButton },
        };
        
        var message = await botClient.SendTextMessageAsync(
            text: $"_{localization.GetValue(Button.LanguageSettings)}_",
            chatId: chatId,
            replyMarkup: new InlineKeyboardMarkup(keyboardMatrix),
            parseMode: ParseMode.Markdown,
            cancellationToken: cancellationToken);
        
        return (chatId, message.MessageId);
    }

    public async ValueTask<(long ChatId, long MessageId)> SendMainMenuAsync(
        long chatId,
        CancellationToken cancellationToken = default)
    {
        var keyboardMatrix = new[]
        {
            new[] { Button.Settings, Button.Menu },
            new[] { Button.Orders },
        };

        var message = await botClient.SendTextMessageAsync(
            text: $"_{localization.GetValue(Message.MainMenu)}_",
            chatId: chatId,
            replyMarkup: GetInlineKeyboard(keyboardMatrix),
            parseMode: ParseMode.Markdown,
            cancellationToken: cancellationToken);
        
        return (chatId, message.MessageId);
    }

    public async ValueTask<(long ChatId, long MessageId)> SendSettingsAsync(
        long chatId,
        CancellationToken cancellationToken = default)
    {
        var keyboardMatrix = new[]
        {
            new[] { Button.LanguageSettings, Button.LocationSettings },
            new[] { Button.ContactSettings },
        };

        var message = await botClient.SendTextMessageAsync(
            text: $"_{localization.GetValue(Button.Settings)}_",
            chatId: chatId,
            replyMarkup: GetInlineKeyboard(keyboardMatrix),
            parseMode: ParseMode.Markdown,
            cancellationToken: cancellationToken);
        
        return (chatId, message.MessageId);
    }

    public async ValueTask<(long ChatId, long MessageId)> SendLocationKeyboardAsync(
        long chatId,
        long userId,
        CancellationToken cancellationToken = default)
    {
        var user = await userService.GetUserWithLocationsOrDefaultAsync(userId, cancellationToken);

        var buttons = new List<string>();
        if(user.Locations.Any())
            buttons.Add(Button.ShowLocations);
        if(user.Locations.Count < 3)
            buttons.Add(Button.AddLocation);

        var keyboardMatrix = new[]
        { 
            buttons.ToArray(),
            new[] { Button.Settings }
        };

        var message = await botClient.SendTextMessageAsync(
            text: $"_{localization.GetValue(Button.LocationSettings)}_",
            chatId: chatId,
            replyMarkup: GetInlineKeyboard(keyboardMatrix),
            parseMode: ParseMode.Markdown,
            cancellationToken: cancellationToken);
        
        return (chatId, message.MessageId);
    }

    public async ValueTask<(long ChatId, long MessageId)> SendLocationRequestAsync(
        long chatId,
        CancellationToken cancellationToken = default)
    {
        var button =  KeyboardButton.WithRequestLocation(localization.GetValue(Button.LocationRequest));
        var keyboardLayout = new[] { new[] { button } };

        var message = await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: $"*{localization.GetValue(Message.LocationRequest)}*",
            replyMarkup: new ReplyKeyboardMarkup(keyboardLayout) { ResizeKeyboard = true },
            parseMode: ParseMode.Markdown,
            cancellationToken: cancellationToken);

        return (chatId, message.MessageId);
    }

    public async ValueTask<(long ChatId, IEnumerable<long> MessageIds)> SendLocationsAsync(long chatId, long userId, CancellationToken cancellationToken = default)
    {
        var messageIds = new List<long>();
        var user = await userService.GetUserWithLocationsOrDefaultAsync(userId, cancellationToken);
        foreach(var location in user.Locations)
        {
            var deleteButton  = new[] { new[] 
            { 
                InlineKeyboardButton.WithCallbackData(
                    text: localization.GetValue(Button.DeleteAddress), 
                    callbackData: Button.DeleteAddress + $".{location.Id}") 
            } };

            var message = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: location.Address,
                replyMarkup: new InlineKeyboardMarkup(deleteButton),
                parseMode: ParseMode.Markdown,
                cancellationToken: cancellationToken);
            messageIds.Add(message.MessageId); 
        }

        return (chatId, messageIds);
    }

    public async ValueTask<(long ChatId, long MessageId)> SendLocationExceededErrorAsync(long chatId, CancellationToken cancellationToken = default)
    {
        var message = await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: localization.GetValue(Message.LocationMaxExceeded),
            parseMode: ParseMode.Html,
            replyMarkup: new ReplyKeyboardRemove(),
            cancellationToken: cancellationToken);

        return (chatId, message.MessageId);
    }

    public async ValueTask<(long ChatId, long MessageId)> SendContactAsync(
        long chatId, 
        long userId, 
        CancellationToken cancellationToken = default)
    {
        var user = await userService.GetUserWithContactOrDefaultAsync(userId, cancellationToken);
        if(user.Contact is null)
            return await SendContactRequestAsync(chatId, cancellationToken);
        
        var message = await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: localization.GetValue(
                key: Message.ContactDisplay, 
                arguments: new [] { user.Contact.FirstName, user.Contact.LastName, user.Contact.PhoneNumber }),
            replyMarkup: GetInlineKeyboard(new[] { new[] { Button.ContactUpdate }}),
            parseMode: ParseMode.Html,
            cancellationToken: cancellationToken);
        
        var _ = RemoveKeyboardAsync(chatId, cancellationToken);
        
        return (chatId, message.MessageId);
    }

    public async ValueTask<(long ChatId, long MessageId)> SendContactRequestAsync(
        long chatId, 
        CancellationToken cancellationToken = default)
    {
        var button =  KeyboardButton.WithRequestContact(localization.GetValue(Button.ContactRequest));
        var keyboardLayout = new[] { new[] { button } };

        var message = await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: localization.GetValue(Message.ContactRequest),
            replyMarkup: new ReplyKeyboardMarkup(keyboardLayout) { ResizeKeyboard = true },
            parseMode: ParseMode.Html,
            cancellationToken: cancellationToken);

        return (chatId, message.MessageId);
    }

    public async ValueTask<(long ChatId, long MessageId)> SendMenuAsync(
    long chatId,
    CancellationToken cancellationToken = default)
    {
        var keyboardMatrix = new[]
        {
            new[] { Button.Food, Button.Salad },
            new[] { Button.Snack, Button.Drink },
            new[] { Button.Set },
        };

        var message = await botClient.SendTextMessageAsync(
            text: $"_{localization.GetValue(Button.Settings)}_",
            chatId: chatId,
            replyMarkup: GetInlineKeyboard(keyboardMatrix),
            parseMode: ParseMode.Markdown,
            cancellationToken: cancellationToken);

        return (chatId, message.MessageId);
    }

    public async ValueTask <(long ChatId, long MessageId)> SendCategoriesAsync(
        long chatId,
        CancellationToken cancellationToken = default)
    {
        var categoryDb = await productService.GetCategoriesAsync(cancellationToken);

        var categories = categoryDb.Select(p => $"category.{p.Id}");

        var row = (int)Math.Ceiling((double)categories.Count() / 2);
        var categoryMatrix = Enumerable.Range(0, row)
            .Select(a => categories.Skip(a * 2).Take(2).ToArray()).ToArray();

        var message = await botClient.SendTextMessageAsync(
            text: $"_{"Categories"}_",
            chatId: chatId,
            replyMarkup: GetInlineKeyboard(categoryMatrix),
            parseMode: ParseMode.Markdown,
            cancellationToken: cancellationToken);

        return (chatId, message.MessageId);
    }

    public async ValueTask<(long ChatId, long MessageId)> SendFoodAsync(
        long chatId, 
        CancellationToken cancellationToken = default)
    {
        var products = await productService.GetProductsAsync(1, cancellationToken);

        var keyboardMatrix = new[]
        {
            new[] {"1", "2", "3", "4", "5", "6", "7", "8", "9"}
        };

        var product = products.FirstOrDefault();

        var message = await botClient.SendTextMessageAsync(
            text: $"name: {product.Name}\nprice: {product.Price}",
            chatId: chatId,
            replyMarkup: GetInlineKeyboardPagination(keyboardMatrix),
            parseMode: ParseMode.Markdown,
            cancellationToken: cancellationToken);

        return (chatId, message.MessageId);
    }

    private InlineKeyboardMarkup GetInlineKeyboardPagination(string[][] matrix)
    {
        var buttonLength = matrix[0].Length;
        string[][] newMatrix = new string[1][];

        if(buttonLength > 4)
        {
            newMatrix[0] = new string[5];
            matrix[0][buttonLength - (buttonLength - 4)] = ">>";
            for(int i = 0; i < buttonLength; i++)
            {
                newMatrix[0][i] = matrix[0][i];
                if(matrix[0][i].Equals(">>"))
                    break;
            }
        }
        else if(buttonLength <= 4)
        {
            newMatrix[0] = new string[buttonLength];
            for(int i = 0; i < buttonLength; i++)
            {
                newMatrix[0][i] = matrix[0][i];
            }
        }
        
        var buttonMatrix = new InlineKeyboardButton[newMatrix.GetLength(0)][];
        for(int i = 0; i < newMatrix.GetLength(0); i++)
            buttonMatrix[i] = newMatrix[i]
                .Select(x => InlineKeyboardButton.WithCallbackData(localization.GetValue(x), x)).ToArray();
        
        return new InlineKeyboardMarkup(buttonMatrix);
    }

    public async ValueTask<(long ChatId, long MessageId)> SendSnackAsync(
        long chatId,
        int messageId,
        string clickedNavigation, 
        CancellationToken cancellationToken = default)
    {
        // List<MenuButtonModel> buttonModels = new List<MenuButtonModel>();
        var products = await productService.GetProductsAsync(1, cancellationToken);

        // foreach (var item in products)
        // {
        //     buttonModels.Add(new MenuButtonModel(item.Name, item.Id.ToString()));
        // }
        Console.WriteLine("------------------------------------------------------------------");
        Console.WriteLine(clickedNavigation);
        Console.WriteLine("------------------------------------------------------------------");

        List<MenuButtonModel> buttonModels = new()
        {
            new MenuButtonModel("Button11", "Button11Value"),
            new MenuButtonModel("Button21", "Button21Value"),
            new MenuButtonModel("Button31", "Button31Value"),
            new MenuButtonModel("Button41", "Button41Value"),
            new MenuButtonModel("Button51", "Button51Value"), 
            new MenuButtonModel("Button61", "Button61Value"),
            new MenuButtonModel("Button71", "Button71Value"),
            new MenuButtonModel("Button81", "Button81Value"),
            new MenuButtonModel("Button91", "Button91Value"),
        };

        const int column = 2;
        const int row = 2;
        if(clickedNavigation.Contains("toPage"))
        {
            var replyMenu = botClient.GetPaginationInlineKeyboard(buttonModels, column, row, clickedNavigation);
            var product = products.FirstOrDefault();

            var message = await botClient.SendTextMessageAsync(
                text: $"name: {product.Name}\nprice: {product.Price}",
                chatId: chatId,
                replyMarkup: replyMenu,
                parseMode: ParseMode.Markdown,
                cancellationToken: cancellationToken);

            return (chatId, message.MessageId);
        }
        else
        {
            var replyMenu = botClient.GetPaginationInlineKeyboard(buttonModels, column, row);
            var product = products.FirstOrDefault();

            var message = await botClient.SendTextMessageAsync(
                text: $"name: {product.Name}\nprice: {product.Price}",
                chatId: chatId,
                replyMarkup: replyMenu,
                parseMode: ParseMode.Markdown,
                cancellationToken: cancellationToken);

            return (chatId, message.MessageId);
        }
    }
    
    private async ValueTask RemoveKeyboardAsync(long chatId, CancellationToken cancellationToken = default)
    {
        var message = await botClient.SendTextMessageAsync(
            text: " .",
            chatId: chatId,
            replyMarkup: new ReplyKeyboardRemove(),
            cancellationToken: cancellationToken);
        await botClient.DeleteMessageAsync(chatId, message.MessageId, cancellationToken: cancellationToken);
    }


    private InlineKeyboardMarkup GetInlineKeyboard(string[][] matrix)
    {
        var buttonMatrix = new InlineKeyboardButton[matrix.GetLength(0)][];
        for(int i = 0; i < matrix.GetLength(0); i++)
            buttonMatrix[i] = matrix[i]
                .Select(x => InlineKeyboardButton.WithCallbackData(localization.GetValue(x), x)).ToArray();
        
        return new InlineKeyboardMarkup(buttonMatrix);
    }

    private static string GetCheckmarkOrEmpty(string userLanguage, string languageCode)
        => string.Equals(userLanguage, languageCode, StringComparison.InvariantCultureIgnoreCase)
        ? "✅"
        :string.Empty;
}