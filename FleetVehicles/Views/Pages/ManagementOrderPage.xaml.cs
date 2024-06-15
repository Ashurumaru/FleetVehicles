using FleetVehicles.Data;
using FleetVehicles.Models;
using FleetVehicles.Views.Cards;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace FleetVehicles.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для ManagementOrderPage.xaml
    /// </summary>
    public partial class ManagementOrderPage : Page
    {
        private Employees _currentUser;
        private string _currentUserRole;
        private int? _employeeIdFilter;
        private int? _carIdFilter;

        public ManagementOrderPage(int currentUserId, int? employeeIdFilter, int? carIdFilter = null)
        {
            InitializeComponent();
            LoadUser(currentUserId);
            _employeeIdFilter = employeeIdFilter;
            _carIdFilter = carIdFilter;
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
                        btnCreateOrder.Visibility = Visibility.Hidden;
                    }
                }
            }
        }

        private void LoadData(string searchQuery = "")
        {
            using (var context = new FleetVehiclesEntities())
            {
                var ordersQuery = from order in context.Orders
                                  join customer in context.Customers on order.IdCustomer equals customer.IdCustomer
                                  join fleetCar in context.FleetCars on order.IdFleetCar equals fleetCar.IdFleetCar
                                  join car in context.Cars on fleetCar.IdCar equals car.IdCar
                                  join driver in context.Employees on fleetCar.IdDriver equals driver.IdEmployee
                                  join dispatcher in context.Employees on order.IdDispatcher equals dispatcher.IdEmployee
                                  join tariff in context.Tariff on order.IdTariff equals tariff.IdTariff
                                  select new
                                  {
                                      OrderID = order.IdOrder,
                                      CustomerName = customer.PhoneNumber,
                                      DateStart = order.DateStart,
                                      DateEnd = order.DateEnd,
                                      DepartureAddress = order.DepartureAddress,
                                      ArrivalAddress = order.ArrivalAddress,
                                      CarModel = car.CarModel.Name,
                                      CarBrand = car.CarModel.CarBrand.Name,
                                      DriverFirstName = driver.FirstName,
                                      DriverLastName = driver.LastName,
                                      TotalCost = order.TotalCost,
                                      DispatcherFirstName = dispatcher.FirstName,
                                      DispatcherLastName = dispatcher.LastName,
                                      TariffName = tariff.Name,
                                      NumberOfPassengers = order.NumberOfPassengers,
                                      Notes = order.Notes,
                                      Status = order.DateEnd.HasValue ? "Завершен" : "В процессе",
                                      IdDriver = order.FleetCars.IdDriver,
                                      IdDispatcher = order.IdDispatcher,
                                      IdFleetCar = order.FleetCars.IdFleetCar,
                                  };
                if (_employeeIdFilter != null)
                {
                    ordersQuery = ordersQuery.Where(o => o.IdDriver == _employeeIdFilter.Value || o.IdDispatcher == _employeeIdFilter.Value);
                    btnCreateOrder.Visibility = Visibility.Hidden;
                }
                if (_carIdFilter != null)
                {
                    ordersQuery = ordersQuery.Where(o => o.IdFleetCar == _carIdFilter.Value);
                    btnCreateOrder.Visibility = Visibility.Hidden;
                }
                var orders = ordersQuery.OrderByDescending(o => o.DateStart).ToList();
               
                if (!string.IsNullOrWhiteSpace(searchQuery))
                {
                    searchQuery = searchQuery.ToLower();
                    orders = orders.Where(o =>
                        o.CustomerName.ToLower().Contains(searchQuery) ||
                        o.DepartureAddress.ToLower().Contains(searchQuery) ||
                        o.ArrivalAddress.ToLower().Contains(searchQuery) ||
                        o.CarModel.ToLower().Contains(searchQuery) ||
                        o.CarBrand.ToLower().Contains(searchQuery) ||
                        o.DriverFirstName.ToLower().Contains(searchQuery) ||
                        o.DriverLastName.ToLower().Contains(searchQuery) ||
                        o.DispatcherFirstName.ToLower().Contains(searchQuery) ||
                        o.DispatcherLastName.ToLower().Contains(searchQuery) ||
                        o.TariffName.ToLower().Contains(searchQuery) ||
                        o.Notes.ToLower().Contains(searchQuery) ||
                        o.DateStart.ToString().Contains(searchQuery) ||
                        o.DateEnd.ToString().Contains(searchQuery) ||
                        o.TotalCost.ToString().Contains(searchQuery) ||
                        o.NumberOfPassengers.ToString().Contains(searchQuery) ||
                        o.Status.ToLower().Contains(searchQuery)
                    ).ToList();
                }

                var orderViews = orders.Select(o => new OrderView
                {
                    OrderID = o.OrderID,
                    CustomerName = o.CustomerName,
                    DateStart = o.DateStart,
                    DateEnd = o.DateEnd,
                    DepartureAddress = o.DepartureAddress,
                    ArrivalAddress = o.ArrivalAddress,
                    Car = o.CarBrand + " " + o.CarModel,
                    Driver = o.DriverFirstName + " " + o.DriverLastName,
                    TotalCost = (int)o.TotalCost,
                    Dispatcher = o.DispatcherFirstName + " " + o.DispatcherLastName,
                    Tariff = o.TariffName,
                    NumberOfPassengers = (int)o.NumberOfPassengers,
                    Notes = o.Notes,
                    TripDate = $"{o.DateStart:dd.MM.yyyy HH:mm} - {o.DateEnd:dd.MM.yyyy HH:mm}",
                    Status = o.Status,
                    IsCompleted = o.Status != "Завершен",
                    CanManageOrder = _currentUserRole == "Администратор" || _currentUserRole == "Диспетчер"
                }).ToList();
                OrderList.ItemsSource = orderViews;
                if (orders.Count == 0)
                {
                    if (_carIdFilter == null && _employeeIdFilter == null)
                    {
                        MessageBox.Show("Нет заказов, соответствующих вашему запросу.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
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
        private void ShowOrderCard_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                var order = button.CommandParameter as OrderView;
                if (order != null)
                {
                    OrderCard card = new OrderCard(order.OrderID, _currentUser.IdEmployee);
                    card.Closed += Card_Closed;
                    card.Show();
                }
                else
                {
                    OrderCard card = new OrderCard(null, _currentUser.IdEmployee);
                    card.Closed += Card_Closed;
                    card.Show();
                }
            }
        }

        private void Card_Closed(object sender, EventArgs e)
        {
            LoadData();
        }

        private void CloseOrder_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var orderView = button?.CommandParameter as OrderView;
            if (orderView != null)
            {
                using (var context = new FleetVehiclesEntities())
                {
                    var order = context.Orders.SingleOrDefault(o => o.IdOrder == orderView.OrderID);
                    if (order != null)
                    {
                        order.DateEnd = DateTime.Now;
                        context.SaveChanges();
                        MessageBox.Show("Заказ успешно закрыт.");
                        LoadData();
                    }
                    else
                    {
                        MessageBox.Show("Заказ не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void DeleteOrder_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var orderView = button?.CommandParameter as OrderView;
            if (orderView != null)
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить заказ?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    using (var context = new FleetVehiclesEntities())
                    {
                        var order = context.Orders.SingleOrDefault(o => o.IdOrder == orderView.OrderID);
                        if (order != null)
                        {
                            var relatedServices = context.OrderAdditionalService.Where(oas => oas.IdOrder == order.IdOrder).ToList();
                            foreach (var service in relatedServices)
                            {
                                context.OrderAdditionalService.Remove(service);
                            }

                            context.Orders.Remove(order);
                            context.SaveChanges();
                            MessageBox.Show($"Заказ №{order.IdOrder} успешно удален.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                            LoadData();
                        }
                        else
                        {
                            MessageBox.Show("Заказ не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
        }

        private void OrderList_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (var item in OrderList.Items)
            {
                var container = OrderList.ItemContainerGenerator.ContainerFromItem(item) as ContentPresenter;
                if (container == null)
                    continue;

                var closeOrderButton = container.ContentTemplate.FindName("CloseOrder", container) as Button;
                var deleteOrderButton = container.ContentTemplate.FindName("DeleteOrder", container) as Button;
                var orderView = item as OrderView;

                if (closeOrderButton != null && deleteOrderButton != null)
                {
                    closeOrderButton.Visibility = (orderView.Status != "Завершен" && (_currentUser.IdPosition == 3 || _currentUser.IdPosition == 2)) ? Visibility.Visible : Visibility.Collapsed;

                    deleteOrderButton.Visibility = _currentUser.IdPosition == 3 || _currentUser.IdPosition == 2 ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }
    }
}
