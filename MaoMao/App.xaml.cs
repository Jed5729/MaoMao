
using CommunityToolkit.Maui.Behaviors;
using MaoMao.Services;

namespace MaoMao
{
    public partial class App : Application
    {
        public App(ThemeManager themeManager)
        {
            InitializeComponent();

            MainPage = new AppShell(themeManager);
            if(Current is not null)
			Current.PageAppearing += (s, e) => // Remove stupid ugly status bar thing :)
			{
				if (e is ContentPage page && page is not null && !page.Behaviors.OfType<StatusBarBehavior>().Any())
				{
					page.Behaviors.Add(new StatusBarBehavior());
				}
			};
        }

        protected override Window CreateWindow(IActivationState? activationState)
		{
            var window = base.CreateWindow(activationState);

            if(DeviceInfo.Idiom == DeviceIdiom.Desktop)
            {
                window.Width = 1000;
                window.Height = 700;
            }

            return window;
		}
    }
}
