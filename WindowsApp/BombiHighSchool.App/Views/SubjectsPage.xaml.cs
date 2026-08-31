using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using BombiHighSchool.App.ViewModels;

namespace BombiHighSchool.App.Views;

public sealed partial class SubjectsPage : Page
{
    public SubjectsViewModel ViewModel { get; } = new();
    public SubjectsPage() { InitializeComponent(); Loaded += async (_, _) => await ViewModel.LoadCommand.ExecuteAsync(null); }
}
