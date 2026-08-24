using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using BombiHighSchool.App.Views;

namespace BombiHighSchool.App;

public sealed partial class MainPage : Page
{
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
            case "Logout": Frame?.Navigate(typeof(LoginPage)); break;
            default: ContentFrame.Navigate(typeof(DashboardPage)); break;
        }
    }
}
