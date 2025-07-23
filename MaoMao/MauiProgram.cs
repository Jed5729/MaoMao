using Microsoft.Extensions.Logging;
using MauiIcons.Core;
using MauiIcons.Cupertino;
using MaoMao.Views;
using MaoMao.ViewModels;
using CommunityToolkit.Maui;

namespace MaoMao
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .UseMauiCommunityToolkitMediaElement()
                .UseCupertinoMauiIcons()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.AddSingleton<Home>();
            builder.Services.AddSingleton<HomeViewModel>();

            builder.Services.AddTransient<Player>();
            builder.Services.AddTransient<PlayerViewModel>();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
