using Microsoft.UI.Xaml.Controls;
using BombiHighSchool.App.ViewModels;

namespace BombiHighSchool.App.Views;

public sealed partial class AdminAcademicPage : Page
{
    public AdminAcademicViewModel ViewModel { get; } = new();
    public AdminAcademicPage()
    {
        InitializeComponent();
        Loaded += async (_, _) => await ViewModel.LoadAsync();
    }
}
