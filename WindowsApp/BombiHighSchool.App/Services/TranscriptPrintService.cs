using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Printing;
using BombiHighSchool.App;
using Windows.Graphics.Printing;

namespace BombiHighSchool.App.Services;

public sealed class TranscriptPrintService
{
    private PrintManager? _printManager;
    private PrintDocument? _printDocument;
    private IPrintDocumentSource? _source;
    private UIElement? _printPage;

    public void Register(UIElement printPage)
    {
        Unregister();
        _printPage = printPage;
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(MainWindow.Current);
        _printManager = PrintManagerInterop.GetForWindow(hwnd);
        _printManager.PrintTaskRequested += OnPrintTaskRequested;
        _printDocument = new PrintDocument();
        _source = _printDocument.DocumentSource;
        _printDocument.Paginate += OnPaginate;
        _printDocument.GetPreviewPage += OnGetPreviewPage;
        _printDocument.AddPages += OnAddPages;
    }

    public void Unregister()
    {
        if (_printManager is not null) _printManager.PrintTaskRequested -= OnPrintTaskRequested;
        if (_printDocument is not null)
        {
            _printDocument.Paginate -= OnPaginate;
            _printDocument.GetPreviewPage -= OnGetPreviewPage;
            _printDocument.AddPages -= OnAddPages;
        }
        _printManager = null; _printDocument = null; _source = null; _printPage = null;
    }

    public async Task<bool> ShowPrintUIAsync()
    {
        if (!PrintManager.IsSupported()) return false;
        try
        {
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(MainWindow.Current);
            await PrintManagerInterop.ShowPrintUIForWindowAsync(hwnd);
            return true;
        }
        catch { return false; }
    }

    private void OnPrintTaskRequested(PrintManager sender, PrintTaskRequestedEventArgs args)
    {
        var task = args.Request.CreatePrintTask("Bombi High School Transcript", request => request.SetSource(_source));
    }

    private void OnPaginate(object sender, PaginateEventArgs e)
    {
        _printDocument!.SetPreviewPageCount(1, PreviewPageCountType.Final);
    }

    private void OnGetPreviewPage(object sender, GetPreviewPageEventArgs e)
    {
        _printDocument!.SetPreviewPage(e.PageNumber, _printPage!);
    }

    private void OnAddPages(object sender, AddPagesEventArgs e)
    {
        _printDocument!.AddPage(_printPage!);
        _printDocument.AddPagesComplete();
    }
}
