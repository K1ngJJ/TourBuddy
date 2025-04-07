using TourBuddy.Services.Email;
using TourBuddy.ViewModels;
using Microsoft.Extensions.Logging;
using TourBuddy.Services.Database;
using TourBuddy.Services.Auth;
using TourBuddy.Helpers;
using TourBuddy.ViewModels;
using TourBuddy.Views;
using TourBuddy.Models;

namespace TourBuddy;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif
        // Load settings (async operation in sync context for simplicity)
        var dbSettingsTask = AppSettings.GetDatabaseSettingsAsync();
        var emailSettingsTask = AppSettings.GetEmailSettingsAsync();

        Task.WhenAll(dbSettingsTask, emailSettingsTask).Wait();

        var dbSettings = dbSettingsTask.Result;
        var emailSettings = emailSettingsTask.Result;

        // Register settings
        builder.Services.AddSingleton(dbSettings);
        builder.Services.AddSingleton(emailSettings);

        // Register services
        builder.Services.AddSingleton<ISQLiteService, SQLiteService>();
        builder.Services.AddSingleton<ISQLServerService, SQLServerService>();
        builder.Services.AddSingleton<SyncService>();
        builder.Services.AddSingleton<IAuthService, AuthService>();
        builder.Services.AddSingleton<IEmailService, EmailService>();

        // Register views and view models
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<LoginPage>();

        builder.Services.AddTransient<RegisterViewModel>();
        builder.Services.AddTransient<RegisterPage>();

        builder.Services.AddTransient<ForgotPasswordViewModel>();
        builder.Services.AddTransient<ForgotPasswordPage>();

        builder.Services.AddTransient<ResetPasswordViewModel>();
        builder.Services.AddTransient<ResetPasswordPage>();

        builder.Services.AddTransient<ProfileViewModel>();
        builder.Services.AddTransient<ProfilePage>();

        return builder.Build();
    }
}

	
