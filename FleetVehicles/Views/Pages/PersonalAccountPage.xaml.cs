using FleetVehicles.Data;
using FleetVehicles.Models;
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
                        LoadDispatcherStats(employeeView);
                    }
                    else if (employee.PositionName == "Водитель")
                    {
                        DriverLicense.Visibility = Visibility.Visible;
                        DriverLicenseNumber.Visibility = Visibility.Visible;
                        DriverStats.Visibility = Visibility.Visible;
                        LoadDriverStats(employeeView);
                    }
                }
                else
                {
                    MessageBox.Show("Сотрудник не найден.");
                }
            }
        }

        private void LoadDispatcherStats(PersonalAccountView employeeView)
        {
            using (var context = new FleetVehiclesEntities())
            {
                employeeView.ProcessedOrdersCount = context.Orders.Count(o => o.IdDispatcher == employeeView.IdEmployee);
            }
        }

        private void LoadDriverStats(PersonalAccountView employeeView)
        {
            using (var context = new FleetVehiclesEntities())
            {
                employeeView.TripsCount = context.Orders.Count(o => o.FleetCars.IdDriver == employeeView.IdEmployee);
            }
        }

        private void ShowCardEmployee_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
