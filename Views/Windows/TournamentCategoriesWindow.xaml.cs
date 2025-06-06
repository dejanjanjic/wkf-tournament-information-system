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

namespace WKFTournamentIS.Views.Windows
{
    public partial class TournamentCategoriesWindow : Window, INotifyPropertyChanged
    {
        private readonly Tournament _currentTournament;
        private readonly ICategoryService _categoryService;
        private readonly ICategoryInTournamentService _categoryInTournamentService;
        private readonly ICompetitorRegistrationService _competitorRegistrationService;

        private readonly bool _isOperatorView;

        public ObservableCollection<Category> AssignedCategories { get; set; } = new ObservableCollection<Category>();

        private string _pageSubtitleText;
        public string PageSubtitle
        {
            get => _pageSubtitleText;
            set { _pageSubtitleText = value; OnPropertyChanged(nameof(PageSubtitle)); }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        public TournamentCategoriesWindow(Tournament tournament, ICategoryService categoryService, ICategoryInTournamentService categoryInTournamentService, ICompetitorRegistrationService competitorRegistrationService, bool isOperatorView = false)
        {
            InitializeComponent();
            _currentTournament = tournament;
            _categoryService = categoryService;
            _categoryInTournamentService = categoryInTournamentService;
            _competitorRegistrationService = competitorRegistrationService;
            _isOperatorView = isOperatorView;

            DataContext = this;
            this.Title = string.Format(Strings.TournamentCategories_WindowTitle, _currentTournament.Name); 
            UpdatePageSubtitle(); 
            this.Loaded += TournamentCategoriesWindow_Loaded;

            SetupOperatorView();
        }

        private void UpdatePageSubtitle()
        {
            var subtitleResource = _isOperatorView
                ? Strings.TournamentCategories_PageSubtitle_Operator
                : Strings.TournamentCategories_PageSubtitle;
            PageSubtitle = string.Format(subtitleResource, _currentTournament.Name);
        }

        private void SetupOperatorView()
        {
            if (_isOperatorView)
            {
                BtnAddCategory.Visibility = Visibility.Collapsed;
                BtnRemoveCategory.Visibility = Visibility.Collapsed;
            }
        }


        private async void TournamentCategoriesWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadAssignedCategoriesAsync();
        }

        private async Task LoadAssignedCategoriesAsync()
        {
            try
            {
                var categories = await _categoryService.GetCategoriesByTournamentIdAsync(_currentTournament.Id);
                AssignedCategories.Clear();
                if (categories != null)
                {
                    foreach (var category in categories)
                    {
                        AssignedCategories.Add(category);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Strings.Error_LoadingCategoriesForTournament, ex.Message), Strings.Error_Generic, MessageBoxButton.OK, MessageBoxImage.Error); 
            }
        }

        private async void BtnAddCategory_Click(object sender, RoutedEventArgs e)
        {
            var assignWindow = new AssignCategoryWindow(_currentTournament.Id, _categoryService) { Owner = this };
            if (assignWindow.ShowDialog() == true)
            {
                var categoryToAdd = assignWindow.SelectedCategory;
                if (categoryToAdd != null)
                {
                    try
                    {
                        await _categoryInTournamentService.AssignCategoryToTournamentAsync(_currentTournament.Id, categoryToAdd.Id);
                        await LoadAssignedCategoriesAsync();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(string.Format(Strings.Error_AddingCategoryToTournament, ex.Message), Strings.Error_Generic, MessageBoxButton.OK, MessageBoxImage.Error); 
                    }
                }
            }
        }

        private async void BtnRemoveCategory_Click(object sender, RoutedEventArgs e)
        {
            if (CategoriesDataGrid.SelectedItem is Category selectedCategory)
            {
                string message = string.Format(Strings.Confirm_RemoveCategoryFromTournament_Text, selectedCategory.Name); 
                if (MessageBox.Show(message, Strings.Confirm_Delete_Caption, MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes) 
                {
                    try
                    {
                        bool success = await _categoryInTournamentService.RemoveCategoryFromTournamentAsync(_currentTournament.Id, selectedCategory.Id);
                        if (success)
                        {
                            AssignedCategories.Remove(selectedCategory);
                        }
                        else
                        {
                            MessageBox.Show(Strings.Error_RemovingCategoryFromTournament_Failed, Strings.Error_Generic, MessageBoxButton.OK, MessageBoxImage.Error); 
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(string.Format(Strings.Error_RemovingCategoryFromTournament_Generic, ex.Message), Strings.Error_Generic, MessageBoxButton.OK, MessageBoxImage.Error); 
                    }
                }
            }
        }

        private void CategoriesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool isSelected = CategoriesDataGrid.SelectedItem != null;
            BtnRemoveCategory.IsEnabled = isSelected;
            BtnViewCategoryCompetitors.IsEnabled = isSelected;
        }

        private async void BtnViewCategoryCompetitors_Click(object sender, RoutedEventArgs e)
        {
            if (CategoriesDataGrid.SelectedItem is Category selectedCategory)
            {
                var categoryInTournament = await _categoryInTournamentService.GetCategoryInTournamentByTournamentAndCategoryAsync(_currentTournament.Id, selectedCategory.Id);

                if (categoryInTournament == null)
                {
                    MessageBox.Show(Strings.Error_CategoryTournamentLinkNotFound, Strings.Error_Generic, MessageBoxButton.OK, MessageBoxImage.Error); 
                    return;
                }

                var competitorsWindow = new CategoryCompetitorsWindow(categoryInTournament, _competitorRegistrationService, _isOperatorView)
                {
                    Owner = this
                };

                competitorsWindow.ShowDialog();
            }
        }

        private void CategoriesDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (BtnViewCategoryCompetitors.IsEnabled)
            {
                BtnViewCategoryCompetitors_Click(sender, e);
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