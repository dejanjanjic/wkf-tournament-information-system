using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using WKFTournamentIS.Core.Models;
using WKFTournamentIS.Resources;

namespace WKFTournamentIS.Views.Windows
{
    public partial class AddEditCategoryWindow : Window, INotifyPropertyChanged
    {
        private string _categoryName;
        private string _nameError;
        private Category _originalCategory;

        public Category Category { get; private set; }
        public bool IsEditMode { get; private set; }

        public string CategoryName
        {
            get => _categoryName;
            set { _categoryName = value; OnPropertyChanged(nameof(CategoryName)); ValidateName(); }
        }

        public string NameError
        {
            get => _nameError;
            set { _nameError = value; OnPropertyChanged(nameof(NameError)); OnPropertyChanged(nameof(HasNameError)); OnPropertyChanged(nameof(IsFormValid)); }
        }

        public bool HasNameError => !string.IsNullOrEmpty(NameError);
        public bool IsFormValid => !HasNameError && !string.IsNullOrWhiteSpace(CategoryName);

        public AddEditCategoryWindow()
        {
            InitializeComponent();
            DataContext = this;
            this.Title = Strings.AddEditCategory_Title_Add;
            ValidateName();
        }

        public AddEditCategoryWindow(Category categoryToEdit) : this()
        {
            IsEditMode = true;
            this.Title = Strings.AddEditCategory_Title_Edit;
            _originalCategory = categoryToEdit;
            CategoryName = categoryToEdit.Name;
        }

        private void ValidateName()
        {
            if (string.IsNullOrWhiteSpace(CategoryName))
                NameError = Strings.Validation_CategoryNameRequired;
            else if (CategoryName.Length > 500)
                NameError = Strings.Validation_CategoryNameMaxLength;
            else
                NameError = null;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            ValidateName();
            if (!IsFormValid)
            {
                MessageBox.Show(Strings.Validation_FixErrorsBeforeSaving, Strings.Validation_ErrorCaption,
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Category = new Category
            {
                Name = CategoryName.Trim()
            };

            if (IsEditMode && _originalCategory != null)
            {
                Category.Id = _originalCategory.Id;
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