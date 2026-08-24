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

        StudentNavigation.SelectedItem = StudentNavigation.MenuItems[0];
        NavigateTo("Overview");
    }

    private void Navigation_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is NavigationViewItem item && item.Tag is string section)
            NavigateTo(section);
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
            case "Logout": SessionService.Clear(); Frame?.Navigate(typeof(LoginPage)); break;
        }
        if (ContentFrame.Content is StudentPortalPage overview) _ = overview.LoadStudentAsync(SessionService.StudentId!);
        else if (ContentFrame.Content is StudentProfilePage profile) _ = profile.LoadStudentAsync(SessionService.StudentId!);
        else if (ContentFrame.Content is StudentSubjectsPage subjects) _ = subjects.LoadStudentAsync(SessionService.StudentId!);
        else if (ContentFrame.Content is StudentResultsPage results) _ = results.LoadStudentAsync(SessionService.StudentId!);
        else if (ContentFrame.Content is StudentTranscriptPage transcript) _ = transcript.LoadStudentAsync(SessionService.StudentId!);
    }
}
