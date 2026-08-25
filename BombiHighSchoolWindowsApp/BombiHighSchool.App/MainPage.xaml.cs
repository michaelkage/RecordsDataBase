using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using BombiHighSchool.App.Services;
using BombiHighSchool.App.Views;
using Microsoft.Extensions.DependencyInjection; // Add this

namespace BombiHighSchool.App;

public sealed partial class MainPage : Page
{
    // Clean, loosely coupled service tracking
    private readonly ILocalDataService _dataService;

    // Receive the service via constructor injection
    public MainPage(ILocalDataService dataService) 
    { 
        _dataService = dataService;
        InitializeComponent(); 
        Loaded += MainPage_Loaded; 
    }

    private void MainPage_Loaded(object sender, RoutedEventArgs e) 
    { 
        AppNavigation.SelectedItem = AppNavigation.MenuItems[0]; 
        NavigateToPage<DashboardPage>(); // Use DI-aware navigation
    }

    private void Navigation_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is NavigationViewItem item && item.Tag is string section) NavigateToSection(section);
        else if (args.SelectedItemContainer is NavigationViewItem container && container.Tag is string footerSection) NavigateToSection(footerSection);
    }

    private void NavigateToSection(string section)
    {
        switch (section)
        {
            case "Dashboard": NavigateToPage<DashboardPage>(); break;
            case "Students": NavigateToPage<StudentsPage>(); break;
            case "Subjects": NavigateToPage<SubjectsPage>(); break;
            case "Academic": NavigateToPage<AdminAcademicPage>(); break;
            case "Scores": NavigateToPage<ScoresPage>(); break;
            case "Rankings": NavigateToPage<RankingsPage>(); break;
            case "Settings": NavigateToPage<SettingsPage>(); break;
            case "Logout": SessionService.Clear(); Frame?.Navigate(typeof(LoginPage)); break;
            default: NavigateToPage<DashboardPage>(); break;
        }
    }

    // NEW HACK: Resolves pages through your DI Container instead of parameterless types
    private void NavigateToPage<T>() where T : Page
    {
        var pageInstance = ((App)Application.Current).Services.GetRequiredService<T>();
        ContentFrame.Content = pageInstance;
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
        
        NavigateToPage<StudentsPage>();
        if (ContentFrame.Content is StudentsPage page) page.FocusSearch(query);
    }

    private void NewStudent_Click(object sender, RoutedEventArgs e)
    {
        NavigateToPage<StudentsPage>();
        if (ContentFrame.Content is StudentsPage page) page.StartNewStudent();
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
        var dialog = new ContentDialog { Title = title, Content = message, CloseButtonText = "OK", XamlRoot = XamlRoot };
        await dialog.ShowAsync();
    }
}
