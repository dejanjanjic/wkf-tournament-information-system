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
    public partial class AdminWindow : Window, IInitializableWithUser
    {
        private readonly ITournamentService _tournamentService;
        private readonly IOperatorService _operatorService;
        private readonly ICategoryService _categoryService;
        private readonly ICompetitorService _competitorService;
        private readonly IClubService _clubService;
        private readonly ThemeManager _themeManager;

        private User _currentUser;
        private DispatcherTimer _timeTimer;
        private string _currentSection = "Tournaments";

        public ObservableCollection<Tournament> TournamentsList { get; set; } = new ObservableCollection<Tournament>();
        public ObservableCollection<User> OperatorsList { get; set; } = new ObservableCollection<User>();
        public ObservableCollection<Category> CategoriesList { get; set; } = new ObservableCollection<Category>();
        public ObservableCollection<Competitor> CompetitorsList { get; set; } = new ObservableCollection<Competitor>();
        public ObservableCollection<Club> ClubsList { get; set; } = new ObservableCollection<Club>();

        public List<string> AvailableThemes { get; set; }
        public string CurrentTheme { get; set; }

        public AdminWindow(ITournamentService tournamentService, IOperatorService operatorService, ICategoryService categoryService, ICompetitorService competitorService, IClubService clubService, ThemeManager themeManager)
        {
            InitializeComponent();
            _tournamentService = tournamentService;
            _operatorService = operatorService;
            _categoryService = categoryService;
            _competitorService = competitorService;
            _clubService = clubService;
            _themeManager = themeManager;

            InitializeTimer();

            AvailableThemes = _themeManager.AvailableThemes;
            CurrentTheme = _themeManager.CurrentTheme;

            DataContext = this;
            this.Loaded += AdminWindow_Loaded;

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

                var newAdminWindow = App.ServiceProvider.GetRequiredService<AdminWindow>();
                newAdminWindow.InitializeUser(_currentUser);
                newAdminWindow.Show();
                this.Close();
            }
        }

        private async void AdminWindow_Loaded(object sender, RoutedEventArgs e)
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
            if (_currentSection == "Tournaments") UpdateItemCount();
        }

        private async Task LoadOperatorsAsync(string searchTerm = null)
        {
            try
            {
                StatusText.Text = Strings.Status_LoadingOperators;
                var operators = string.IsNullOrWhiteSpace(searchTerm)
                    ? await _operatorService.GetAllOperatorsAsync()
                    : await _operatorService.SearchOperatorsAsync(searchTerm);

                OperatorsList.Clear();
                if (operators != null)
                {
                    foreach (var op in operators.OrderBy(u => u.Username))
                    {
                        OperatorsList.Add(op);
                    }
                }
                StatusText.Text = string.Format(Strings.Status_ShowingOperators, OperatorsList.Count);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{Strings.Error_LoadingOperators} {ex.Message}", Strings.Error_Generic, MessageBoxButton.OK, MessageBoxImage.Error);
                OperatorsList.Clear();
                StatusText.Text = Strings.Error_LoadingOperators;
            }
            if (_currentSection == "Operators") UpdateItemCount();
        }

        private async Task LoadCategoriesAsync(string searchTerm = null)
        {
            try
            {
                StatusText.Text = Strings.Status_LoadingCategories;
                var categories = string.IsNullOrWhiteSpace(searchTerm)
                    ? await _categoryService.GetAllCategoriesAsync()
                    : await _categoryService.SearchCategoriesAsync(searchTerm);

                CategoriesList.Clear();
                if (categories != null)
                {
                    foreach (var category in categories.OrderBy(c => c.Name))
                    {
                        CategoriesList.Add(category);
                    }
                }
                StatusText.Text = string.Format(Strings.Status_ShowingCategories, CategoriesList.Count);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{Strings.Error_LoadingCategories} {ex.Message}", Strings.Error_Generic, MessageBoxButton.OK, MessageBoxImage.Error);
                CategoriesList.Clear();
                StatusText.Text = Strings.Error_LoadingCategories;
            }
            if (_currentSection == "Categories") UpdateItemCount();
        }

        private async Task LoadCompetitorsAsync(string searchTerm = null)
        {
            try
            {
                StatusText.Text = Strings.Status_LoadingCompetitors;
                var competitors = string.IsNullOrWhiteSpace(searchTerm)
                    ? await _competitorService.GetAllCompetitorsAsync()
                    : await _competitorService.SearchCompetitorsAsync(searchTerm);

                CompetitorsList.Clear();
                if (competitors != null)
                {
                    foreach (var competitor in competitors.OrderBy(c => c.LastName).ThenBy(c => c.FirstName))
                    {
                        CompetitorsList.Add(competitor);
                    }
                }
                StatusText.Text = string.Format(Strings.Status_ShowingCompetitors, CompetitorsList.Count);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{Strings.Error_LoadingCompetitors} {ex.Message}", Strings.Error_Generic, MessageBoxButton.OK, MessageBoxImage.Error);
                CompetitorsList.Clear();
                StatusText.Text = Strings.Error_LoadingCompetitors;
            }
            if (_currentSection == "Competitors") UpdateItemCount();
        }

        private async Task LoadClubsAsync(string searchTerm = null)
        {
            try
            {
                StatusText.Text = Strings.Status_LoadingClubs;
                var clubs = string.IsNullOrWhiteSpace(searchTerm)
                    ? await _clubService.GetAllClubsAsync()
                    : await _clubService.SearchClubsAsync(searchTerm);

                ClubsList.Clear();
                if (clubs != null)
                {
                    foreach (var club in clubs.OrderBy(c => c.Name))
                    {
                        ClubsList.Add(club);
                    }
                }
                StatusText.Text = string.Format(Strings.Status_ShowingClubs, ClubsList.Count);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{Strings.Error_LoadingClubs} {ex.Message}", Strings.Error_Generic, MessageBoxButton.OK, MessageBoxImage.Error);
                ClubsList.Clear();
                StatusText.Text = Strings.Error_LoadingClubs;
            }
            if (_currentSection == "Clubs") UpdateItemCount();
        }

        private void UpdateItemCount()
        {
            int count = 0;
            switch (_currentSection)
            {
                case "Tournaments": count = TournamentsList?.Count ?? 0; break;
                case "Operators": count = OperatorsList?.Count ?? 0; break;
                case "Categories": count = CategoriesList?.Count ?? 0; break;
                case "Competitors": count = CompetitorsList?.Count ?? 0; break;
                case "Clubs": count = ClubsList?.Count ?? 0; break;
            }
            ItemCountText.Text = count.ToString();
        }

        private void UpdateNavigationButtons(string activeSection)
        {
            BtnTournaments.Style = (Style)FindResource("NavigationButtonStyle");
            BtnOperators.Style = (Style)FindResource("NavigationButtonStyle");
            BtnCategories.Style = (Style)FindResource("NavigationButtonStyle");
            BtnCompetitors.Style = (Style)FindResource("NavigationButtonStyle");
            BtnClubs.Style = (Style)FindResource("NavigationButtonStyle");
            BtnSettings.Style = (Style)FindResource("NavigationButtonStyle");

            Button activeButton = null;
            switch (activeSection)
            {
                case "Tournaments": activeButton = BtnTournaments; break;
                case "Operators": activeButton = BtnOperators; break;
                case "Categories": activeButton = BtnCategories; break;
                case "Competitors": activeButton = BtnCompetitors; break;
                case "Clubs": activeButton = BtnClubs; break;
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
            OperatorsContent.Visibility = Visibility.Collapsed;
            CategoriesContent.Visibility = Visibility.Collapsed;
            CompetitorsContent.Visibility = Visibility.Collapsed;
            ClubsContent.Visibility = Visibility.Collapsed;
            SettingsContent.Visibility = Visibility.Collapsed;

            _currentSection = section;
            UpdateNavigationButtons(section);
            TxtSearch.Text = "";

            bool showSearchAndAdd = section != "Settings";
            TxtSearch.Visibility = showSearchAndAdd ? Visibility.Visible : Visibility.Collapsed;
            BtnAdd.Visibility = showSearchAndAdd ? Visibility.Visible : Visibility.Collapsed;

            switch (section)
            {
                case "Tournaments":
                    TournamentsContent.Visibility = Visibility.Visible;
                    PageTitle.Text = Strings.PageTitle_Tournaments;
                    PageSubtitle.Text = Strings.PageSubtitle_Tournaments;
                    TxtSearch.ToolTip = Strings.Search_Tournaments;
                    BtnAdd.Content = Strings.Add_Tournament;
                    break;
                case "Operators":
                    OperatorsContent.Visibility = Visibility.Visible;
                    PageTitle.Text = Strings.PageTitle_Operators;
                    PageSubtitle.Text = Strings.PageSubtitle_Operators;
                    TxtSearch.ToolTip = Strings.Search_Operators;
                    BtnAdd.Content = Strings.Add_Operator;
                    break;
                case "Categories":
                    CategoriesContent.Visibility = Visibility.Visible;
                    PageTitle.Text = Strings.PageTitle_Categories;
                    PageSubtitle.Text = Strings.PageSubtitle_Categories;
                    TxtSearch.ToolTip = Strings.Search_Categories;
                    BtnAdd.Content = Strings.Add_Category;
                    break;
                case "Competitors":
                    CompetitorsContent.Visibility = Visibility.Visible;
                    PageTitle.Text = Strings.PageTitle_Competitors;
                    PageSubtitle.Text = Strings.PageSubtitle_Competitors;
                    TxtSearch.ToolTip = Strings.Search_Competitors;
                    BtnAdd.Content = Strings.Add_Competitor;
                    break;
                case "Clubs":
                    ClubsContent.Visibility = Visibility.Visible;
                    PageTitle.Text = Strings.PageTitle_Clubs;
                    PageSubtitle.Text = Strings.PageSubtitle_Clubs;
                    TxtSearch.ToolTip = Strings.Search_Clubs;
                    BtnAdd.Content = Strings.Add_Club;
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
            ResetActionButtons();
            await LoadTournamentsAsync();
        }

        private async void BtnOperators_Click(object sender, RoutedEventArgs e)
        {
            ShowContent("Operators");
            ResetActionButtons();
            await LoadOperatorsAsync();
        }

        private async void BtnCategories_Click(object sender, RoutedEventArgs e)
        {
            ShowContent("Categories");
            ResetActionButtons();
            await LoadCategoriesAsync();
        }

        private async void BtnCompetitors_Click(object sender, RoutedEventArgs e)
        {
            ShowContent("Competitors");
            ResetActionButtons();
            await LoadCompetitorsAsync();
        }

        private async void BtnClubs_Click(object sender, RoutedEventArgs e)
        {
            ShowContent("Clubs");
            ResetActionButtons();
            await LoadClubsAsync();
        }

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            ShowContent("Settings");
            ResetActionButtons();
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

        private async void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            switch (_currentSection)
            {
                case "Tournaments": await AddNewTournamentAsync(); break;
                case "Operators": await AddNewOperatorAsync(); break;
                case "Categories": await AddNewCategoryAsync(); break;
                case "Competitors": await AddNewCompetitorAsync(); break;
                case "Clubs": await AddNewClubAsync(); break;
            }
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

        private async Task AddNewTournamentAsync()
        {
            var addTournamentWindow = new AddTournamentWindow { Owner = this };
            if (addTournamentWindow.ShowDialog() == true)
            {
                var tournamentFromDialog = addTournamentWindow.Tournament;
                try
                {
                    StatusText.Text = string.Format(Strings.Status_Adding, tournamentFromDialog.Name);
                    Tournament createdTournament = await _tournamentService.CreateTournamentAsync(tournamentFromDialog);
                    if (createdTournament?.Id > 0)
                    {
                        await LoadTournamentsAsync();
                        StatusText.Text = string.Format(Strings.Status_Added, createdTournament.Name);
                        TournamentsDataGrid.SelectedItem = TournamentsList.FirstOrDefault(t => t.Id == createdTournament.Id);
                        TournamentsDataGrid.ScrollIntoView(TournamentsDataGrid.SelectedItem);
                    }
                    else
                    {
                        MessageBox.Show(Strings.Error_SavingTournament, Strings.Error_Saving, MessageBoxButton.OK, MessageBoxImage.Error);
                        StatusText.Text = Strings.Error_SavingTournament;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, ex is InvalidOperationException or ArgumentException ? Strings.Error_Validation : Strings.Error_Database, MessageBoxButton.OK, MessageBoxImage.Error);
                    StatusText.Text = Strings.Error_SavingTournament;
                }
            }
        }
        private void EditTournament(Tournament tournament)
        {
            var editTournamentWindow = new AddTournamentWindow(tournament) { Owner = this };
            if (editTournamentWindow.ShowDialog() == true)
            {
                _ = UpdateTournamentAsync(editTournamentWindow.Tournament);
            }
        }

        private async Task UpdateTournamentAsync(Tournament tournamentToUpdate)
        {
            try
            {
                StatusText.Text = string.Format(Strings.Status_Updating, tournamentToUpdate.Name);
                bool success = await _tournamentService.UpdateTournamentAsync(tournamentToUpdate);
                if (success)
                {
                    await LoadTournamentsAsync();
                    StatusText.Text = string.Format(Strings.Status_Updated, tournamentToUpdate.Name);
                    var updatedItemInList = TournamentsList.FirstOrDefault(t => t.Id == tournamentToUpdate.Id);
                    if (updatedItemInList != null)
                    {
                        TournamentsDataGrid.SelectedItem = updatedItemInList;
                        TournamentsDataGrid.ScrollIntoView(updatedItemInList);
                    }
                }
                else
                {
                    MessageBox.Show(Strings.Error_UpdatingTournament, Strings.Error_Updating, MessageBoxButton.OK, MessageBoxImage.Error);
                    StatusText.Text = Strings.Error_UpdatingTournament;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex is InvalidOperationException or ArgumentException ? Strings.Error_Validation : Strings.Error_Database, MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = Strings.Error_UpdatingTournament;
            }
        }

        private void BtnEditTournament_Click(object sender, RoutedEventArgs e)
        {
            if (TournamentsDataGrid.SelectedItem is Tournament selectedTournament)
            {
                EditTournament(selectedTournament);
            }
        }
        private async void BtnDeleteTournament_Click(object sender, RoutedEventArgs e)
        {
            if (TournamentsDataGrid.SelectedItem is Tournament selectedTournament)
            {
                var message = string.Format(Strings.Confirm_DeleteTournament_Text, selectedTournament.Name);
                if (MessageBox.Show(message, Strings.Confirm_Delete_Caption, MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    try
                    {
                        StatusText.Text = string.Format(Strings.Status_Deleting, selectedTournament.Name);
                        bool deleted = await _tournamentService.DeleteTournamentAsync(selectedTournament.Id);
                        if (deleted)
                        {
                            TournamentsList.Remove(selectedTournament);
                            UpdateItemCount();
                            StatusText.Text = Strings.Status_Deleted;
                            ResetActionButtons();
                        }
                        else
                        {
                            MessageBox.Show(Strings.Error_Deleting, Strings.Error_Generic, MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, Strings.Error_Deleting, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void BtnViewDetails_Click(object sender, RoutedEventArgs e)
        {
            if (TournamentsDataGrid.SelectedItem is Tournament selectedTournament)
            {
                var categoryService = App.ServiceProvider.GetRequiredService<ICategoryService>();
                var categoryInTournamentService = App.ServiceProvider.GetRequiredService<ICategoryInTournamentService>();
                var competitorRegistrationService = App.ServiceProvider.GetRequiredService<ICompetitorRegistrationService>();

                var detailsWindow = new TournamentCategoriesWindow(selectedTournament, categoryService, categoryInTournamentService, competitorRegistrationService)
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

        private async Task AddNewOperatorAsync()
        {
            var dialog = new AddEditOperatorWindow { Owner = this };
            if (dialog.ShowDialog() == true)
            {
                User operatorData = dialog.Operator;
                if (operatorData != null && !string.IsNullOrWhiteSpace(operatorData.Username) && !string.IsNullOrWhiteSpace(operatorData.PasswordHash))
                {
                    try
                    {
                        StatusText.Text = string.Format(Strings.Status_Adding, operatorData.Username);
                        User createdOperator = await _operatorService.CreateOperatorAsync(operatorData.Username, operatorData.PasswordHash);
                        if (createdOperator != null)
                        {
                            await LoadOperatorsAsync();
                            StatusText.Text = string.Format(Strings.Status_Added, createdOperator.Username);
                            OperatorsDataGrid.SelectedItem = OperatorsList.FirstOrDefault(o => o.Id == createdOperator.Id);
                            OperatorsDataGrid.ScrollIntoView(OperatorsDataGrid.SelectedItem);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, ex is InvalidOperationException ? Strings.Error_Validation : Strings.Error_Generic, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private async void BtnEditOperatorPassword_Click(object sender, RoutedEventArgs e)
        {
            if (OperatorsDataGrid.SelectedItem is User selectedOperator)
            {
                var dialog = new AddEditOperatorWindow(selectedOperator) { Owner = this };
                if (dialog.ShowDialog() == true)
                {
                    User operatorData = dialog.Operator;
                    if (operatorData != null && !string.IsNullOrWhiteSpace(operatorData.PasswordHash))
                    {
                        try
                        {
                            StatusText.Text = string.Format(Strings.Status_Updating, selectedOperator.Username);
                            bool success = await _operatorService.UpdateOperatorPasswordAsync(operatorData.Id, operatorData.PasswordHash);
                            if (success)
                            {
                                MessageBox.Show(string.Format(Strings.PasswordChangedSuccess, selectedOperator.Username), Strings.Success_Caption, MessageBoxButton.OK, MessageBoxImage.Information);
                                StatusText.Text = Strings.Status_Updated;
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, Strings.Error_Generic, MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
        }

        private async void BtnDeleteOperator_Click(object sender, RoutedEventArgs e)
        {
            if (OperatorsDataGrid.SelectedItem is User selectedOperator)
            {
                var message = string.Format(Strings.Confirm_DeleteOperator_Text, selectedOperator.Username);
                if (MessageBox.Show(message, Strings.Confirm_Delete_Caption, MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    try
                    {
                        StatusText.Text = string.Format(Strings.Status_Deleting, selectedOperator.Username);
                        bool success = await _operatorService.DeleteOperatorAsync(selectedOperator.Id);
                        if (success)
                        {
                            OperatorsList.Remove(selectedOperator);
                            UpdateItemCount();
                            StatusText.Text = Strings.Status_Deleted;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, Strings.Error_Deleting, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private async Task AddNewCategoryAsync()
        {
            var dialog = new AddEditCategoryWindow { Owner = this };
            if (dialog.ShowDialog() == true)
            {
                Category category = dialog.Category;
                if (category != null && !string.IsNullOrWhiteSpace(category.Name))
                {
                    try
                    {
                        StatusText.Text = string.Format(Strings.Status_Adding, category.Name);
                        Category createdCategory = await _categoryService.CreateCategoryAsync(category);
                        if (createdCategory != null)
                        {
                            await LoadCategoriesAsync();
                            StatusText.Text = string.Format(Strings.Status_Added, createdCategory.Name);
                            CategoriesDataGrid.SelectedItem = CategoriesList.FirstOrDefault(c => c.Id == createdCategory.Id);
                            CategoriesDataGrid.ScrollIntoView(CategoriesDataGrid.SelectedItem);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, ex is InvalidOperationException ? Strings.Error_Validation : Strings.Error_Generic, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private async void BtnEditCategory_Click(object sender, RoutedEventArgs e)
        {
            if (CategoriesDataGrid.SelectedItem is Category selectedCategory)
            {
                var dialog = new AddEditCategoryWindow(selectedCategory) { Owner = this };
                if (dialog.ShowDialog() == true)
                {
                    Category category = dialog.Category;
                    if (category != null && !string.IsNullOrWhiteSpace(category.Name))
                    {
                        try
                        {
                            StatusText.Text = string.Format(Strings.Status_Updating, category.Name);
                            bool success = await _categoryService.UpdateCategoryAsync(category);
                            if (success)
                            {
                                await LoadCategoriesAsync();
                                StatusText.Text = string.Format(Strings.Status_Updated, category.Name);
                                CategoriesDataGrid.SelectedItem = CategoriesList.FirstOrDefault(c => c.Id == category.Id);
                                CategoriesDataGrid.ScrollIntoView(CategoriesDataGrid.SelectedItem);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, ex is InvalidOperationException ? Strings.Error_Validation : Strings.Error_Generic, MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
        }

        private async void BtnDeleteCategory_Click(object sender, RoutedEventArgs e)
        {
            if (CategoriesDataGrid.SelectedItem is Category selectedCategory)
            {
                var message = string.Format(Strings.Confirm_DeleteCategory_Text, selectedCategory.Name);
                if (MessageBox.Show(message, Strings.Confirm_Delete_Caption, MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    try
                    {
                        StatusText.Text = string.Format(Strings.Status_Deleting, selectedCategory.Name);
                        bool success = await _categoryService.DeleteCategoryAsync(selectedCategory.Id);
                        if (success)
                        {
                            CategoriesList.Remove(selectedCategory);
                            UpdateItemCount();
                            StatusText.Text = Strings.Status_Deleted;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, Strings.Error_Deleting, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private async Task AddNewCompetitorAsync()
        {
            var dialog = new AddEditCompetitorWindow { Owner = this };
            if (dialog.ShowDialog() == true)
            {
                Competitor competitor = dialog.Competitor;
                if (competitor != null)
                {
                    try
                    {
                        StatusText.Text = string.Format(Strings.Status_Adding, $"{competitor.FirstName} {competitor.LastName}");
                        Competitor createdCompetitor = await _competitorService.CreateCompetitorAsync(competitor);
                        if (createdCompetitor != null)
                        {
                            await LoadCompetitorsAsync();
                            StatusText.Text = string.Format(Strings.Status_Added, $"{createdCompetitor.FirstName} {createdCompetitor.LastName}");
                            CompetitorsDataGrid.SelectedItem = CompetitorsList.FirstOrDefault(c => c.Id == createdCompetitor.Id);
                            CompetitorsDataGrid.ScrollIntoView(CompetitorsDataGrid.SelectedItem);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, ex is InvalidOperationException ? Strings.Error_Validation : Strings.Error_Generic, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private async void BtnEditCompetitor_Click(object sender, RoutedEventArgs e)
        {
            if (CompetitorsDataGrid.SelectedItem is Competitor selectedCompetitor)
            {
                var dialog = new AddEditCompetitorWindow(selectedCompetitor) { Owner = this };
                if (dialog.ShowDialog() == true)
                {
                    Competitor competitor = dialog.Competitor;
                    if (competitor != null)
                    {
                        try
                        {
                            StatusText.Text = string.Format(Strings.Status_Updating, $"{competitor.FirstName} {competitor.LastName}");
                            bool success = await _competitorService.UpdateCompetitorAsync(competitor);
                            if (success)
                            {
                                await LoadCompetitorsAsync();
                                StatusText.Text = string.Format(Strings.Status_Updated, $"{competitor.FirstName} {competitor.LastName}");
                                CompetitorsDataGrid.SelectedItem = CompetitorsList.FirstOrDefault(c => c.Id == competitor.Id);
                                CompetitorsDataGrid.ScrollIntoView(CompetitorsDataGrid.SelectedItem);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, ex is InvalidOperationException ? Strings.Error_Validation : Strings.Error_Generic, MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
        }

        private async void BtnDeleteCompetitor_Click(object sender, RoutedEventArgs e)
        {
            if (CompetitorsDataGrid.SelectedItem is Competitor selectedCompetitor)
            {
                var message = string.Format(Strings.Confirm_DeleteCompetitor_Text, selectedCompetitor.FirstName, selectedCompetitor.LastName);
                if (MessageBox.Show(message, Strings.Confirm_Delete_Caption, MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    try
                    {
                        StatusText.Text = string.Format(Strings.Status_Deleting, $"{selectedCompetitor.FirstName} {selectedCompetitor.LastName}");
                        bool success = await _competitorService.DeleteCompetitorAsync(selectedCompetitor.Id);
                        if (success)
                        {
                            CompetitorsList.Remove(selectedCompetitor);
                            UpdateItemCount();
                            StatusText.Text = Strings.Status_Deleted;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"{ex.Message}\n{Strings.Error_CompetitorConnected}", Strings.Error_Deleting, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private async Task AddNewClubAsync()
        {
            var dialog = new AddEditClubWindow { Owner = this };
            if (dialog.ShowDialog() == true)
            {
                var club = dialog.Club;
                try
                {
                    StatusText.Text = string.Format(Strings.Status_Adding, club.Name);
                    Club createdClub = await _clubService.CreateClubAsync(club);
                    if (createdClub != null)
                    {
                        await LoadClubsAsync();
                        StatusText.Text = string.Format(Strings.Status_Added, createdClub.Name);
                        ClubsDataGrid.SelectedItem = ClubsList.FirstOrDefault(c => c.Id == createdClub.Id);
                        ClubsDataGrid.ScrollIntoView(ClubsDataGrid.SelectedItem);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, Strings.Error_Database, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void BtnEditClub_Click(object sender, RoutedEventArgs e)
        {
            if (ClubsDataGrid.SelectedItem is Club selectedClub)
            {
                var dialog = new AddEditClubWindow(selectedClub) { Owner = this };
                if (dialog.ShowDialog() == true)
                {
                    try
                    {
                        StatusText.Text = string.Format(Strings.Status_Updating, dialog.Club.Name);
                        bool success = await _clubService.UpdateClubAsync(dialog.Club);
                        if (success)
                        {
                            await LoadClubsAsync();
                            StatusText.Text = string.Format(Strings.Status_Updated, dialog.Club.Name);
                            ClubsDataGrid.SelectedItem = ClubsList.FirstOrDefault(c => c.Id == dialog.Club.Id);
                            ClubsDataGrid.ScrollIntoView(ClubsDataGrid.SelectedItem);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, Strings.Error_Database, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private async void BtnDeleteClub_Click(object sender, RoutedEventArgs e)
        {
            if (ClubsDataGrid.SelectedItem is Club selectedClub)
            {
                var message = string.Format(Strings.Confirm_DeleteClub_Text, selectedClub.Name);
                if (MessageBox.Show(message, Strings.Confirm_Delete_Caption, MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    try
                    {
                        StatusText.Text = string.Format(Strings.Status_Deleting, selectedClub.Name);
                        bool deleted = await _clubService.DeleteClubAsync(selectedClub.Id);
                        if (deleted)
                        {
                            ClubsList.Remove(selectedClub);
                            UpdateItemCount();
                            StatusText.Text = Strings.Status_Deleted;
                            ResetActionButtons();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"{Strings.Error_Deleting}: {ex.Message}\n{Strings.Error_ClubConnected}", Strings.Error_Generic, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void ResetActionButtons()
        {
            BtnEditTournament.IsEnabled = false;
            BtnDeleteTournament.IsEnabled = false;
            BtnViewDetails.IsEnabled = false;

            BtnEditOperatorPassword.IsEnabled = false;
            BtnDeleteOperator.IsEnabled = false;

            BtnEditCategory.IsEnabled = false;
            BtnDeleteCategory.IsEnabled = false;

            BtnEditCompetitor.IsEnabled = false;
            BtnDeleteCompetitor.IsEnabled = false;

            BtnEditClub.IsEnabled = false;
            BtnDeleteClub.IsEnabled = false;
        }

        private void TournamentsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var hasSelection = TournamentsDataGrid.SelectedItem != null;
            BtnEditTournament.IsEnabled = hasSelection;
            BtnDeleteTournament.IsEnabled = hasSelection;
            BtnViewDetails.IsEnabled = hasSelection;
        }

        private void OperatorsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool hasSelection = OperatorsDataGrid.SelectedItem != null;
            BtnEditOperatorPassword.IsEnabled = hasSelection;
            BtnDeleteOperator.IsEnabled = hasSelection;
        }

        private void CategoriesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool hasSelection = CategoriesDataGrid.SelectedItem != null;
            BtnEditCategory.IsEnabled = hasSelection;
            BtnDeleteCategory.IsEnabled = hasSelection;
        }

        private void CompetitorsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool hasSelection = CompetitorsDataGrid.SelectedItem != null;
            BtnEditCompetitor.IsEnabled = hasSelection;
            BtnDeleteCompetitor.IsEnabled = hasSelection;
        }

        private void ClubsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool hasSelection = ClubsDataGrid.SelectedItem != null;
            BtnEditClub.IsEnabled = hasSelection;
            BtnDeleteClub.IsEnabled = hasSelection;
        }

        private async void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = TxtSearch.Text;
            switch (_currentSection)
            {
                case "Tournaments": await LoadTournamentsAsync(searchText); break;
                case "Operators": await LoadOperatorsAsync(searchText); break;
                case "Categories": await LoadCategoriesAsync(searchText); break;
                case "Competitors": await LoadCompetitorsAsync(searchText); break;
                case "Clubs": await LoadClubsAsync(searchText); break;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _timeTimer?.Stop();
            base.OnClosed(e);
        }
    }
}