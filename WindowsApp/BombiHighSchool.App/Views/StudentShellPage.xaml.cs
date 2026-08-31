using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using BombiHighSchool.App.Services;

namespace BombiHighSchool.App.Views;

public sealed partial class StudentShellPage : Page
{
    public StudentShellPage()
    {
        InitializeComponent();
        Loaded += StudentShellPage_Loaded;
    }

    private void StudentShellPage_Loaded(object sender, RoutedEventArgs e)
    {
        if (!SessionService.IsStudentSession)
        {
            Frame?.Navigate(typeof(LoginPage));
            return;
        }
        StudentIdText.Text = SessionService.StudentId ?? "";
        WelcomeText.Text = $"Welcome back, {SessionService.Username ?? "Student"}";
        StudentNavigation.SelectedItem = StudentNavigation.MenuItems[0];
        NavigateTo("Overview");
    }

    private void Navigation_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is NavigationViewItem item && item.Tag is string section) NavigateTo(section);
    }

    private void NavigateTo(string section)
    {
        if (!SessionService.IsStudentSession) { SessionService.Clear(); Frame?.Navigate(typeof(LoginPage)); return; }
        switch (section)
        {
            case "Overview": ContentFrame.Navigate(typeof(StudentPortalPage)); break;
            case "Profile": ContentFrame.Navigate(typeof(StudentProfilePage)); break;
            case "Subjects": ContentFrame.Navigate(typeof(StudentSubjectsPage)); break;
            case "Results": ContentFrame.Navigate(typeof(StudentResultsPage)); break;
            case "Transcript": ContentFrame.Navigate(typeof(StudentTranscriptPage)); break;
            case "Logout": SignOut_Click(this, new RoutedEventArgs()); return;
        }
        _ = LoadCurrentPageAsync();
    }

    private async Task LoadCurrentPageAsync()
    {
        if (string.IsNullOrWhiteSpace(SessionService.StudentId)) return;
        switch (ContentFrame.Content)
        {
            case StudentPortalPage overview: await overview.LoadStudentAsync(SessionService.StudentId); break;
            case StudentProfilePage profile: await profile.LoadStudentAsync(SessionService.StudentId); break;
            case StudentSubjectsPage subjects: await subjects.LoadStudentAsync(SessionService.StudentId); break;
            case StudentResultsPage results: await results.LoadStudentAsync(SessionService.StudentId); break;
            case StudentTranscriptPage transcript: await transcript.LoadStudentAsync(SessionService.StudentId); break;
        }
    }

    private async void Refresh_Click(object sender, RoutedEventArgs e) => await LoadCurrentPageAsync();

    private void SignOut_Click(object sender, RoutedEventArgs e)
    {
        SessionService.Clear();
        Frame?.Navigate(typeof(LoginPage));
    }
}
