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
    /// Логика взаимодействия для CustomerCard.xaml
    /// </summary>
    public partial class CustomerCard : Window
    {
        private readonly FleetVehiclesEntities _context;
        private Customers _customer;
        private bool _isNewCustomer;

        public CustomerCard(int? customerId = null)
        {
            InitializeComponent();
            _context = new FleetVehiclesEntities();
            _isNewCustomer = !customerId.HasValue;

            if (_isNewCustomer)
            {
                _customer = new Customers();
                DataContext = _customer;
            }
            else
            {
                _customer = _context.Customers.SingleOrDefault(c => c.IdCustomer == customerId.Value);
                DataContext = _customer;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(PhoneNumberTextBox.Text) ||
                !ValidatePhoneNumber(PhoneNumberTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, заполните все обязательные поля корректно.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _customer.PhoneNumber = PhoneNumberTextBox.Text;
            _customer.Notes = NotesTextBox.Text;
            _customer.StopList = StopListCheckBox.IsChecked == true;

            if (_isNewCustomer)
            {
                _context.Customers.Add(_customer);
            }

            _context.SaveChanges();
            MessageBox.Show("Клиент успешно сохранен.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
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
    }
}
