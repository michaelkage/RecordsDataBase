using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BombiHighSchool.App.Services;

namespace BombiHighSchool.App.ViewModels;

public partial class RankingsViewModel : ObservableObject
{
    private readonly RankingService _service = new();
    [ObservableProperty] private ObservableCollection<StudentRanking> rankings = [];
    [ObservableProperty] private string classFilter = "All classes";
    public string[] Classes { get; } = ["All classes", "JSS1", "JSS2", "JSS3", "SS1", "SS2", "SS3"];
    [RelayCommand] public async Task LoadAsync() { var filter = ClassFilter == "All classes" ? null : ClassFilter; Rankings = new(await _service.GetStudentRankingsAsync(filter)); }
    partial void OnClassFilterChanged(string value) => _ = LoadAsync();
}
