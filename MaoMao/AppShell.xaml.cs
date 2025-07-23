using MaoMao.Views;
using MauiIcons.Core;

namespace MaoMao
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            _ = new MauiIcon();
            Routing.RegisterRoute(nameof(Home), typeof(Home));
            Routing.RegisterRoute(nameof(Player), typeof(Player));
        }
    }
}
