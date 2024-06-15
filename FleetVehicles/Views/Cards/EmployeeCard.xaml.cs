using FleetVehicles.Data;
using FleetVehicles.Views.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
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
    /// Логика взаимодействия для EmployeeCard.xaml
    /// </summary>
    public partial class EmployeeCard : Window
    {
        private readonly FleetVehiclesEntities _context;
        private Employees _employee;
        private bool _isNewEmployee;
        private int _currentUserId;
        public EmployeeCard(int currentUserId,int? employeeId)
        {
            InitializeComponent();
            _context = new FleetVehiclesEntities();
            _currentUserId = currentUserId;
            _isNewEmployee = !employeeId.HasValue;

            if (_isNewEmployee)
            {
                _employee = new Employees();
                DataContext = _employee;
            }
            else
            {
                var user = _context.Employees.SingleOrDefault(e => e.IdEmployee == _currentUserId);
                if (user != null)
                {
                    if (user.Position.Name != "Администратор")
                    {
                        PositionComboBox.IsEnabled = false;
                    }
                }
                _employee = _context.Employees.SingleOrDefault(e => e.IdEmployee == employeeId.Value);
                DataContext = _employee;
                FrameOrders.Visibility = Visibility.Visible;
                FrameOrders.Navigate(new ManagementOrderPage(_currentUserId, employeeId));
            }

            LoadComboBoxData();
        }

        private void LoadComboBoxData()
        {
            PositionComboBox.ItemsSource = _context.Position.Select(p => new
            {
                p.IdPosition,
                p.Name
            }).ToList();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(FirstNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(LastNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(PhoneNumberTextBox.Text) ||
                string.IsNullOrWhiteSpace(LoginTextBox.Text) ||
                string.IsNullOrWhiteSpace(PasswordTextBox.Text) ||
                PositionComboBox.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, заполните все обязательные поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!ValidatePhoneNumber(PhoneNumberTextBox.Text))
            {
                MessageBox.Show("Введите корректный номер телефона.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _employee.FirstName = FirstNameTextBox.Text;
            _employee.LastName = LastNameTextBox.Text;
            _employee.Patronymic = PatronymicTextBox.Text;
            _employee.PhoneNumber = PhoneNumberTextBox.Text;
            _employee.DriverLicenseNumber = DriverLicenseNumberTextBox.Text;
            _employee.Login = LoginTextBox.Text;
            _employee.Password = PasswordTextBox.Text;
            _employee.IdPosition = (int)PositionComboBox.SelectedValue;

            if (_isNewEmployee)
            {
                _context.Employees.Add(_employee);
            }

            _context.SaveChanges();
            MessageBox.Show("Сотрудник успешно сохранен.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private bool ValidatePhoneNumber(string phoneNumber)
        {
            return Regex.IsMatch(phoneNumber, @"^\+?\d{10,15}$");
        }

        private bool ValidateEmail(string email)
        {
            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }
    }
}
