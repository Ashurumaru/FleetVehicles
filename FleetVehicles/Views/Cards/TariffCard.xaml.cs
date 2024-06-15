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
    /// Логика взаимодействия для TariffCard.xaml
    /// </summary>
    public partial class TariffCard : Window
    {
        private readonly FleetVehiclesEntities _context;
        private Tariff _currentTariff;
        private bool _isNewTariff;

        public TariffCard()
        {
            InitializeComponent();
            _context = new FleetVehiclesEntities();
            LoadData();
        }

        private void LoadData(string searchQuery = "")
        {
            var tariffsQuery = _context.Tariff.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                searchQuery = searchQuery.ToLower();
                tariffsQuery = tariffsQuery.Where(t => t.Name.ToLower().Contains(searchQuery));
            }

            var tariffs = tariffsQuery.ToList();

            TariffList.ItemsSource = tariffs;
        }

        private void SearchTariffTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadData(SearchTariffTextBox.Text);
        }

        private void ResetTariffSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchTariffTextBox.Text = string.Empty;
            LoadData();
        }

        private void CreateTariff_Click(object sender, RoutedEventArgs e)
        {
            _isNewTariff = true;
            _currentTariff = new Tariff();
            ShowTariffForm();
        }

        private void EditTariff_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                int tariffId = (int)button.CommandParameter;
                _currentTariff = _context.Tariff.SingleOrDefault(t => t.IdTariff == tariffId);
                if (_currentTariff != null)
                {
                    _isNewTariff = false;
                    ShowTariffForm();
                }
            }
        }
        private void DeleteTariff_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                int tariffId = (int)button.CommandParameter;
                var tariff = _context.Tariff.SingleOrDefault(t => t.IdTariff == tariffId);
                if (tariff != null)
                {
                    var relatedOrders = _context.Orders.Any(o => o.IdTariff == tariffId);
                    if (relatedOrders)
                    {
                        MessageBox.Show("Невозможно удалить тариф, так как он связан с заказами.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var result = MessageBox.Show("Вы уверены, что хотите удалить тариф?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (result == MessageBoxResult.Yes)
                    {
                        _context.Tariff.Remove(tariff);
                        _context.SaveChanges();
                        MessageBox.Show("Тариф успешно удален.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadData();
                    }
                }
            }
        }
        private void ShowTariffForm()
        {
            TariffList.Visibility = Visibility.Collapsed;
            TariffForm.Visibility = Visibility.Visible;

            NameTextBox.Text = _currentTariff.Name;
            CostTextBox.Text = _currentTariff.Cost.ToString();
        }

        private void HideTariffForm()
        {
            TariffList.Visibility = Visibility.Visible;
            TariffForm.Visibility = Visibility.Collapsed;
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

            _currentTariff.Name = NameTextBox.Text;
            _currentTariff.Cost = (int?)cost;

            if (_isNewTariff)
            {
                _context.Tariff.Add(_currentTariff);
            }

            _context.SaveChanges();
            MessageBox.Show("Тариф успешно сохранен.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            HideTariffForm();
            LoadData();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            HideTariffForm();
        }

        private void CostTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^[0-9]*\.?[0-9]+$");
        }
    }
}