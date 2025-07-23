
namespace MaoMao
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
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
