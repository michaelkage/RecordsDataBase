using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using BombiHighSchool.App.ViewModels;

namespace BombiHighSchool.App;

public sealed partial class MainPage : Page
{
    public MainPageViewModel ViewModel { get; } = new();

    public MainPage()
    {
        InitializeComponent();
        Loaded += MainPage_Loaded;
    }

    private async void MainPage_Loaded(object sender, RoutedEventArgs e)
    {
        AppNavigation.SelectedItem = AppNavigation.MenuItems[0];
        await ViewModel.LoadAsync();
    }

    private void Navigation_SelectionChanged(
        NavigationView sender,
        NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is NavigationViewItem item && item.Tag is string section)
        {
            ViewModel.NavigateCommand.Execute(section);
        }
        else if (args.SelectedItemContainer is NavigationViewItem container && container.Tag is string footerSection)
        {
            ViewModel.NavigateCommand.Execute(footerSection);
        }
    }
}
