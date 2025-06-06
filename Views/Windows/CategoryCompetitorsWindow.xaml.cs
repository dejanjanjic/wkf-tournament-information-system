using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WKFTournamentIS.Core.Models;
using WKFTournamentIS.Services.Interfaces;
using WKFTournamentIS.Resources;
using System.Globalization;

namespace WKFTournamentIS.Views.Windows
{
    public partial class CategoryCompetitorsWindow : Window, INotifyPropertyChanged
    {
        private readonly CategoryInTournament _categoryInTournament;
        private readonly ICompetitorRegistrationService _registrationService;
        private readonly bool _isOperatorView;

        public ObservableCollection<CompetitorInCategoryInTournament> RegisteredCompetitors { get; set; } = new ObservableCollection<CompetitorInCategoryInTournament>();

        private string _pageSubtitleText;
        public string PageSubtitle
        {
            get => _pageSubtitleText;
            set { _pageSubtitleText = value; OnPropertyChanged(nameof(PageSubtitle)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public CategoryCompetitorsWindow(CategoryInTournament categoryInTournament, ICompetitorRegistrationService registrationService, bool isOperatorView = false)
        {
            InitializeComponent();
            _categoryInTournament = categoryInTournament ?? throw new ArgumentNullException(nameof(categoryInTournament));
            _registrationService = registrationService ?? throw new ArgumentNullException(nameof(registrationService));
            _isOperatorView = isOperatorView;

            DataContext = this;
            this.Title = string.Format(CultureInfo.CurrentCulture, Strings.CategoryCompetitors_WindowTitle, _categoryInTournament.Category?.Name);
            UpdatePageSubtitle();
            this.Loaded += CategoryCompetitorsWindow_Loaded;

            SetupOperatorView();
        }

        private void SetupOperatorView()
        {
            if (_isOperatorView)
            {
                BtnAddCompetitor.Visibility = Visibility.Collapsed;
                BtnRemoveCompetitor.Visibility = Visibility.Collapsed;
                BtnUpdatePlacement.Visibility = Visibility.Visible;
            }
            else
            {
                BtnAddCompetitor.Visibility = Visibility.Visible;
                BtnRemoveCompetitor.Visibility = Visibility.Visible;
                BtnUpdatePlacement.Visibility = Visibility.Collapsed;
            }
        }
        private void UpdatePageSubtitle()
        {
            PageSubtitle = string.Format(CultureInfo.CurrentCulture, Strings.CategoryCompetitors_PageSubtitle, _categoryInTournament.Category?.Name, _categoryInTournament.Tournament?.Name);
        }

        private async void CategoryCompetitorsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadRegisteredCompetitorsAsync();
        }

        private async Task LoadRegisteredCompetitorsAsync()
        {
            try
            {
                var registrations = await _registrationService.GetRegistrationsWithDetailsAsync(_categoryInTournament.Id);
                RegisteredCompetitors.Clear();
                if (registrations != null)
                {
                    foreach (var registration in registrations)
                    {
                        RegisteredCompetitors.Add(registration);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(CultureInfo.CurrentCulture, Strings.Error_LoadingRegisteredCompetitors, ex.Message), Strings.Error_Generic, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BtnAddCompetitor_Click(object sender, RoutedEventArgs e)
        {
            var registerWindow = new RegisterCompetitorWindow(_categoryInTournament.Id, _registrationService) { Owner = this };
            if (registerWindow.ShowDialog() == true)
            {
                var competitorToRegister = registerWindow.SelectedCompetitor;
                if (competitorToRegister != null)
                {
                    try
                    {
                        await _registrationService.RegisterCompetitorAsync(_categoryInTournament.Id, competitorToRegister.Id);
                        await LoadRegisteredCompetitorsAsync();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(string.Format(CultureInfo.CurrentCulture, Strings.Error_RegisteringCompetitor, ex.Message), Strings.Error_Generic, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private async void BtnRemoveCompetitor_Click(object sender, RoutedEventArgs e)
        {
            if (CompetitorsDataGrid.SelectedItem is CompetitorInCategoryInTournament selectedRegistration)
            {
                string message = string.Format(CultureInfo.CurrentCulture, Strings.Confirm_RemoveCompetitor_Text, $"{selectedRegistration.Competitor.FirstName} {selectedRegistration.Competitor.LastName}");
                if (MessageBox.Show(message, Strings.Confirm_Delete_Caption, MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    try
                    {
                        bool success = await _registrationService.UnregisterCompetitorAsync(_categoryInTournament.Id, selectedRegistration.CompetitorId);
                        if (success)
                        {
                            RegisteredCompetitors.Remove(selectedRegistration);
                        }
                        else
                        {
                            MessageBox.Show(Strings.Error_RemovingCompetitor_Failed, Strings.Error_Generic, MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(string.Format(CultureInfo.CurrentCulture, Strings.Error_RemovingCompetitor_Generic, ex.Message), Strings.Error_Generic, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private async void BtnUpdatePlacement_Click(object sender, RoutedEventArgs e)
        {
            if (CompetitorsDataGrid.SelectedItem is CompetitorInCategoryInTournament selectedRegistration)
            {
                if (_registrationService == null)
                {
                    MessageBox.Show("Servis za registraciju nije dostupan.", Strings.Error_Generic, MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var updatePlacementWindow = new UpdatePlacementWindow(selectedRegistration, _registrationService)
                {
                    Owner = this,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };

                if (updatePlacementWindow.ShowDialog() == true)
                {
                    await LoadRegisteredCompetitorsAsync();
                }
            }
            else
            {
                MessageBox.Show(Strings.Warning_NoCompetitorSelected, Strings.Warning_Generic_Caption, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CompetitorsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool hasSelection = CompetitorsDataGrid.SelectedItem != null;

            if (BtnRemoveCompetitor.Visibility == Visibility.Visible)
            {
                BtnRemoveCompetitor.IsEnabled = hasSelection;
            }
            if (BtnUpdatePlacement.Visibility == Visibility.Visible)
            {
                BtnUpdatePlacement.IsEnabled = hasSelection;
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed) DragMove();
        }
        protected virtual void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
