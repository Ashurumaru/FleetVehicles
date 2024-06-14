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
    /// Логика взаимодействия для ManagementEmployeePage.xaml
    /// </summary>
    public partial class ManagementEmployeePage : Page
    {
        private Employees _currentUser;

        public ManagementEmployeePage(int currentUserId)
        {
            InitializeComponent();
            LoadUser(currentUserId);
            LoadData();
        }

        private void LoadUser(int userId)
        {
            using (var context = new FleetVehiclesEntities())
            {
                var user = context.Employees.SingleOrDefault(e => e.IdEmployee == userId);
                if (user != null)
                {
                    _currentUser = user;
                    if (_currentUser.IdPosition != 3 && _currentUser.IdPosition != 2)
                    {
                        btnCreateEmployee.Visibility = Visibility.Hidden;
                    }
                }
            }
        }

        private void LoadData(string searchQuery = "")
        {
            using (var context = new FleetVehiclesEntities())
            {
                var employeesQuery = from employee in context.Employees
                                     join position in context.Position on employee.IdPosition equals position.IdPosition
                                     select new
                                     {
                                         employee.IdEmployee,
                                         FullName = employee.LastName + " " + employee.FirstName + " " + employee.Patronymic,
                                         employee.PhoneNumber,
                                         employee.Login,
                                         employee.Password,
                                         employee.DriverLicenseNumber,
                                         employee.Patronymic,
                                         PositionName = position.Name,
                                         TotalOrders = context.Orders.Count(o => o.IdDispatcher == employee.IdEmployee || o.FleetCars.IdDriver == employee.IdEmployee),
                                     };

                var employees = employeesQuery.OrderBy(e => e.FullName).ToList();

                if (!string.IsNullOrWhiteSpace(searchQuery))
                {
                    searchQuery = searchQuery.ToLower();
                    employees = employees.Where(e =>
                        e.FullName.ToLower().Contains(searchQuery) ||
                        e.PhoneNumber.ToLower().Contains(searchQuery) ||
                        e.Login.ToLower().Contains(searchQuery) ||
                        e.PositionName.ToLower().Contains(searchQuery) ||
                        e.DriverLicenseNumber.ToLower().Contains(searchQuery) ||
                        e.Password.ToLower().Contains(searchQuery) ||
                        e.Patronymic.ToLower().Contains(searchQuery)
                    ).ToList();
                }

                var employeeViews = employees.Select(e => new EmployeeView
                {
                    IdEmployee = e.IdEmployee,
                    FullName = e.FullName,
                    PhoneNumber = e.PhoneNumber,
                    Login = e.Login,
                    Password = e.Password,
                    DriverLicenseNumber = e.DriverLicenseNumber,
                    Patronymic = e.Patronymic,
                    PositionName = e.PositionName,
                    TotalOrders = e.TotalOrders,
                }).ToList();

                EmployeeList.ItemsSource = employeeViews;
                if (employees.Count == 0)
                {
                    MessageBox.Show("Нет сотрудников, соответствующих вашему запросу.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadData(SearchTextBox.Text);
        }

        private void ResetSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = string.Empty;
            LoadData();
        }

        private void ShowEmployeeCard_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                var employee = button.CommandParameter as EmployeeView;
                if (employee != null)
                {
                    EmployeeCard card = new EmployeeCard(_currentUser.IdEmployee, employee.IdEmployee);
                    card.Closed += Card_Closed;
                    card.Show();
                }
                else
                {
                    EmployeeCard card = new EmployeeCard(_currentUser.IdEmployee, null);
                    card.Closed += Card_Closed;
                    card.Show();
                }
            }
        }

        private void Card_Closed(object sender, EventArgs e)
        {
            LoadData();
        }

        private void DeleteEmployee_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var employeeView = button?.CommandParameter as EmployeeView;
            if (employeeView != null)
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить сотрудника?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    using (var context = new FleetVehiclesEntities())
                    {
                        var employee = context.Employees.SingleOrDefault(u => u.IdEmployee == employeeView.IdEmployee);
                        if (employee != null)
                        {
                            var hasOrders = context.Orders.Any(o => o.IdDispatcher == employee.IdEmployee || o.FleetCars.IdDriver == employee.IdEmployee);
                            if (hasOrders)
                            {
                                MessageBox.Show("Сотрудник не может быть удален, так как он связан с заказами.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }

                            context.Employees.Remove(employee);
                            context.SaveChanges();
                            MessageBox.Show($"Сотрудник {employee.FirstName} {employee.LastName} успешно удален.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                            LoadData();
                        }
                        else
                        {
                            MessageBox.Show("Сотрудник не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
        }
    }
}