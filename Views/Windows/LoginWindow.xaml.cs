using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MaterialDesignThemes.Wpf;
using Microsoft.Extensions.DependencyInjection;
using WKFTournamentIS.Core.Enums;
using WKFTournamentIS.Core.Models;
using WKFTournamentIS.Resources;
using WKFTournamentIS.Services;
using WKFTournamentIS.Services.Interfaces;
using WKFTournamentIS.Views.Windows.Interfaces;

namespace WKFTournamentIS.Views.Windows
{
    public partial class LoginWindow : Window
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly ThemeManager _themeManager;
        private bool _isLoggingIn = false;

        public LoginWindow(IAuthenticationService authenticationService, ThemeManager themeManager)
        {
            InitializeComponent();
            _authenticationService = authenticationService;
            _themeManager = themeManager;

            txtUsername.Focus();
            txtPassword.KeyDown += TxtPassword_KeyDown;
            txtUsername.KeyDown += TxtUsername_KeyDown;

            SetInitialLanguageInComboBox();
        }

        private void SetInitialLanguageInComboBox()
        {
            string currentLanguageCode = System.Globalization.CultureInfo.CurrentUICulture.Name.Split('-')[0];

            foreach (ComboBoxItem item in cmbLanguage.Items)
            {
                if (item.Tag?.ToString() == currentLanguageCode)
                {
                    cmbLanguage.SelectedItem = item;
                    break;
                }
            }
        }

        private void CmbLanguage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!this.IsLoaded || cmbLanguage.SelectedItem == null)
            {
                return;
            }

            if (cmbLanguage.SelectedItem is ComboBoxItem selectedItem)
            {
                string languageCode = selectedItem.Tag.ToString();

                if (languageCode == System.Globalization.CultureInfo.CurrentUICulture.Name.Split('-')[0])
                {
                    return;
                }

                TranslationManager.SetLanguage(languageCode);

                var newLoginWindow = new LoginWindow(App.ServiceProvider.GetRequiredService<IAuthenticationService>(), App.ServiceProvider.GetRequiredService<ThemeManager>());
                newLoginWindow.Show();
                this.Close();
            }
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                this.DragMove();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void TxtUsername_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                txtPassword.Focus();
            }
        }

        private void TxtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!_isLoggingIn)
                {
                    BtnLogin_Click(sender, e);
                }
            }
        }

        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (_isLoggingIn)
            {
                return;
            }

            _isLoggingIn = true;
            btnLogin.IsEnabled = false;

            try
            {
                HideError();

                string username = txtUsername.Text.Trim();
                string password = txtPassword.Password;

                if (string.IsNullOrWhiteSpace(username))
                {
                    ShowError(GetLocalizedMessage("MsgEmptyUsername"));
                    txtUsername.Focus();
                    return;
                }

                if (string.IsNullOrEmpty(password))
                {
                    ShowError(GetLocalizedMessage("MsgEmptyPassword"));
                    txtPassword.Focus();
                    return;
                }

                User authenticatedUser = await _authenticationService.LoginAsync(username, password);

                if (authenticatedUser != null)
                {
                    _themeManager.SetCurrentUser(authenticatedUser);

                    Dispatcher.Invoke(() =>
                    {
                        IInitializableWithUser windowWithUser = null;

                        switch (authenticatedUser.Role)
                        {
                            case UserRole.Administrator:
                                windowWithUser = App.ServiceProvider.GetRequiredService<AdminWindow>();
                                break;
                            case UserRole.Operator:
                                windowWithUser = App.ServiceProvider.GetRequiredService<OperatorWindow>();
                                break;
                            default:
                                ShowError(GetLocalizedMessage("UnknownRole"));
                                return;
                        }

                        if (windowWithUser is Window nextWindow)
                        {
                            windowWithUser.InitializeUser(authenticatedUser);
                            nextWindow.Show();
                            this.Close();
                            return;
                        }
                    });
                }
                else
                {
                    ShowError(GetLocalizedMessage("InvalidCredentials"));
                    txtPassword.Clear();
                    txtUsername.Focus();
                }
            }
            catch (Exception)
            {
                ShowError(GetLocalizedMessage("LoginError"));
            }
            finally
            {
                if (this.IsLoaded)
                {
                    _isLoggingIn = false;
                    btnLogin.IsEnabled = true;
                }
            }
        }

        private string GetLocalizedMessage(string key, params object[] args)
        {
            string message = Strings.ResourceManager.GetString(key, Strings.Culture);

            if (string.IsNullOrEmpty(message))
            {
                return key;
            }

            return args.Length > 0 ? string.Format(message, args) : message;
        }

        private void ShowError(string message)
        {
            lblError.Text = message;
            if (Application.Current.Resources["ErrorBackgroundBrush"] is SolidColorBrush brush)
            {
                errorBorder.Background = brush;
            }
            else
            {
                errorBorder.Background = new SolidColorBrush(Colors.Red);
            }
            errorBorder.Visibility = Visibility.Visible;
        }

        private void ShowSuccess(string message)
        {
            lblError.Text = message;
            if (Application.Current.Resources["SuccessBackgroundBrush"] is SolidColorBrush brush)
            {
                errorBorder.Background = brush;
            }
            else
            {
                errorBorder.Background = new SolidColorBrush(Colors.Green);
            }
            errorBorder.Visibility = Visibility.Visible;
        }

        private void HideError()
        {
            errorBorder.Visibility = Visibility.Collapsed;
            lblError.Text = "";
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            if (Application.Current.Windows.Count == 0 ||
                (Application.Current.MainWindow == this && Application.Current.Windows.OfType<Window>().All(w => w == this)))
            {
                Application.Current.Shutdown();
            }
        }

        private void txtUsername_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}