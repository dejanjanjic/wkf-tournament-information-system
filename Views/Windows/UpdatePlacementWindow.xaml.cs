using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WKFTournamentIS.Core.Models;
using WKFTournamentIS.Services.Interfaces;
using WKFTournamentIS.Resources;

namespace WKFTournamentIS.Views.Windows
{
    public partial class UpdatePlacementWindow : Window, INotifyPropertyChanged
    {
        private readonly CompetitorInCategoryInTournament _originalRegistration;
        private readonly ICompetitorRegistrationService _registrationService;

        private int? _placement;
        private string _placementError;

        public string CompetitorFullName { get; }
        public string CompetitorClubName { get; }

        public int? Placement
        {
            get => _placement;
            set
            {
                if (_placement != value)
                {
                    _placement = value;
                    OnPropertyChanged();
                    ValidatePlacement();
                }
            }
        }

        public string PlacementError
        {
            get => _placementError;
            set
            {
                if (_placementError != value)
                {
                    _placementError = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(HasPlacementError));
                    OnPropertyChanged(nameof(IsFormValid));
                }
            }
        }

        public bool HasPlacementError => !string.IsNullOrEmpty(PlacementError);
        public bool IsFormValid => !HasPlacementError;

        public UpdatePlacementWindow(CompetitorInCategoryInTournament registration, ICompetitorRegistrationService registrationService)
        {
            InitializeComponent();
            DataContext = this;

            _originalRegistration = registration ?? throw new ArgumentNullException(nameof(registration));
            _registrationService = registrationService ?? throw new ArgumentNullException(nameof(registrationService));

            if (registration.Competitor != null)
            {
                CompetitorFullName = $"{registration.Competitor.FirstName} {registration.Competitor.LastName}";
                CompetitorClubName = registration.Competitor.Club?.Name ?? "N/A";
            }
            else
            {
                CompetitorFullName = "N/A";
                CompetitorClubName = "N/A";
            }

            Placement = registration.Placement;

            ValidatePlacement();
        }

        private void ValidatePlacement()
        {
            if (Placement.HasValue && Placement.Value <= 0)
            {
                PlacementError = Strings.Validation_PlacementMustBePositive;
            }
            else
            {
                PlacementError = null;
            }
        }

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            ValidatePlacement();
            if (!IsFormValid)
            {
                MessageBox.Show(Strings.Validation_FixErrorsBeforeSaving,
                                Strings.Validation_ErrorCaption,
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                bool success = await _registrationService.UpdatePlacementAsync(_originalRegistration.Id, Placement);
                if (success)
                {
                    MessageBox.Show(Strings.MessageBox_UpdatePlacementSuccessText,
                                    Strings.MessageBox_UpdatePlacementSuccessTitle,
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                    Close();
                }
                else
                {
                    string errorMessage = string.Format(CultureInfo.CurrentCulture, Strings.MessageBox_UpdatePlacementErrorText, "Operation returned false.");
                    MessageBox.Show(errorMessage,
                                    Strings.MessageBox_UpdatePlacementErrorTitle,
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                string errorMessage = string.Format(CultureInfo.CurrentCulture, Strings.MessageBox_UpdatePlacementErrorText, ex.Message);
                MessageBox.Show(errorMessage,
                                Strings.MessageBox_UpdatePlacementErrorTitle,
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}