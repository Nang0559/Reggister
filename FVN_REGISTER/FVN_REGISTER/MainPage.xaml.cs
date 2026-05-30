namespace FVN_REGISTER
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
#if WINDOWS && DEBUG
        // ✅ Tự động mở DevTools khi app chạy
        blazorWebView.BlazorWebViewInitialized += (s, e) =>
        {
            e.WebView.CoreWebView2.Settings.AreDevToolsEnabled = true;
            e.WebView.CoreWebView2.OpenDevToolsWindow();
        };
#endif
        }
    }
}
