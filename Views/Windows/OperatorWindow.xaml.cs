using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using WKFTournamentIS.Core.Models;
using WKFTournamentIS.Resources;
using WKFTournamentIS.Services;
using WKFTournamentIS.Services.Interfaces;
using WKFTournamentIS.Views.Windows.Interfaces;

namespace WKFTournamentIS.Views.Windows
{
    public partial class OperatorWindow : Window, IInitializableWithUser
    {
        private readonly ITournamentService _tournamentService;
        private readonly ICategoryService _categoryService;
        private readonly ICategoryInTournamentService _categoryInTournamentService;
        private readonly ICompetitorRegistrationService _competitorRegistrationService;
        private readonly ThemeManager _themeManager;


        private User _currentUser;
        private DispatcherTimer _timeTimer;
        private string _currentSection = "Tournaments";

        public ObservableCollection<Tournament> TournamentsList { get; set; } = new ObservableCollection<Tournament>();
        public List<string> AvailableThemes { get; set; }
        public string CurrentTheme { get; set; }

        public OperatorWindow(ITournamentService tournamentService, ICategoryService categoryService,
                              ICategoryInTournamentService categoryInTournamentService,
                              ICompetitorRegistrationService competitorRegistrationService,
                              ThemeManager themeManager)
        {
            InitializeComponent();
            _tournamentService = tournamentService;
            _categoryService = categoryService;
            _categoryInTournamentService = categoryInTournamentService;
            _competitorRegistrationService = competitorRegistrationService;
            _themeManager = themeManager;

            InitializeTimer();

            AvailableThemes = _themeManager.AvailableThemes;
            CurrentTheme = _themeManager.CurrentTheme;

            DataContext = this;
            this.Loaded += OperatorWindow_Loaded;

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
            if (!this.IsLoaded || cmbLanguage.SelectedItem == null) return;

            if (cmbLanguage.SelectedItem is ComboBoxItem selectedItem)
            {
                string languageCode = selectedItem.Tag.ToString();
                if (languageCode == System.Globalization.CultureInfo.CurrentUICulture.Name.Split('-')[0]) return;

                TranslationManager.SetLanguage(languageCode);

                var newOperatorWindow = App.ServiceProvider.GetRequiredService<OperatorWindow>();
                newOperatorWindow.InitializeUser(_currentUser);
                newOperatorWindow.Show();
                this.Close();
            }
        }

        private async void OperatorWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadInitialDataAsync();
            ShowContent("Tournaments");
        }

        private async Task LoadInitialDataAsync()
        {
            StatusText.Text = Strings.Status_Loading;
            await LoadTournamentsAsync();
            UpdateItemCount();
        }

        public void InitializeUser(User authenticatedUser)
        {
            _currentUser = authenticatedUser;
            if (_currentUser != null)
            {
                UserInfoText.Text = string.Format(Strings.WelcomeUser, _currentUser.Username);
            }
        }

        private void InitializeTimer()
        {
            _timeTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timeTimer.Tick += (s, e) => CurrentTimeText.Text = DateTime.Now.ToString("HH:mm:ss");
            _timeTimer.Start();
            CurrentTimeText.Text = DateTime.Now.ToString("HH:mm:ss");
        }

        private async Task LoadTournamentsAsync(string searchTerm = null)
        {
            try
            {
                StatusText.Text = Strings.Status_LoadingTournaments;
                var tournaments = string.IsNullOrWhiteSpace(searchTerm)
                    ? await _tournamentService.GetAllTournamentsAsync()
                    : await _tournamentService.SearchTournamentsAsync(searchTerm);

                TournamentsList.Clear();
                if (tournaments != null)
                {
                    foreach (var tournament in tournaments.OrderByDescending(t => t.BeginningDateTime))
                    {
                        TournamentsList.Add(tournament);
                    }
                }
                StatusText.Text = string.Format(Strings.Status_ShowingTournaments, TournamentsList.Count);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{Strings.Error_LoadingTournaments} {ex.Message}", Strings.Error_Generic, MessageBoxButton.OK, MessageBoxImage.Error);
                TournamentsList.Clear();
                StatusText.Text = Strings.Error_LoadingTournaments;
            }
            UpdateItemCount();
        }

