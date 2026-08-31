using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace BombiHighSchool.App.Services;

public static class AppUxServices
{
    public static async Task<bool> ConfirmAsync(XamlRoot? root, string title, string message, string primaryText = "Continue", string closeText = "Cancel")
    {
        if (root is null) return false;

        var dialog = new ContentDialog
        {
            Title = title,
            Content = message,
            PrimaryButtonText = primaryText,
            CloseButtonText = closeText,
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = root
        };

        return await dialog.ShowAsync() == ContentDialogResult.Primary;
    }

    public static void ShowInfoBar(InfoBar infoBar, string title, string message, InfoBarSeverity severity = InfoBarSeverity.Informational)
    {
        infoBar.Title = title;
        infoBar.Message = message;
        infoBar.Severity = severity;
        infoBar.IsOpen = true;
    }
}
