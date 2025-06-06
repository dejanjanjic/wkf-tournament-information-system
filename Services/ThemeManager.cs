using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WKFTournamentIS.Core.Models;
using WKFTournamentIS.Services.Interfaces;

namespace WKFTournamentIS.Services
{
    public class ThemeManager
    {
        private const string DefaultTheme = "Light";
        private readonly IServiceProvider _serviceProvider;
        private User _currentUser;

        public List<string> AvailableThemes { get; } = new List<string> { "Light", "Dark", "Blue" };

        public string CurrentTheme { get; private set; }

        public ThemeManager(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            CurrentTheme = DefaultTheme;
        }

        public void SetCurrentUser(User user)
        {
            _currentUser = user;
            string userTheme = string.IsNullOrEmpty(user?.PreferredTheme) ? DefaultTheme : user.PreferredTheme;
            ApplyTheme(userTheme, saveToDb: false);
        }

        public void ApplyTheme(string themeName, bool saveToDb = true)
        {
            if (string.IsNullOrEmpty(themeName) || !AvailableThemes.Contains(themeName))
            {
                themeName = DefaultTheme;
            }

            var app = Application.Current;
            if (app == null) return;

            var oldThemeDictionary = app.Resources.MergedDictionaries
                .FirstOrDefault(rd => rd.Source != null && rd.Source.OriginalString.Contains("/Themes/"));

            if (oldThemeDictionary != null)
            {
                if (oldThemeDictionary.Source.OriginalString.Contains(themeName))
                    return;
                app.Resources.MergedDictionaries.Remove(oldThemeDictionary);
            }

            ResourceDictionary newThemeDictionary = new ResourceDictionary
            {
                Source = new Uri($"/WKFTournamentIS;component/Resources/Themes/{themeName}.xaml", UriKind.RelativeOrAbsolute)
            };
            app.Resources.MergedDictionaries.Add(newThemeDictionary);

            CurrentTheme = themeName;

            if (saveToDb && _currentUser != null)
            {
                _ = SaveThemeToDatabaseAsync(themeName);
            }
        }

        private async Task SaveThemeToDatabaseAsync(string themeName)
        {
            if (_currentUser == null) return;

            using (var scope = _serviceProvider.CreateScope())
            {
                var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
                await userService.UpdateUserThemeAsync(_currentUser.Id, themeName);
            }
        }

        public void InitializeTheme()
        {
            ApplyTheme(CurrentTheme, saveToDb: false);
        }
    }
}