using FleetVehicles.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    /// Логика взаимодействия для InsurancePolicyCard.xaml
    /// </summary>
    public partial class InsurancePolicyCard : Window
    {
        private readonly FleetVehiclesEntities _context;
        private InsurancePolicy _insurancePolicy;
        private bool _isNewInsurancePolicy;

        public InsurancePolicyCard(int fleetCarId, int? insurancePolicyId)
        {
            InitializeComponent();
            _context = new FleetVehiclesEntities();
            _isNewInsurancePolicy = !insurancePolicyId.HasValue;

            if (_isNewInsurancePolicy)
            {
                _insurancePolicy = new InsurancePolicy { IdFleetCar = fleetCarId };
                DataContext = _insurancePolicy;
            }
            else
            {
                _insurancePolicy = _context.InsurancePolicy.SingleOrDefault(ip => ip.IdInsurance == insurancePolicyId.Value);
                DataContext = _insurancePolicy;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TypeTextBox.Text) ||
                string.IsNullOrWhiteSpace(NumberTextBox.Text) ||
                !DateOfIssuePicker.SelectedDate.HasValue ||
                !DateOfExpiryPicker.SelectedDate.HasValue)
            {
                MessageBox.Show("Пожалуйста, заполните все обязательные поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _insurancePolicy.Type = TypeTextBox.Text;
            _insurancePolicy.Number = NumberTextBox.Text;
            _insurancePolicy.DateOfIssue = DateOfIssuePicker.SelectedDate.Value;
            _insurancePolicy.DateOfExpiry = DateOfExpiryPicker.SelectedDate.Value;

            if (_isNewInsurancePolicy)
            {
                _context.InsurancePolicy.Add(_insurancePolicy);
            }

            _context.SaveChanges();
            MessageBox.Show("Страховой полис успешно сохранен.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}