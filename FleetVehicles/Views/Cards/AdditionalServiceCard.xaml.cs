using FleetVehicles.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FleetVehicles.Views.Cards
{
    /// <summary>
    /// Логика взаимодействия для AdditionalServiceCard.xaml
    /// </summary>
    public partial class AdditionalServiceCard : Window
    {
        private readonly FleetVehiclesEntities _context;
        private AdditionalService _currentAdditionalService;
        private bool _isNewAdditionalService;

        public AdditionalServiceCard()
        {
            InitializeComponent();
            _context = new FleetVehiclesEntities();
            LoadData();
        }

        private void LoadData(string searchQuery = "")
        {
            var additionalServicesQuery = _context.AdditionalService.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                searchQuery = searchQuery.ToLower();
                additionalServicesQuery = additionalServicesQuery.Where(t => t.Name.ToLower().Contains(searchQuery));
            }

            var additionalServices = additionalServicesQuery.ToList();

            AdditionalServiceList.ItemsSource = additionalServices;
        }

        private void SearchAdditionalServiceTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadData(SearchAdditionalServiceTextBox.Text);
        }

        private void ResetAdditionalServiceSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchAdditionalServiceTextBox.Text = string.Empty;
            LoadData();
        }

        private void CreateAdditionalService_Click(object sender, RoutedEventArgs e)
        {
            _isNewAdditionalService = true;
            _currentAdditionalService = new AdditionalService();
            ShowAdditionalServiceForm();
        }

        private void EditAdditionalService_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                int additionalServiceId = (int)button.CommandParameter;
                _currentAdditionalService = _context.AdditionalService.SingleOrDefault(t => t.IdAdditionalService == additionalServiceId);
                if (_currentAdditionalService != null)
                {
                    _isNewAdditionalService = false;
                    ShowAdditionalServiceForm();
                }
            }
        }

        private void DeleteAdditionalService_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                int additionalServiceId = (int)button.CommandParameter;
                var additionalService = _context.AdditionalService.SingleOrDefault(t => t.IdAdditionalService == additionalServiceId);
                if (additionalService != null)
                {
                    var relatedOrderAdditionalServices = _context.OrderAdditionalService.Any(oas => oas.IdAdditionalService == additionalServiceId);
                    if (relatedOrderAdditionalServices)
                    {
                        MessageBox.Show("Невозможно удалить услугу, так как она связана с заказами.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var result = MessageBox.Show("Вы уверены, что хотите удалить услугу?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (result == MessageBoxResult.Yes)
                    {
                        _context.AdditionalService.Remove(additionalService);
                        _context.SaveChanges();
                        MessageBox.Show("Услуга успешно удалена.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadData();
                    }
                }
            }
        }

        private void ShowAdditionalServiceForm()
        {
            AdditionalServiceList.Visibility = Visibility.Collapsed;
            AdditionalServiceForm.Visibility = Visibility.Visible;

            NameTextBox.Text = _currentAdditionalService.Name;
            CostTextBox.Text = _currentAdditionalService.Cost.ToString();
        }

        private void HideAdditionalServiceForm()
        {
            AdditionalServiceList.Visibility = Visibility.Visible;
            AdditionalServiceForm.Visibility = Visibility.Collapsed;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text) || string.IsNullOrWhiteSpace(CostTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, заполните все обязательные поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!decimal.TryParse(CostTextBox.Text, out decimal cost))
            {
                MessageBox.Show("Пожалуйста, введите корректную стоимость.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _currentAdditionalService.Name = NameTextBox.Text;
            _currentAdditionalService.Cost = (int?)cost;

            if (_isNewAdditionalService)
            {
                _context.AdditionalService.Add(_currentAdditionalService);
            }

            _context.SaveChanges();
            MessageBox.Show("Услуга успешно сохранена.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            HideAdditionalServiceForm();
            LoadData();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            HideAdditionalServiceForm();
        }

        private void CostTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^[0-9]*\.?[0-9]+$");
        }
    }
}