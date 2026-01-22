using Microsoft.Extensions.Logging;
using MauiIcons.Core;
using MauiIcons.Cupertino;
using MaoMao.Views;
using MaoMao.ViewModels;
using CommunityToolkit.Maui;
using MaoMao.Services;
using System.Net.Http.Headers;

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

            builder.Services.AddSingleton<IThemeManager, ThemeManager>();
            builder.Services.AddSingleton<AuthManager>();
            builder.Services.AddScoped<AuthManager.AuthHandler>();

			builder.Services.AddHttpClient("ApiClient", c =>
			{
				c.BaseAddress = new Uri("https://localhost:7208/");
				c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			}).AddHttpMessageHandler<AuthManager.AuthHandler>();

			builder.Services.AddTransient<Home>();
            builder.Services.AddTransient<HomeViewModel>();

            builder.Services.AddTransient<Player>();
            builder.Services.AddTransient<PlayerViewModel>();

            builder.Services.AddTransient<AndroidMore>();
            builder.Services.AddTransient<AndroidMoreViewModel>();

            builder.Services.AddTransient<Settings>();
            builder.Services.AddTransient<SettingsViewModel>();

            builder.Services.AddTransient<Search>();
            builder.Services.AddTransient<Downloads>();
			builder.Services.AddTransient<History>();
			builder.Services.AddTransient<Account>();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
