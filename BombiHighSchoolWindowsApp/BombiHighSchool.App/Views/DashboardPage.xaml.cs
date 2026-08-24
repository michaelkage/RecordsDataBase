using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using BombiHighSchool.App.Services;

namespace BombiHighSchool.App.Views;

public sealed partial class DashboardPage : Page
{
    private readonly LocalDataService _data = new();

    public DashboardPage()
    {
        InitializeComponent();
        Loaded += DashboardPage_Loaded;
    }

    private async void DashboardPage_Loaded(object sender, RoutedEventArgs e)
    {
        DateText.Text = DateTime.Now.ToString("dddd, dd MMMM yyyy");
        try
        {
            var data = await _data.LoadAsync();
            StudentsCount.Text = data.Students.Count(s => !s.IsArchived).ToString();
            SubjectsCount.Text = data.Subjects.Count.ToString();
            ScoresCount.Text = data.Scores.Count.ToString();
            BackupState.Text = System.IO.File.Exists(_data.BackupPath) ? "Backup available" : "No backup yet";
            AttentionText.Text = data.Students.Count(s => !s.IsArchived && string.IsNullOrWhiteSpace(s.Name)) > 0 ? "Some student profiles are incomplete." : "No immediate record issues detected.";
            WarningText.Text = _data.LastLoadWarning ?? "";
        }
        catch (DatabaseUnavailableException ex)
        {
            DatabaseState.Text = "Recovery needed";
            DatabaseState.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Orange);
            AttentionText.Text = ex.Message;
            WarningText.Text = ex.Message;
        }
        catch (Exception ex) { AttentionText.Text = ex.Message; WarningText.Text = ex.Message; }
    }

    private void AddStudent_Click(object sender, RoutedEventArgs e) => Frame?.Navigate(typeof(StudentsPage));
    private void Students_Click(object sender, RoutedEventArgs e) => Frame?.Navigate(typeof(StudentsPage));
    private void Scores_Click(object sender, RoutedEventArgs e) => Frame?.Navigate(typeof(ScoresPage));
    private void Subjects_Click(object sender, RoutedEventArgs e) => Frame?.Navigate(typeof(SubjectsPage));
    private void Rankings_Click(object sender, RoutedEventArgs e) => Frame?.Navigate(typeof(RankingsPage));
    private void Settings_Click(object sender, RoutedEventArgs e) => Frame?.Navigate(typeof(SettingsPage));

    private async void Backup_Click(object sender, RoutedEventArgs e)
    {
        try { await _data.CreateBackupAsync(); BackupState.Text = "Backup just created"; }
        catch (Exception ex) { WarningText.Text = $"Backup failed: {ex.Message}"; }
    }
}
