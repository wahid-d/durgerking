namespace DurgerKing.Models;

public class MenuButtonModel
{
    public MenuButtonModel(string text, string? value = null, IEnumerable<MenuButtonModel>? childrenItems = null)
    {
        Text = text;
        Value = value;
        ChildrenItems = childrenItems?.ToList();
    }

    public string Text { get; }
    public string? Value { get; }
    public List<MenuButtonModel>? ChildrenItems { get; }
}

internal class MenuButtonNumberModel : MenuButtonModel
{
    public MenuButtonNumberModel(string text, int numberPage, string? value = null,
        IEnumerable<MenuButtonNumberModel>? childrenItems = null) : base(text, value)
    {
        NumberPage = numberPage;
        var list = childrenItems?.ToList();

        list?.ForEach(x => x.ParentItemValue = value ?? string.Empty);
        ChildrenItems = list;
    }

    public new List<MenuButtonNumberModel>? ChildrenItems { get; }

    public int NumberPage { get; }
    public string? ParentItemValue { get; private set; }
}