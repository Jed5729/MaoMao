using MaoMao.Services;
using MaoMao.Views;
using MauiIcons.Core;

namespace MaoMao
{
    public partial class AppShell : Shell
    {
        public AppShell(ThemeManager themeManager)
        {
            InitializeComponent();
            _ = new MauiIcon();
            Routing.RegisterRoute(nameof(Home), typeof(Home));
            Routing.RegisterRoute(nameof(Player), typeof(Player));
            Routing.RegisterRoute(nameof(Search), typeof(Search));
            Routing.RegisterRoute(nameof(Downloads), typeof(Downloads));
            Routing.RegisterRoute(nameof(Watchlists), typeof(Watchlists));
            Routing.RegisterRoute(nameof(History), typeof(History));
            Routing.RegisterRoute(nameof(Account), typeof(Account));
            Routing.RegisterRoute(nameof(Settings), typeof(Settings));
            themeManager.SetTheme(themeManager.GetSavedThemeOrDefault());
		}

        public bool IsNotAndroid => DeviceInfo.Current.Platform != DevicePlatform.Android;

        public bool IsAndroid => DeviceInfo.Current.Platform == DevicePlatform.Android;
    }
}
