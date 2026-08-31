using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using BombiHighSchool.App.Services;
using BombiHighSchool.App.Views;
using Microsoft.Extensions.DependencyInjection;

namespace BombiHighSchool.App;

public sealed partial class MainPage : Page
{
    private readonly LocalDataService _dataService;

    public MainPage(LocalDataService dataService)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        InitializeComponent();
        Loaded += MainPage_Loaded;
        KeyDown += MainPage_KeyDown;
    }

    private void MainPage_Loaded(object sender, RoutedEventArgs e)
    {
        if (AppNavigation.MenuItems.Count > 0 && AppNavigation.SelectedItem is null)
            AppNavigation.SelectedItem = AppNavigation.MenuItems[0];
    }

    private void AppNavigation_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is NavigationViewItem item && item.Tag is string section)
        {
            NavigateToSection(section);
            return;
        }

        if (args.SelectedItemContainer is NavigationViewItem container && container.Tag is string footerSection)
            NavigateToSection(footerSection);
    }

    private void AppNavigation_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        if (args.InvokedItemContainer is NavigationViewItem item && item.Tag is string section)
            NavigateToSection(section);
    }

    private void AppNavigation_DisplayModeChanged(NavigationView sender, NavigationViewDisplayModeChangedEventArgs args)
    {
        // NavigationView manages compact/expanded behavior natively.
    }

    private void NavigateToSection(string section)
    {
        var pageType = section switch
        {
            "dashboard" => typeof(DashboardPage),
            "students" => typeof(StudentsPage),
            "subjects" => typeof(SubjectsPage),
            "academic" => typeof(AdminAcademicPage),
            "scores" => typeof(ScoresPage),
            "rankings" => typeof(RankingsPage),
            "settings" => typeof(SettingsPage),
            _ => typeof(DashboardPage)
        };

        NavigateToPage(pageType);
    }

    private void NavigateToPage(Type pageType)
    {
        if (ContentFrame.Content?.GetType() == pageType)
            return;

        var page = ((App)Application.Current).Services.GetService(pageType) as Page;
        if (page is null)
            throw new InvalidOperationException($"No page service is registered for {pageType.Name}.");

        ContentFrame.Content = page;
    }

    private void MainPage_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        var ctrlDown = Microsoft.UI.Input.InputKeyboardSource
            .GetKeyStateForCurrentThread(Windows.System.VirtualKey.Control)
            .HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);

        if (!ctrlDown) return;

        switch (e.Key)
        {
            case Windows.System.VirtualKey.K:
                GlobalSearchBox.Focus(FocusState.Keyboard);
                e.Handled = true;
                break;
            case Windows.System.VirtualKey.N:
                NewStudent_Click(this, new RoutedEventArgs());
                e.Handled = true;
                break;
            case Windows.System.VirtualKey.Number1: SelectMenuItem(0); e.Handled = true; break;
            case Windows.System.VirtualKey.Number2: SelectMenuItem(1); e.Handled = true; break;
            case Windows.System.VirtualKey.Number3: SelectMenuItem(2); e.Handled = true; break;
            case Windows.System.VirtualKey.Number4: SelectMenuItem(3); e.Handled = true; break;
            case Windows.System.VirtualKey.Number5: SelectMenuItem(4); e.Handled = true; break;
        }
    }

    private void SelectMenuItem(int index)
    {
        if (index >= 0 && index < AppNavigation.MenuItems.Count)
            AppNavigation.SelectedItem = AppNavigation.MenuItems[index];
    }

    private void GlobalSearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        var query = sender.Text?.Trim();
        if (string.IsNullOrWhiteSpace(query)) return;

        SelectMenuItem(1);
        if (ContentFrame.Content is StudentsPage page)
            page.FocusSearch(query);
    }

    private void NewStudent_Click(object sender, RoutedEventArgs e)
    {
        SelectMenuItem(1);
        if (ContentFrame.Content is StudentsPage page)
            page.StartNewStudent();
    }

    private async void Backup_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await _dataService.CreateBackupAsync();
            await ShowMessageAsync("Backup created", "The local school database has been backed up successfully.");
        }
        catch (Exception ex)
        {
            await ShowMessageAsync("Backup failed", ex.Message);
        }
    }

    private async Task ShowMessageAsync(string title, string message)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = message,
            CloseButtonText = "OK",
            XamlRoot = XamlRoot
        };
        await dialog.ShowAsync();
    }
}
