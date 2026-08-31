using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BombiHighSchool.App.Models;
using BombiHighSchool.App.Services;

namespace BombiHighSchool.App.ViewModels;

public partial class RankingsViewModel : ObservableObject
{
    private readonly RankingService _service = new();
    private readonly LocalDataService _dataService = new();
    private bool _periodLoaded;
    [ObservableProperty] private ObservableCollection<StudentRanking> rankings = [];
    [ObservableProperty] private string classFilter = "All classes";
    [ObservableProperty] private string armFilter = "All arms";
    [ObservableProperty] private string session = "2026/2027";
    [ObservableProperty] private string term = "First Term";
    public string[] Classes { get; } = ["All classes", "JSS1", "JSS2", "JSS3", "SS1", "SS2", "SS3"];
    public string[] Arms { get; } = ["All arms", .. SchoolRules.Arms];
    public string[] Terms => SchoolRules.Terms;

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (!_periodLoaded)
        {
            var data = await _dataService.LoadAsync();
            Session = data.CurrentAcademicPeriod.Session;
            Term = data.CurrentAcademicPeriod.Term;
            _periodLoaded = true;
        }
        var classFilter = ClassFilter == "All classes" ? null : ClassFilter;
        var armFilter = ArmFilter == "All arms" ? null : ArmFilter;
        Rankings = new(await _service.GetStudentRankingsAsync(classFilter, armFilter, Session, Term));
    }

    partial void OnClassFilterChanged(string value) => _ = LoadAsync();
    partial void OnArmFilterChanged(string value) => _ = LoadAsync();
    partial void OnTermChanged(string value) => _ = LoadAsync();
}
