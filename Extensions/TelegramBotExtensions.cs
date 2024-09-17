using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using DurgerKing.Models;

namespace DurgerKing.Extensions;

public static class TelegramBotExtensions
{
    public static InlineKeyboardMarkup? GetPaginationInlineKeyboard(this ITelegramBotClient sourceClient,
        IEnumerable<MenuButtonModel> data, int columns, int rows, string? clickedNavigationData = null)
    {
        // init page number in list
        var pagingList = SetNumberPage(data, columns, rows);

        var parentItemText = string.Empty;
        var isNavigationButton = clickedNavigationData?.Contains("toPage_") ?? false;

        var currentPageIndex = isNavigationButton
            ? int.Parse(clickedNavigationData!.Split("_")[1])
            : 0;
        if (clickedNavigationData != null)
        {
            var findParentText = clickedNavigationData;
            // find clicked data
            var toFindRootList = clickedNavigationData.Contains("parent_") || isNavigationButton;
            if (toFindRootList)
                findParentText = clickedNavigationData.Replace("parent_", string.Empty);

            if (isNavigationButton)
                findParentText = string.Join("_", clickedNavigationData.Split("_").Skip(2));

            var (result, parentData) = FindButtonsByButtonText(pagingList, findParentText, !toFindRootList);

            if (result == null)
                return null;

            pagingList = result.ToList();
            parentItemText = parentData;

            if (currentPageIndex == 0 && !isNavigationButton)
                currentPageIndex = pagingList.FirstOrDefault(x => x.Value == findParentText)?.NumberPage ?? 0;
        }

        // max count page
        var pageCount = pagingList.Last().NumberPage;

        var showList = pagingList.Where(x => x.NumberPage == currentPageIndex).ToList();

        var firstButton = showList.First();

        var resultMenuButtons = showList.Chunk(columns).Select(row => row.Select(button =>
            new InlineKeyboardButton(button.Text)
            {
                CallbackData = button.Value
            })).ToList();

        var navigationButtons = new List<InlineKeyboardButton>();

        // add prev button
        if (currentPageIndex > 0)
            navigationButtons.Add(new InlineKeyboardButton(Settings.ButtonPrevText)
            {
                CallbackData = $"{Settings.PaginationData}_{currentPageIndex - 1}_{firstButton.Value}"
            });

        // add next button
        if (currentPageIndex < pageCount)
        {
            navigationButtons.Add(new InlineKeyboardButton(Settings.ButtonNextText)
            {
                CallbackData = $"{Settings.PaginationData}_{currentPageIndex + 1}_{firstButton.Value}"
            });
        }

        if (navigationButtons.Any())
            resultMenuButtons.Add(navigationButtons);

        if (!string.IsNullOrEmpty(parentItemText))
            resultMenuButtons.Add(new[]
                { new InlineKeyboardButton(Settings.ButtonUpText) { CallbackData = $"parent_{parentItemText}" } });

        return new InlineKeyboardMarkup(resultMenuButtons);
    }

    private static List<MenuButtonNumberModel> SetNumberPage(IEnumerable<MenuButtonModel> modelList, int columns,
        int rows)
    {
        return modelList.Chunk(columns * rows).SelectMany((row, page) =>
                row.Select(newItem => new MenuButtonNumberModel(newItem.Text, page, newItem.Value,
                    newItem.ChildrenItems != null
                        ? SetNumberPage(newItem.ChildrenItems.ToList(), columns, rows)
                        : null)))
            .ToList();
    }

    private static (List<MenuButtonNumberModel>?, string?) FindButtonsByButtonText(List<MenuButtonNumberModel> modelList,
        string findTextButton, bool onlyChildren)
    {
        var findedItem = modelList.FirstOrDefault(x => x.Value == findTextButton);
        if (findedItem != null)
            return onlyChildren
                ? (findedItem.ChildrenItems, findedItem.Value ?? string.Empty)
                : (modelList, findedItem.ParentItemValue);

        foreach (var modelItem in modelList)
        {
            if (modelItem.ChildrenItems == null) continue;

            var (result, parent) = FindButtonsByButtonText(modelItem.ChildrenItems, findTextButton, onlyChildren);
            if (result != null)
                return (result, parent);
        }

        return (null, string.Empty);
    }
}