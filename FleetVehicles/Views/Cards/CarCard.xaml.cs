using FleetVehicles.Data;
using FleetVehicles.Models;
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
    /// Логика взаимодействия для CarCard.xaml
    /// </summary>
    public partial class CarCard : Window
    {
        private readonly FleetVehiclesEntities _context;
        private Cars _currentCar;
        private bool _isNewCar;

        public CarCard()
        {
            InitializeComponent();
            _context = new FleetVehiclesEntities();
            LoadData();
        }

        private void LoadData(string searchQuery = "")
        {
            var carsQuery = _context.Cars.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                searchQuery = searchQuery.ToLower();
                carsQuery = carsQuery.Where(c => c.CarModel.Name.ToLower().Contains(searchQuery) ||
                                                 c.BodyType.Name.ToLower().Contains(searchQuery));
            }

            var cars = carsQuery.Select(c => new CarView
            {
                IdCar = c.IdCar,
                ModelName = c.CarModel.Name,
                BodyName = c.BodyType.Name,
                NumberOfSeats = c.NumberOfSeats,
                LoadCapacity = c.LoadCapacity,
                CarInfo = c.CarModel.Name + " - " + c.BodyType.Name + " - " + c.NumberOfSeats + " мест - " + c.LoadCapacity + " кг"
            }).ToList();

            CarList.ItemsSource = cars;
        }

        private void SearchCarTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadData(SearchCarTextBox.Text);
        }

        private void ResetCarSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchCarTextBox.Text = string.Empty;
            LoadData();
        }

        private void CreateCar_Click(object sender, RoutedEventArgs e)
        {
            _isNewCar = true;
            _currentCar = new Cars();
            ShowCarForm();
        }

        private void EditCar_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                int carId = (int)button.CommandParameter;
                _currentCar = _context.Cars.SingleOrDefault(c => c.IdCar == carId);
                if (_currentCar != null)
                {
                    _isNewCar = false;
                    ShowCarForm();
                }
            }
        }

        private void DeleteCar_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                int carId = (int)button.CommandParameter;
                var car = _context.Cars.SingleOrDefault(c => c.IdCar == carId);
                if (car != null)
                {
                    var relatedFleetCars = _context.FleetCars.Any(fc => fc.IdCar == carId);
                    if (relatedFleetCars)
                    {
                        MessageBox.Show("Невозможно удалить машину, так как она связана с автопарком.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var result = MessageBox.Show("Вы уверены, что хотите удалить машину?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (result == MessageBoxResult.Yes)
                    {
                        _context.Cars.Remove(car);
                        _context.SaveChanges();
                        MessageBox.Show("Машина успешно удалена.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadData();
                    }
                }
            }
        }

        private void ShowCarForm()
        {
            CarForm.Visibility = Visibility.Visible;
            Grid.SetRowSpan(CarForm, 2);

            ModelComboBox.ItemsSource = _context.CarModel.Select(cm => new { cm.IdModel, cm.Name }).ToList();
            BodyComboBox.ItemsSource = _context.BodyType.Select(bt => new { bt.IdBody, bt.Name }).ToList();

            if (!_isNewCar)
            {
                ModelComboBox.SelectedValue = _currentCar.IdModel;
                BodyComboBox.SelectedValue = _currentCar.IdBody;
                NumberOfSeatsTextBox.Text = _currentCar.NumberOfSeats.ToString();
                LoadCapacityTextBox.Text = _currentCar.LoadCapacity.ToString();
            }
        }

        private void HideCarForm()
        {
            CarForm.Visibility = Visibility.Collapsed;
            Grid.SetRowSpan(CarForm, 1);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ModelComboBox.SelectedItem == null || BodyComboBox.SelectedItem == null ||
                string.IsNullOrWhiteSpace(NumberOfSeatsTextBox.Text) || string.IsNullOrWhiteSpace(LoadCapacityTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, заполните все обязательные поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _currentCar.IdModel = (int)ModelComboBox.SelectedValue;
            _currentCar.IdBody = (int)BodyComboBox.SelectedValue;
            _currentCar.NumberOfSeats = int.Parse(NumberOfSeatsTextBox.Text);
            _currentCar.LoadCapacity = int.Parse(LoadCapacityTextBox.Text);

            if (_isNewCar)
            {
                _context.Cars.Add(_currentCar);
            }

            _context.SaveChanges();
            MessageBox.Show("Машина успешно сохранена.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            HideCarForm();
            LoadData();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            HideCarForm();
        }

        private void NumberOfSeatsTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^[0-9]*$");
        }

        private void LoadCapacityTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^[0-9]*\.?[0-9]+$");
        }

        private void AddModelButton_Click(object sender, RoutedEventArgs e)
        {
            CarModelCard card = new CarModelCard();
            card.Closed += (s, args) => ModelComboBox.ItemsSource = _context.CarModel.Select(cm => new { cm.IdModel, cm.Name }).ToList();
            card.ShowDialog();
        }

        private void AddBodyButton_Click(object sender, RoutedEventArgs e)
        {
            CarBodyCard card = new CarBodyCard();
            card.Closed += (s, args) => BodyComboBox.ItemsSource = _context.BodyType.Select(bt => new { bt.IdBody, bt.Name }).ToList();
            card.ShowDialog();
        }
    }
}
