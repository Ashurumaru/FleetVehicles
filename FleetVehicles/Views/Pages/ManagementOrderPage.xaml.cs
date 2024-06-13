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
    /// Логика взаимодействия для ManagementOrderPage.xaml
    /// </summary>
    public partial class ManagementOrderPage : Page
    {
        private int _currentUserId;
        public ManagementOrderPage(int currentUserId)
        {
            InitializeComponent();
            LoadData();
            _currentUserId = currentUserId;
        }

        private void LoadData()
        {
            using (var context = new FleetVehiclesEntities())
            {
                var orders = (from order in context.Orders
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
                                  Notes = order.Notes
                              }).ToList()
                              .Select(o => new OrderView
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
                                  TripDate = $"{o.DateStart:dd.MM.yyyy HH:mm} - {o.DateEnd:dd.MM.yyyy HH:mm}"
                              }).ToList();

                OrderList.ItemsSource = orders;
            }
        }


        private void ShowOrderCard_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                var order = button.CommandParameter as OrderView;
                if (order != null)
                {
                    using (var context = new FleetVehiclesEntities())
                    {
                        Orders orderData = context.Orders.SingleOrDefault(o => o.IdOrder == order.OrderID);
                        if (orderData != null)
                        {
                            OrderCard card = new OrderCard(_currentUserId, orderData);
                            card.Closed += Card_Closed;
                            card.Show();
                        }
                        else
                        {
                            MessageBox.Show("Заявка не найдена.");
                        }
                    }
                }
                else
                {
                    OrderCard card = new OrderCard(_currentUserId);
                    card.Closed += Card_Closed;
                    card.Show();
                }
            }
        }

        private void Card_Closed(object sender, EventArgs e)
        {
            LoadData();
        }

        private void btnCreateOrder_Click(object sender, RoutedEventArgs e)
        {
            OrderCard card = new OrderCard(_currentUserId);
            card.Closed += Card_Closed;
            card.Show();
        }
    }
}
