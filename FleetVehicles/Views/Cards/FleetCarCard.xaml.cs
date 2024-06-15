using FleetVehicles.Data;
using FleetVehicles.Views.Pages;
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
    /// Логика взаимодействия для FleetCarCard.xaml
    /// </summary>
    public partial class FleetCarCard : Window
    {
        private readonly FleetVehiclesEntities _context;
        private FleetCars _fleetCar;
        private bool _isNewFleetCar;
        private int _currentUserId;
        public FleetCarCard(int currentUserId,int? fleetCarId)
        {
            InitializeComponent();
            _context = new FleetVehiclesEntities();
            _isNewFleetCar = !fleetCarId.HasValue;
            _currentUserId = currentUserId;

            if (_isNewFleetCar)
            {
                _fleetCar = new FleetCars();
                DataContext = _fleetCar;
            }
            else
            {
                _fleetCar = _context.FleetCars.SingleOrDefault(fc => fc.IdFleetCar == fleetCarId.Value);
                DataContext = _fleetCar;
            }

            LoadComboBoxData();
            LoadOrders();
            LoadInsurancePolicies();
        }

        private void LoadComboBoxData()
        {
            DriverComboBox.ItemsSource = _context.Employees.Where(e => e.IdPosition == 1).Select(e => new
            {
                e.IdEmployee,
                FullName = e.FirstName + " " + e.LastName
            }).ToList();

            CarComboBox.ItemsSource = _context.Cars.Select(c => new
            {
                c.IdCar,
                CarInfo = c.CarModel.Name + " " + c.CarModel.CarBrand.Name
            }).ToList();

            ColorComboBox.ItemsSource = _context.CarColor.Select(cc => new
            {
                cc.IdColor,
                cc.Name
            }).ToList();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(VinNumberTextBox.Text) ||
                string.IsNullOrWhiteSpace(RegistrationNumberTextBox.Text) ||
                DriverComboBox.SelectedItem == null ||
                CarComboBox.SelectedItem == null ||
                ColorComboBox.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, заполните все обязательные поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _fleetCar.VinNumber = VinNumberTextBox.Text;
            _fleetCar.RegistrationNumber = RegistrationNumberTextBox.Text;
            _fleetCar.IdDriver = (int)DriverComboBox.SelectedValue;
            _fleetCar.IdCar = (int)CarComboBox.SelectedValue;
            _fleetCar.IdColor = (int)ColorComboBox.SelectedValue;

            if (_isNewFleetCar)
            {
                _context.FleetCars.Add(_fleetCar);
            }

            _context.SaveChanges();
            MessageBox.Show("Машина автопарка успешно сохранена.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void VinNumberTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^[a-zA-Z0-9]+$");
        }

        private void RegistrationNumberTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^[a-zA-Z0-9]+$");
        }

        private void AddDriverButton_Click(object sender, RoutedEventArgs e)
        {
            EmployeeCard card = new EmployeeCard(_currentUserId, null); 
            card.Closed += (s, args) => LoadComboBoxData(); 
            card.ShowDialog();
        }

        private void AddCarButton_Click(object sender, RoutedEventArgs e)
        {
            CarCard card = new CarCard(); 
            card.Closed += (s, args) => LoadComboBoxData();
            card.ShowDialog();
        }

        private void AddColorButton_Click(object sender, RoutedEventArgs e)
        {
            ColorCard card = new ColorCard(); 
            card.Closed += (s, args) => LoadComboBoxData(); 
            card.ShowDialog();
        }
        private void LoadOrders()
        {
            FrameOrders.Navigate(new ManagementOrderPage(_currentUserId, null,_fleetCar.IdFleetCar));
        }
        private void LoadInsurancePolicies()
        {
            var policies = _context.InsurancePolicy.Where(ip => ip.IdFleetCar == _fleetCar.IdFleetCar).Select(ip => new
            {
                ip.IdInsurance,
                ip.Type,
                ip.Number,
                ip.DateOfIssue,
                ip.DateOfExpiry
            }).ToList();

            InsurancePolicyList.ItemsSource = policies;
        }

        private void AddInsurancePolicy_Click(object sender, RoutedEventArgs e)
        {
            InsurancePolicyCard card = new InsurancePolicyCard(_fleetCar.IdFleetCar, null);
            card.Closed += (s, args) => LoadInsurancePolicies();
            card.ShowDialog();
        }

        private void EditInsurancePolicy_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var policyId = button.CommandParameter as int?;
            if (policyId.HasValue)
            {
                InsurancePolicyCard card = new InsurancePolicyCard(_fleetCar.IdFleetCar, policyId.Value);
                card.Closed += (s, args) => LoadInsurancePolicies();
                card.ShowDialog();
            }
        }

        private void DeleteInsurancePolicy_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var policyId = button.CommandParameter as int?;
            if (policyId.HasValue)
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить полис?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    var policy = _context.InsurancePolicy.SingleOrDefault(ip => ip.IdInsurance == policyId.Value);
                    if (policy != null)
                    {
                        _context.InsurancePolicy.Remove(policy);
                        _context.SaveChanges();
                        LoadInsurancePolicies();
                        MessageBox.Show("Полис успешно удален.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
        }
    }
}

