using Microsoft.UI.Xaml.Controls;
using BombiHighSchool.App.ViewModels;
namespace BombiHighSchool.App.Views;
public sealed partial class RankingsPage : Page
{
    public RankingsViewModel ViewModel { get; } = new();
    public RankingsPage() { InitializeComponent(); Loaded += async (_, _) => await ViewModel.LoadCommand.ExecuteAsync(null); }
}
