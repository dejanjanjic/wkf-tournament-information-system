using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using WKFTournamentIS.Core.Models;
using WKFTournamentIS.Resources; 

namespace WKFTournamentIS.Views.Windows
{
    public partial class AddTournamentWindow : Window, INotifyPropertyChanged
    {
        private string _tournamentName;
        private DateTime _beginningDate = DateTime.Now.AddDays(30);
        private string _beginningHour = "09";
        private string _beginningMinute = "00";
        private DateTime _endingDate = DateTime.Now.AddDays(32);
        private string _endingHour = "18";
        private string _endingMinute = "00";
        private string _location;
        private string _nameError;
        private string _locationError;
        private string _dateError;
        private Tournament _originalTournament;

        public Tournament Tournament { get; private set; }
        public bool IsEditMode { get; set; }

        public string TournamentName
        {
            get => _tournamentName;
            set { _tournamentName = value; OnPropertyChanged(nameof(TournamentName)); ValidateName(); }
        }

        public DateTime BeginningDate
        {
            get => _beginningDate;
            set { _beginningDate = value; OnPropertyChanged(nameof(BeginningDate)); ValidateDates(); }
        }

        public string BeginningHour
        {
            get => _beginningHour;
            set { _beginningHour = value; OnPropertyChanged(nameof(BeginningHour)); ValidateDates(); }
        }

        public string BeginningMinute
        {
            get => _beginningMinute;
            set { _beginningMinute = value; OnPropertyChanged(nameof(BeginningMinute)); ValidateDates(); }
        }

        public DateTime EndingDate
        {
            get => _endingDate;
            set { _endingDate = value; OnPropertyChanged(nameof(EndingDate)); ValidateDates(); }
        }

        public string EndingHour
        {
            get => _endingHour;
            set { _endingHour = value; OnPropertyChanged(nameof(EndingHour)); ValidateDates(); }
        }

        public string EndingMinute
        {
            get => _endingMinute;
            set { _endingMinute = value; OnPropertyChanged(nameof(EndingMinute)); ValidateDates(); }
        }
        public DateTime BeginningDateTime
        {
            get
            {
                int.TryParse(BeginningHour, out int hour);
                int.TryParse(BeginningMinute, out int minute);
                return BeginningDate.Date.AddHours(hour).AddMinutes(minute);
            }
        }
        public DateTime EndingDateTime
        {
            get
            {
                int.TryParse(EndingHour, out int hour);
                int.TryParse(EndingMinute, out int minute);
                return EndingDate.Date.AddHours(hour).AddMinutes(minute);
            }
        }

        public string Location
        {
            get => _location;
            set { _location = value; OnPropertyChanged(nameof(Location)); ValidateLocation(); }
        }

        public string NameError
        {
            get => _nameError;
            set
            {
                _nameError = value;
                OnPropertyChanged(nameof(NameError));
                OnPropertyChanged(nameof(HasNameError));
                OnPropertyChanged(nameof(IsFormValid));
            }
        }

        public string LocationError
        {
            get => _locationError;
            set
            {
                _locationError = value;
                OnPropertyChanged(nameof(LocationError));
                OnPropertyChanged(nameof(HasLocationError));
                OnPropertyChanged(nameof(IsFormValid));
            }
        }

        public string DateError
        {
            get => _dateError;
            set
            {
                _dateError = value;
                OnPropertyChanged(nameof(DateError));
                OnPropertyChanged(nameof(HasDateError));
                OnPropertyChanged(nameof(IsFormValid));
            }
        }

        public bool HasNameError => !string.IsNullOrEmpty(NameError);
        public bool HasLocationError => !string.IsNullOrEmpty(LocationError);
        public bool HasDateError => !string.IsNullOrEmpty(DateError);

        public bool IsFormValid => !HasNameError && !HasLocationError && !HasDateError &&
                                     !string.IsNullOrWhiteSpace(TournamentName) &&
                                     !string.IsNullOrWhiteSpace(Location);

        public AddTournamentWindow()
        {
            InitializeComponent();
            DataContext = this;
            this.Title = Strings.AddTournament_Title_Add; 
            ValidateAll();
        }

        public AddTournamentWindow(Tournament tournament) : this()
        {
            IsEditMode = true;
            this.Title = Strings.AddTournament_Title_Edit; 
            _originalTournament = tournament;
            LoadTournamentData(tournament);
        }

        private void LoadTournamentData(Tournament tournament)
        {
            TournamentName = tournament.Name;
            BeginningDate = tournament.BeginningDateTime.Date;
            BeginningHour = tournament.BeginningDateTime.Hour.ToString("D2");
            BeginningMinute = tournament.BeginningDateTime.Minute.ToString("D2");
            EndingDate = tournament.EndingDateTime.Date;
            EndingHour = tournament.EndingDateTime.Hour.ToString("D2");
            EndingMinute = tournament.EndingDateTime.Minute.ToString("D2");
            Location = tournament.Location;
            ValidateAll();
        }

        private void ValidateAll()
        {
            ValidateName();
            ValidateLocation();
            ValidateDates();
        }

        private void ValidateName()
        {
            if (string.IsNullOrWhiteSpace(TournamentName))
                NameError = Strings.Validation_TournamentNameRequired; 
            else if (TournamentName.Length > 500)
                NameError = Strings.Validation_TournamentNameMaxLength; 
            else
                NameError = null;
        }

        private void ValidateLocation()
        {
            if (string.IsNullOrWhiteSpace(Location))
                LocationError = Strings.Validation_LocationRequired; 
            else
                LocationError = null;
        }

        private void ValidateDates()
        {
            var beginningDateTime = BeginningDateTime;
            var endingDateTime = EndingDateTime;

            if (!IsEditMode && beginningDateTime < DateTime.Now)
            {
                DateError = Strings.Validation_BeginningDateInPast; 
            }
            else if (endingDateTime <= beginningDateTime)
            {
                DateError = Strings.Validation_EndingDateBeforeBeginning; 
            }
            else
            {
                DateError = null;
            }
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

            Tournament = new Tournament
            {
                Name = TournamentName.Trim(),
                BeginningDateTime = BeginningDateTime,
                EndingDateTime = EndingDateTime,
                Location = Location.Trim()
            };

            if (IsEditMode && _originalTournament != null)
            {
                Tournament.Id = _originalTournament.Id;
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