        private void UpdateItemCount()
        {
            ItemCountText.Text = (TournamentsList?.Count ?? 0).ToString();
        }

        private void UpdateNavigationButtons(string activeSection)
        {
            BtnTournaments.Style = (Style)FindResource("NavigationButtonStyle");
            BtnSettings.Style = (Style)FindResource("NavigationButtonStyle");

            Button activeButton = null;
            switch (activeSection)
            {
                case "Tournaments": activeButton = BtnTournaments; break;
                case "Settings": activeButton = BtnSettings; break;
            }

            if (activeButton != null)
            {
                activeButton.Style = (Style)FindResource("ActiveNavigationButtonStyle");
            }
        }

        private void ShowContent(string section)
        {
            TournamentsContent.Visibility = Visibility.Collapsed;
            SettingsContent.Visibility = Visibility.Collapsed;

            _currentSection = section;
            UpdateNavigationButtons(section);

            bool showSearch = (section == "Tournaments");
            TxtSearch.Visibility = showSearch ? Visibility.Visible : Visibility.Collapsed;

            TxtSearch.Text = "";

            switch (section)
            {
                case "Tournaments":
                    TournamentsContent.Visibility = Visibility.Visible;
                    PageTitle.Text = Strings.PageTitle_Tournaments;
                    PageSubtitle.Text = Strings.PageSubtitle_Tournaments_Operator;
                    TxtSearch.ToolTip = Strings.Search_Tournaments;
                    break;
                case "Settings":
                    SettingsContent.Visibility = Visibility.Visible;
                    PageTitle.Text = Strings.NavButton_Settings;
                    PageSubtitle.Text = Strings.Settings_PageSubtitle;
                    break;
            }
            UpdateItemCount();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                this.DragMove();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(Strings.Confirm_CloseApp_Text, Strings.Confirm_CloseApp_Caption, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void BtnMaximizeRestore_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = (this.WindowState == WindowState.Maximized) ? WindowState.Normal : WindowState.Maximized;
            btnMaximizeRestore.Style = (Style)FindResource(this.WindowState == WindowState.Maximized ? "AdminRestoreButtonStyle" : "AdminMaximizeButtonStyle");
            btnMaximizeRestore.ToolTip = this.WindowState == WindowState.Maximized ? Strings.Tooltip_Restore : Strings.Tooltip_Maximize;
        }

        private async void BtnTournaments_Click(object sender, RoutedEventArgs e)
        {
            ShowContent("Tournaments");
            BtnViewDetails.IsEnabled = false;
            await LoadTournamentsAsync();
        }

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            ShowContent("Settings");
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(Strings.Confirm_Logout_Text, Strings.Confirm_Logout_Caption, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _timeTimer?.Stop();
                var loginWindow = App.ServiceProvider.GetRequiredService<LoginWindow>();
                loginWindow.Show();
                this.Close();
            }
        }

        private void BtnViewDetails_Click(object sender, RoutedEventArgs e)
        {
            if (TournamentsDataGrid.SelectedItem is Tournament selectedTournament)
            {
                var detailsWindow = new TournamentCategoriesWindow(
                    selectedTournament,
                    _categoryService,
                    _categoryInTournamentService,
                    _competitorRegistrationService,
                    true)
                {
                    Owner = this
                };
                detailsWindow.ShowDialog();
            }
        }

        private void TournamentsDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (BtnViewDetails.IsEnabled)
            {
                BtnViewDetails_Click(sender, e);
            }
        }

        private void TournamentsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BtnViewDetails.IsEnabled = TournamentsDataGrid.SelectedItem != null;
        }

        private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ThemeComboBox.SelectedItem is string selectedTheme)
            {
                if (selectedTheme != _themeManager.CurrentTheme)
                {
                    _themeManager.ApplyTheme(selectedTheme);
                }
            }
        }

        private async void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = TxtSearch.Text;
            await LoadTournamentsAsync(searchText);
        }

        protected override void OnClosed(EventArgs e)
        {
            _timeTimer?.Stop();
            base.OnClosed(e);
        }
    }
}