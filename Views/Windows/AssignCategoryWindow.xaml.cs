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
    public partial class AssignCategoryWindow : Window, INotifyPropertyChanged
    {
        private readonly int _tournamentId;
        private readonly ICategoryService _categoryService;
        private Category _selectedCategory;

        public ObservableCollection<Category> AvailableCategories { get; set; } = new ObservableCollection<Category>();
        public Category SelectedCategory
        {
            get => _selectedCategory;
            set { _selectedCategory = value; OnPropertyChanged(nameof(SelectedCategory)); OnPropertyChanged(nameof(IsValid)); }
        }

        public bool IsValid => SelectedCategory != null;
        public bool HasNoAvailableCategories => !AvailableCategories.Any();

        public event PropertyChangedEventHandler PropertyChanged;

        public AssignCategoryWindow(int tournamentId, ICategoryService categoryService)
        {
            InitializeComponent();
            _tournamentId = tournamentId;
            _categoryService = categoryService;
            DataContext = this;
            this.Title = Strings.AssignCategory_WindowTitle; 
            this.Loaded += AssignCategoryWindow_Loaded;
        }

        private async void AssignCategoryWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadAvailableCategoriesAsync();
        }

        private async Task LoadAvailableCategoriesAsync()
        {
            try
            {
                var categories = await _categoryService.GetCategoriesNotAssignedToTournamentAsync(_tournamentId);
                AvailableCategories.Clear();
                if (categories != null)
                {
                    foreach (var category in categories)
                    {
                        AvailableCategories.Add(category);
                    }
                }
                OnPropertyChanged(nameof(HasNoAvailableCategories));

                if (HasNoAvailableCategories)
                {
                    SelectedCategory = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Strings.Error_LoadingAvailableCategories, ex.Message), Strings.Error_Generic, MessageBoxButton.OK, MessageBoxImage.Error); 
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (IsValid)
            {
                DialogResult = true;
                Close();
            }
        }
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
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