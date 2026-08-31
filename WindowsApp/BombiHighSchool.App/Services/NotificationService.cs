using System.Collections.ObjectModel;

namespace BombiHighSchool.App.Services;

public sealed class NotificationService
{
    private static readonly ObservableCollection<AppNotification> ItemsInternal = [];
    public ReadOnlyObservableCollection<AppNotification> Items { get; } = new(ItemsInternal);
    public static NotificationService Instance { get; } = new();

    public void Success(string message) => Add("Success", message, NotificationKind.Success);
    public void Info(string message) => Add("Information", message, NotificationKind.Info);
    public void Warning(string message) => Add("Attention", message, NotificationKind.Warning);
    public void Error(string message) => Add("Error", message, NotificationKind.Error);

    public void Remove(Guid id)
    {
        var item = ItemsInternal.FirstOrDefault(x => x.Id == id);
        if (item is not null) ItemsInternal.Remove(item);
    }

    private static void Add(string title, string message, NotificationKind kind)
    {
        ItemsInternal.Insert(0, new AppNotification(Guid.NewGuid(), title, message, kind, DateTimeOffset.Now));
        while (ItemsInternal.Count > 50) ItemsInternal.RemoveAt(ItemsInternal.Count - 1);
    }
}

public enum NotificationKind { Info, Success, Warning, Error }
public sealed record AppNotification(Guid Id, string Title, string Message, NotificationKind Kind, DateTimeOffset CreatedAt);
