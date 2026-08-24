using Microsoft.UI.Xaml.Controls;
using BombiHighSchool.App.ViewModels;
namespace BombiHighSchool.App.Views;
public sealed partial class ScoresPage : Page
{
    public ScoresViewModel ViewModel { get; } = new();
    public ScoresPage() { InitializeComponent(); Loaded += async (_, _) => await ViewModel.LoadCommand.ExecuteAsync(null); }
}
