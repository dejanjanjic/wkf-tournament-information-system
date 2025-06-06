using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WKFTournamentIS.Core.Models;
using WKFTournamentIS.Services.Interfaces;
using WKFTournamentIS.Resources; 

namespace WKFTournamentIS.Views.Windows
{
    public partial class AddEditCompetitorWindow : Window, INotifyPropertyChanged
    {
        private string _firstName;
        private string _lastName;
        private DateTime _dateOfBirth = DateTime.Now.AddYears(-10);
        private string _firstNameError;
        private string _lastNameError;
        private string _dateOfBirthError;

        private readonly IClubService _clubService;
        private Club _selectedClub;
        private string _clubError;

        private Competitor _originalCompetitor;

        public Competitor Competitor { get; private set; }
        public bool IsEditMode { get; private set; }

        public string FirstName
        {
            get => _firstName;
            set { _firstName = value; OnPropertyChanged(nameof(FirstName)); ValidateFirstName(); }
        }

        public string LastName
        {
            get => _lastName;
            set { _lastName = value; OnPropertyChanged(nameof(LastName)); ValidateLastName(); }
        }

        public DateTime DateOfBirth
        {
            get => _dateOfBirth;
            set
            {
                if (_dateOfBirth != value)
                {
                    _dateOfBirth = value;
                    OnPropertyChanged(nameof(DateOfBirth));
                    ValidateDateOfBirth();
                }
            }
        }

        public ObservableCollection<Club> Clubs { get; set; } = new ObservableCollection<Club>();
        public Club SelectedClub
        {
            get => _selectedClub;
            set { _selectedClub = value; OnPropertyChanged(nameof(SelectedClub)); ValidateClub(); }
        }

        public string FirstNameError
        {
            get => _firstNameError;
            set { _firstNameError = value; OnPropertyChanged(nameof(FirstNameError)); OnPropertyChanged(nameof(HasFirstNameError)); OnPropertyChanged(nameof(IsFormValid)); }
        }

        public string LastNameError
        {
            get => _lastNameError;
            set { _lastNameError = value; OnPropertyChanged(nameof(LastNameError)); OnPropertyChanged(nameof(HasLastNameError)); OnPropertyChanged(nameof(IsFormValid)); }
        }

        public string DateOfBirthError
        {
            get => _dateOfBirthError;
            set { _dateOfBirthError = value; OnPropertyChanged(nameof(DateOfBirthError)); OnPropertyChanged(nameof(HasDateOfBirthError)); OnPropertyChanged(nameof(IsFormValid)); }
        }

        public string ClubError
        {
            get => _clubError;
            set { _clubError = value; OnPropertyChanged(nameof(ClubError)); OnPropertyChanged(nameof(HasClubError)); OnPropertyChanged(nameof(IsFormValid)); }
        }

        public bool HasFirstNameError => !string.IsNullOrEmpty(FirstNameError);
        public bool HasLastNameError => !string.IsNullOrEmpty(LastNameError);
        public bool HasDateOfBirthError => !string.IsNullOrEmpty(DateOfBirthError);
        public bool HasClubError => !string.IsNullOrEmpty(ClubError);


        public bool IsFormValid => !HasFirstNameError && !HasLastNameError && !HasDateOfBirthError && !HasClubError &&
                                     !string.IsNullOrWhiteSpace(FirstName) &&
                                     !string.IsNullOrWhiteSpace(LastName) &&
                                     SelectedClub != null &&
                                     DateOfBirth != default(DateTime);

        public AddEditCompetitorWindow()
        {
            InitializeComponent();
            _clubService = App.ServiceProvider.GetRequiredService<IClubService>();
            DataContext = this;
            this.Title = Strings.AddEditCompetitor_Title_Add; 

            this.Loaded += AddEditCompetitorWindow_Loaded;

            ValidateAll();
        }

        public AddEditCompetitorWindow(Competitor competitorToEdit) : this()
        {
            IsEditMode = true;
            this.Title = string.Format(Strings.AddEditCompetitor_Title_Edit, competitorToEdit.FirstName, competitorToEdit.LastName); 
            _originalCompetitor = competitorToEdit;
            LoadCompetitorData(competitorToEdit);
        }

        private async void AddEditCompetitorWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadClubsAsync();

            if (IsEditMode && _originalCompetitor != null && _originalCompetitor.ClubId.HasValue)
            {
                SelectedClub = Clubs.FirstOrDefault(c => c.Id == _originalCompetitor.ClubId.Value);
            }
        }

        private async Task LoadClubsAsync()
        {
            try
            {
                var clubsList = await _clubService.GetAllClubsAsync();
                Clubs.Clear();
                if (clubsList != null)
                {
                    foreach (var club in clubsList.OrderBy(c => c.Name))
                    {
                        Clubs.Add(club);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Strings.Error_LoadingClubs_MessageBox, ex.Message), Strings.Error_Generic, MessageBoxButton.OK, MessageBoxImage.Error); 
            }
        }

        private void LoadCompetitorData(Competitor competitor)
        {
            FirstName = competitor.FirstName;
            LastName = competitor.LastName;
            DateOfBirth = competitor.DateOfBirth;
        }

        private void ValidateAll()
        {
            ValidateFirstName();
            ValidateLastName();
            ValidateDateOfBirth();
            ValidateClub();
        }

        private void ValidateFirstName()
        {
            if (string.IsNullOrWhiteSpace(FirstName))
                FirstNameError = Strings.Validation_FirstNameRequired; 
            else if (FirstName.Length > 50)
                FirstNameError = Strings.Validation_FirstNameMaxLength; 
            else
                FirstNameError = null;
        }

        private void ValidateLastName()
        {
            if (string.IsNullOrWhiteSpace(LastName))
                LastNameError = Strings.Validation_LastNameRequired; 
            else if (LastName.Length > 100)
                LastNameError = Strings.Validation_LastNameMaxLength; 
            else
                LastNameError = null;
        }

        private void ValidateDateOfBirth()
        {
            if (DateOfBirth == default(DateTime) && DpDateOfBirth.SelectedDate == null)
            {
                DateOfBirthError = Strings.Validation_DateOfBirthRequired; 
            }
            else if (DateOfBirth > DateTime.Now)
                DateOfBirthError = Strings.Validation_DateOfBirthInvalidFuture; 
            else if (DateOfBirth > DateTime.Now.AddYears(-3))
                DateOfBirthError = Strings.Validation_DateOfBirthMinAge; 
            else
                DateOfBirthError = null;
        }

        private void ValidateClub()
        {
            if (SelectedClub == null)
                ClubError = Strings.Validation_ClubRequired; 
            else
                ClubError = null;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            ValidateAll();
            if (!IsFormValid)
            {
                MessageBox.Show(Strings.Validation_FixErrorsBeforeSaving, Strings.Validation_ErrorCaption,
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Competitor = new Competitor
            {
                FirstName = FirstName.Trim(),
                LastName = LastName.Trim(),
                DateOfBirth = DateOfBirth.Date,
                ClubId = SelectedClub.Id
            };

            if (IsEditMode && _originalCompetitor != null)
            {
                Competitor.Id = _originalCompetitor.Id;
            }

            DialogResult = true;
            Close();
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
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}