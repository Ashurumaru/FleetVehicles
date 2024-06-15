using FleetVehicles.Data;
using FleetVehicles.Models;
using FleetVehicles.Views.Cards;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FleetVehicles.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для PersonalAccountPage.xaml
    /// </summary>
    public partial class PersonalAccountPage : Page
    {
        private int _currentUserId;

        public PersonalAccountPage(int currentUserId)
        {
            InitializeComponent();
            _currentUserId = currentUserId;
            LoadEmployeeData();
        }

        private void LoadEmployeeData()
        {
            using (var context = new FleetVehiclesEntities())
            {
                var employee = (from e in context.Employees
                                join p in context.Position on e.IdPosition equals p.IdPosition
                                where e.IdEmployee == _currentUserId
                                select new
                                {
                                    e.IdEmployee,
                                    e.FirstName,
                                    e.LastName,
                                    e.Patronymic,
                                    e.PhoneNumber,
                                    e.DriverLicenseNumber,
                                    PositionName = p.Name
                                }).SingleOrDefault();

                if (employee != null)
                {
                    var employeeView = new PersonalAccountView
                    {
                        IdEmployee = employee.IdEmployee,
                        FirstName = employee.FirstName,
                        LastName = employee.LastName,
                        Patronymic = employee.Patronymic,
                        PhoneNumber = employee.PhoneNumber,
                        DriverLicenseNumber = employee.DriverLicenseNumber,
                        IsDriver = employee.PositionName == "Водитель"
                    };

                    DataContext = employeeView;

                    if (employee.PositionName == "Диспетчер")
                    {
                        DispatcherStats.Visibility = Visibility.Visible;
                        LoadDispatcherStats(employeeView, null, null);
                    }
                    else if (employee.PositionName == "Водитель")
                    {
                        DriverLicense.Visibility = Visibility.Visible;
                        DriverLicenseNumber.Visibility = Visibility.Visible;
                        DriverStats.Visibility = Visibility.Visible;
                        LoadDriverStats(employeeView, null, null);
                    }
                }
                else
                {
                    MessageBox.Show("Сотрудник не найден.");
                }
            }
        }

        private void LoadDispatcherStats(PersonalAccountView employeeView, DateTime? startDate, DateTime? endDate)
        {
            using (var context = new FleetVehiclesEntities())
            {
                var query = context.Orders.Where(o => o.IdDispatcher == employeeView.IdEmployee);
                if (startDate.HasValue && endDate.HasValue)
                {
                    query = query.Where(o => o.DateStart >= startDate.Value && o.DateStart <= endDate.Value);
                }
                employeeView.ProcessedOrdersCount = query.Count();
                ProcessedOrdersCountTextBlock.Text = employeeView.ProcessedOrdersCount.ToString();

            }
        }

        private void LoadDriverStats(PersonalAccountView employeeView, DateTime? startDate, DateTime? endDate)
        {
            using (var context = new FleetVehiclesEntities())
            {
                var query = context.Orders.Where(o => o.FleetCars.IdDriver == employeeView.IdEmployee);
                if (startDate.HasValue && endDate.HasValue)
                {
                    query = query.Where(o => o.DateStart >= startDate.Value && o.DateStart <= endDate.Value);
                }
                employeeView.TripsCount = query.Count();
                employeeView.PassengersCount = query.Sum(o => (int?)o.NumberOfPassengers) ?? 0;
                TripsCountTextBlock.Text = employeeView.TripsCount.ToString();
                PassengersCountTextBlock.Text = employeeView.PassengersCount.ToString();
            }
        }

        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            DateTime? startDate = StartDatePicker.SelectedDate;
            DateTime? endDate = EndDatePicker.SelectedDate;

            if (DataContext is PersonalAccountView employeeView)
            {
                if (employeeView.IsDriver)
                {
                    LoadDriverStats(employeeView, startDate, endDate);
                }
                else
                {
                    LoadDispatcherStats(employeeView, startDate, endDate);
                }
            }
        }

        private void ShowCardEmployee_Click(object sender, RoutedEventArgs e)
        {
            EmployeeCard card = new EmployeeCard(_currentUserId, _currentUserId);
            card.Closed += (s, args) => LoadEmployeeData();
            card.ShowDialog();
        }
    }
}
