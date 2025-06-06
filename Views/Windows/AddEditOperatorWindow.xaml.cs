using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using WKFTournamentIS.Core.Enums;
using WKFTournamentIS.Core.Models;
using WKFTournamentIS.Resources; 

namespace WKFTournamentIS.Views.Windows
{
    public partial class AddEditOperatorWindow : Window, INotifyPropertyChanged
    {
        private string _username;
        private string _password;
        private string _confirmPassword;
        private string _usernameError;
        private string _passwordError;
        private string _confirmPasswordError;

        private User _originalOperator;

        public User Operator { get; private set; }
        public bool IsEditMode { get; set; }

        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(nameof(Username)); ValidateUsername(); }
        }

        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(nameof(Password)); ValidatePassword(); ValidateConfirmPassword(); }
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set { _confirmPassword = value; OnPropertyChanged(nameof(ConfirmPassword)); ValidateConfirmPassword(); }
        }

        public string UsernameError
        {
            get => _usernameError;
            set
            {
                _usernameError = value;
                OnPropertyChanged(nameof(UsernameError));
                OnPropertyChanged(nameof(HasUsernameError));
                OnPropertyChanged(nameof(IsFormValid));
            }
        }

        public string PasswordError
        {
            get => _passwordError;
            set
            {
                _passwordError = value;
                OnPropertyChanged(nameof(PasswordError));
                OnPropertyChanged(nameof(HasPasswordError));
                OnPropertyChanged(nameof(IsFormValid));
            }
        }

        public string ConfirmPasswordError
        {
            get => _confirmPasswordError;
            set
            {
                _confirmPasswordError = value;
                OnPropertyChanged(nameof(ConfirmPasswordError));
                OnPropertyChanged(nameof(HasConfirmPasswordError));
                OnPropertyChanged(nameof(IsFormValid));
            }
        }

        public bool HasUsernameError => !string.IsNullOrEmpty(UsernameError);
        public bool HasPasswordError => !string.IsNullOrEmpty(PasswordError);
        public bool HasConfirmPasswordError => !string.IsNullOrEmpty(ConfirmPasswordError);

        public bool IsFormValid => !HasUsernameError && !HasPasswordError && !HasConfirmPasswordError &&
                                     !string.IsNullOrWhiteSpace(Username) &&
                                     !string.IsNullOrWhiteSpace(Password) &&
                                     !string.IsNullOrWhiteSpace(ConfirmPassword);

        public AddEditOperatorWindow()
        {
            InitializeComponent();
            DataContext = this;
            this.Title = Strings.AddEditOperator_Title_Add; 
            ValidateAll();
        }

        public AddEditOperatorWindow(User operatorToEdit) : this()
        {
            IsEditMode = true;
            this.Title = string.Format(Strings.AddEditOperator_Title_Edit, operatorToEdit.Username); 
            _originalOperator = operatorToEdit;
            LoadOperatorData(operatorToEdit);
        }

        private void LoadOperatorData(User operatorUser)
        {
            Username = operatorUser.Username;
            TxtUsername.IsReadOnly = true;
            ValidateAll();
        }

        private void ValidateAll()
        {
            ValidateUsername();
            ValidatePassword();
            ValidateConfirmPassword();
        }

        private void ValidateUsername()
        {
            if (string.IsNullOrWhiteSpace(Username))
                UsernameError = Strings.Validation_UsernameRequired; 
            else if (Username.Length < 3)
                UsernameError = Strings.Validation_UsernameMinLength; 
            else if (Username.Length > 50)
                UsernameError = Strings.Validation_UsernameMaxLength; 
            else
                UsernameError = null;
        }

        private void ValidatePassword()
        {
            if (string.IsNullOrWhiteSpace(Password))
                PasswordError = Strings.Validation_PasswordRequired; 
            else if (Password.Length < 6)
                PasswordError = Strings.Validation_PasswordMinLength; 
            else if (Password.Length > 100)
                PasswordError = Strings.Validation_PasswordMaxLength; 
            else
                PasswordError = null;
        }

        private void ValidateConfirmPassword()
        {
            if (string.IsNullOrWhiteSpace(ConfirmPassword))
                ConfirmPasswordError = Strings.Validation_ConfirmPasswordRequired; 
            else if (Password != ConfirmPassword)
                ConfirmPasswordError = Strings.Validation_PasswordsDoNotMatch; 
            else
                ConfirmPasswordError = null;
        }

        private void TxtPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            Password = TxtPassword.Password;
        }

        private void TxtConfirmPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            ConfirmPassword = TxtConfirmPassword.Password;
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

            Operator = new User
            {
                Username = Username.Trim(),
                PasswordHash = Password,
                Role = UserRole.Operator
            };

            if (IsEditMode && _originalOperator != null)
            {
                Operator.Id = _originalOperator.Id;
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