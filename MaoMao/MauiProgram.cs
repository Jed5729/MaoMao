using Microsoft.Extensions.Logging;
using MauiIcons.Core;
using MauiIcons.Cupertino;
using MaoMao.Views;
using MaoMao.ViewModels;
using CommunityToolkit.Maui;
using MaoMao.Services;

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
//#if ANDROID
//                .ConfigureMauiHandlers(x =>
//                {
//                    x.AddHandler<Shell, Platforms.AndroidCustoms.ShellHandlerEx>();
//                })
//#endif
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            _ = new MauiIcon();

            builder.Services.AddSingleton<ThemeManager>();

            builder.Services.AddSingleton<Home>();
            builder.Services.AddSingleton<HomeViewModel>();

            builder.Services.AddTransient<Player>();
            builder.Services.AddTransient<PlayerViewModel>();

            builder.Services.AddSingleton<AndroidMore>();
            builder.Services.AddSingleton<AndroidMoreViewModel>();

            builder.Services.AddSingleton<Settings>();
            builder.Services.AddSingleton<SettingsViewModel>();

            builder.Services.AddSingleton<Search>();
            builder.Services.AddSingleton<Downloads>();
			builder.Services.AddSingleton<History>();
			builder.Services.AddSingleton<Account>();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
