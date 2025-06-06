using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using WKFTournamentIS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System;
using WKFTournamentIS.Views.Windows;
using WKFTournamentIS.Services.Implementation;
using WKFTournamentIS.Services.Interfaces;
using WKFTournamentIS.Services;

namespace WKFTournamentIS
{
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; }
        public static IConfiguration Configuration { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            TranslationManager.SetLanguage("sr");

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            Configuration = builder.Build();

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            ServiceProvider = serviceCollection.BuildServiceProvider();

            var themeManager = ServiceProvider.GetRequiredService<ThemeManager>();
            themeManager.InitializeTheme();

            var startupWindow = ServiceProvider.GetRequiredService<LoginWindow>();
            startupWindow.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IConfiguration>(Configuration);

            string connectionString = Configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseMySql(connectionString,
                                 ServerVersion.AutoDetect(connectionString)
                )
            );

            services.AddTransient<LoginWindow>();
            services.AddTransient<OperatorWindow>();
            services.AddTransient<AdminWindow>();

            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<ITournamentService, TournamentService>();
            services.AddScoped<IOperatorService, OperatorService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<ICompetitorService, CompetitorService>();
            services.AddScoped<ICategoryInTournamentService, CategoryInTournamentService>();
            services.AddScoped<ICompetitorRegistrationService, CompetitorRegistrationService>();
            services.AddScoped<IClubService, ClubService>();
            services.AddScoped<IUserService, UserService>();

            services.AddSingleton<ThemeManager>();
        }
    }
}