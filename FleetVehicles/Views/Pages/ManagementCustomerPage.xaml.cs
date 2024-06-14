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
    /// Логика взаимодействия для ManagementCustomerPage.xaml
    /// </summary>
    public partial class ManagementCustomerPage : Page
    {
        private Employees _currentUser;

        public ManagementCustomerPage(int currentUserId)
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
                        btnCreateCustomer.Visibility = Visibility.Hidden;
                    }
                }
            }
        }

        private void LoadData(string searchQuery = "")
        {
            using (var context = new FleetVehiclesEntities())
            {
                var customersQuery = from customer in context.Customers
                                     select new
                                     {
                                         customer.IdCustomer,
                                         customer.PhoneNumber,
                                         customer.Notes,
                                         customer.StopList,
                                         TotalSpent = context.Orders.Where(o => o.IdCustomer == customer.IdCustomer).Sum(o => o.TotalCost),
                                         TotalTrips = context.Orders.Count(o => o.IdCustomer == customer.IdCustomer)
                                     };

                var customers = customersQuery.OrderBy(c => c.PhoneNumber).ToList();

                if (!string.IsNullOrWhiteSpace(searchQuery))
                {
                    searchQuery = searchQuery.ToLower();
                    customers = customers.Where(c =>
                        c.PhoneNumber.ToLower().Contains(searchQuery) ||
                        c.Notes.ToLower().Contains(searchQuery)
                    ).ToList();
                }

                var customerViews = customers.Select(c => new CustomerView
                {
                    IdCustomer = c.IdCustomer,
                    PhoneNumber = c.PhoneNumber,
                    Notes = c.Notes,
                    TotalSpent = c.TotalSpent,
                    TotalTrips = c.TotalTrips,
                    StopList = c.StopList == true ? "В стоп листе" : "Не в стоп листе"
                }).ToList();

                CustomerList.ItemsSource = customerViews;
                if (customers.Count == 0)
                {
                    MessageBox.Show("Нет клиентов, соответствующих вашему запросу.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
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

        private void ShowCustomerCard_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                var customer = button.CommandParameter as CustomerView;
                if (customer != null)
                {
                    CustomerCard card = new CustomerCard(customer.IdCustomer);
                    card.Closed += Card_Closed;
                    card.Show();
                }
                else
                {
                    CustomerCard card = new CustomerCard();
                    card.Closed += Card_Closed;
                    card.Show();
                }
            }
        }

        private void Card_Closed(object sender, EventArgs e)
        {
            LoadData();
        }

        private void DeleteCustomer_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var customerView = button?.CommandParameter as CustomerView;
            if (customerView != null)
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить клиента?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    using (var context = new FleetVehiclesEntities())
                    {
                        var customer = context.Customers.SingleOrDefault(c => c.IdCustomer == customerView.IdCustomer);
                        if (customer != null)
                        {
                            context.Customers.Remove(customer);
                            context.SaveChanges();
                            MessageBox.Show($"Клиент {customer.PhoneNumber} успешно удален.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                            LoadData();
                        }
                        else
                        {
                            MessageBox.Show("Клиент не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
        }
    }
}