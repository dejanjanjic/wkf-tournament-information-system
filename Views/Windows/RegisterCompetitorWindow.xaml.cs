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
    public partial class RegisterCompetitorWindow : Window, INotifyPropertyChanged
    {
        private readonly int _categoryInTournamentId;
        private readonly ICompetitorRegistrationService _registrationService;
        private Competitor _selectedCompetitor;

        public ObservableCollection<Competitor> AvailableCompetitors { get; set; } = new ObservableCollection<Competitor>();
        public Competitor SelectedCompetitor
        {
            get => _selectedCompetitor;
            set { _selectedCompetitor = value; OnPropertyChanged(nameof(SelectedCompetitor)); OnPropertyChanged(nameof(IsValid)); }
        }

        public bool IsValid => SelectedCompetitor != null;
        public bool HasNoAvailableCompetitors => !AvailableCompetitors.Any();

        public event PropertyChangedEventHandler PropertyChanged;

        public RegisterCompetitorWindow(int categoryInTournamentId, ICompetitorRegistrationService registrationService)
        {
            InitializeComponent();
            _categoryInTournamentId = categoryInTournamentId;
            _registrationService = registrationService;
            DataContext = this;
            this.Title = Strings.RegisterCompetitor_WindowTitle; 
            this.Loaded += RegisterCompetitorWindow_Loaded;
        }

        private async void RegisterCompetitorWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadAvailableCompetitorsAsync();
        }

        private async Task LoadAvailableCompetitorsAsync()
        {
            try
            {
                var competitors = await _registrationService.GetAvailableCompetitorsForCategoryAsync(_categoryInTournamentId);
                AvailableCompetitors.Clear();
                if (competitors != null)
                {
                    foreach (var competitor in competitors)
                    {
                        AvailableCompetitors.Add(competitor);
                    }
                }
                OnPropertyChanged(nameof(HasNoAvailableCompetitors));
                if (HasNoAvailableCompetitors)
                {
                    SelectedCompetitor = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Strings.Error_LoadingAvailableCompetitors, ex.Message), Strings.Error_Generic, MessageBoxButton.OK, MessageBoxImage.Error); 
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
        protected virtual void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}