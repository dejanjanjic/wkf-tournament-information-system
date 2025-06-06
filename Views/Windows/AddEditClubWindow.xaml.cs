using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using WKFTournamentIS.Core.Models;
using WKFTournamentIS.Resources; 

namespace WKFTournamentIS.Views.Windows
{
    public partial class AddEditClubWindow : Window, INotifyPropertyChanged
    {
        private string _clubName;
        private string _nameError;
        private readonly Club _originalClub;

        public event PropertyChangedEventHandler PropertyChanged;

        public Club Club { get; private set; }
        public bool IsEditMode { get; }

        public string ClubName
        {
            get => _clubName;
            set { _clubName = value; OnPropertyChanged(nameof(ClubName)); ValidateName(); }
        }

        public string NameError
        {
            get => _nameError;
            set { _nameError = value; OnPropertyChanged(nameof(NameError)); OnPropertyChanged(nameof(HasNameError)); OnPropertyChanged(nameof(IsFormValid)); }
        }

        public bool HasNameError => !string.IsNullOrEmpty(NameError);
        public bool IsFormValid => !HasNameError && !string.IsNullOrWhiteSpace(ClubName);

        public AddEditClubWindow()
        {
            InitializeComponent();
            DataContext = this;
            this.Title = Strings.AddEditClub_Title_Add; 
            ValidateName();
        }

        public AddEditClubWindow(Club club) : this()
        {
            IsEditMode = true;
            _originalClub = club;
            ClubName = club.Name;
            this.Title = Strings.AddEditClub_Title_Edit; 
        }

        private void ValidateName()
        {
            if (string.IsNullOrWhiteSpace(ClubName))
            {
                NameError = Strings.Validation_ClubNameRequired; 
            }
            else if (ClubName.Length > 200)
            {
                NameError = Strings.Validation_ClubNameMaxLength; 
            }
            else
            {
                NameError = null;
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            ValidateName();
            if (!IsFormValid)
            {
                MessageBox.Show(Strings.Validation_FixErrorsInForm, Strings.Validation_ErrorCaption, MessageBoxButton.OK, MessageBoxImage.Warning); 
                return;
            }

            Club = new Club
            {
                Name = ClubName.Trim()
            };

            if (IsEditMode)
            {
                Club.Id = _originalClub.Id;
            }

            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed) DragMove();
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}