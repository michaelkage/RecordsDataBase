using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using BombiHighSchool.App.Services;
using BombiHighSchool.App.Views;

namespace BombiHighSchool.App;

public sealed partial class MainPage : Page
{
    private readonly LocalDataService _dataService = new();

    public MainPage() { InitializeComponent(); Loaded += MainPage_Loaded; }
    private void MainPage_Loaded(object sender, RoutedEventArgs e) { AppNavigation.SelectedItem = AppNavigation.MenuItems[0]; ContentFrame.Navigate(typeof(DashboardPage)); }

    private void Navigation_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is NavigationViewItem item && item.Tag is string section) NavigateToSection(section);
        else if (args.SelectedItemContainer is NavigationViewItem container && container.Tag is string footerSection) NavigateToSection(footerSection);
    }

    private void NavigateToSection(string section)
    {
        switch (section)
        {
            case "Dashboard": ContentFrame.Navigate(typeof(DashboardPage)); break;
            case "Students": ContentFrame.Navigate(typeof(StudentsPage)); break;
            case "Subjects": ContentFrame.Navigate(typeof(SubjectsPage)); break;
            case "Academic": ContentFrame.Navigate(typeof(AdminAcademicPage)); break;
            case "Scores": ContentFrame.Navigate(typeof(ScoresPage)); break;
            case "Rankings": ContentFrame.Navigate(typeof(RankingsPage)); break;
            case "Settings": ContentFrame.Navigate(typeof(SettingsPage)); break;
            case "Logout": SessionService.Clear(); Frame?.Navigate(typeof(LoginPage)); break;
            default: ContentFrame.Navigate(typeof(DashboardPage)); break;
        }
    }

    private void MainPage_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.K && Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(Windows.System.VirtualKey.Control).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down))
        { GlobalSearchBox.Focus(FocusState.Keyboard); e.Handled = true; }
    }

    private void GlobalSearch_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        var query = sender.Text?.Trim();
        if (string.IsNullOrWhiteSpace(query)) return;
        ContentFrame.Navigate(typeof(StudentsPage));
        if (ContentFrame.Content is StudentsPage page) page.FocusSearch(query);
    }

    private void NewStudent_Click(object sender, RoutedEventArgs e)
    {
        ContentFrame.Navigate(typeof(StudentsPage));
        if (ContentFrame.Content is StudentsPage page) page.StartNewStudent();
    }

    private async void Backup_Click(object sender, RoutedEventArgs e)
    {
        try { await _dataService.CreateBackupAsync(); await ShowMessageAsync("Backup created", "The local school database has been backed up successfully."); }
        catch (Exception ex) { await ShowMessageAsync("Backup failed", ex.Message); }
    }

    private async Task ShowMessageAsync(string title, string message)
    {
        var dialog = new ContentDialog { Title = title, Content = message, CloseButtonText = "OK", XamlRoot = XamlRoot };
        await dialog.ShowAsync();
    }
}
