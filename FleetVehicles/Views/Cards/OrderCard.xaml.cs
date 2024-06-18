using FleetVehicles.Data;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using System.Text;
using System.Runtime.InteropServices;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using FleetVehicles.Models;

namespace FleetVehicles.Views.Cards
{
    public partial class OrderCard : Window
    {
        private readonly FleetVehiclesEntities _context;
        private Orders _order;
        private bool _isNewOrder;
        private int _currentUserId;
        private List<AdditionalServiceView> _additionalServices;

        public OrderCard(int? orderId, int currentUserId)
        {
            InitializeComponent();
            _context = new FleetVehiclesEntities();
            _currentUserId = currentUserId;
            _isNewOrder = !orderId.HasValue;

            if (_isNewOrder)
            {
                _order = new Orders
                {
                    IdDispatcher = _currentUserId
                };
                var dispatcher = _context.Employees.SingleOrDefault(e => e.IdEmployee == _currentUserId);
                DispatcherComboBox.Text = $"{dispatcher.FirstName} {dispatcher.LastName}";
                InitializeDefaultValues();
            }
            else
            {
                _order = _context.Orders.SingleOrDefault(o => o.IdOrder == orderId.Value);
            }

            DataContext = _order;
            LoadComboBoxData();
            LoadAdditionalServices();
            UpdateTotalCost();
        }

        private void InitializeDefaultValues()
        {
            DateTime now = DateTime.Now;
            DateStartPicker.SelectedDate = now;
            TimeStartTextBox.Text = now.ToString("HH:mm");
            //DateEndPicker.SelectedDate = now.Date;
            //TimeEndTextBox.Text = now.AddHours(1).ToString("HH:mm"); 

            TotalCostTextBox.Text = "0";
            NumberOfPassengersTextBox.Text = "1";
        }

        private void LoadAdditionalServices()
        {
            _additionalServices = _order.OrderAdditionalService
                                        .Select(oas => new AdditionalServiceView
                                        {
                                            IdAdditionalService = (int)oas.IdAdditionalService,
                                            Name = oas.AdditionalService.Name,
                                            Quantity = (int)oas.Quantity,
                                            Cost = (int)oas.AdditionalService.Cost
                                        }).ToList();
            UpdateAdditionalServicesListBox();
        }

        private void LoadOrderData(int orderId)
        {
            _order = _context.Orders.SingleOrDefault(o => o.IdOrder == orderId);
            DataContext = _order;
            LoadComboBoxData();
            UpdateAdditionalServicesListBox();
        }

        private void LoadComboBoxData()
        {
            if (!_isNewOrder)
            {
                if (_order.DateStart != null) 
                {
                    TimeStartTextBox.Text = $"{_order.DateStart.Value.Hour:D2}:{_order.DateStart.Value.Minute:D2}";

                }
                if (_order.DateEnd != null)
                {
                    TimeEndTextBox.Text = $"{_order.DateEnd.Value.Hour:D2}:{_order.DateEnd.Value.Minute:D2}";

                }
            }

            DispatcherComboBox.ItemsSource = _context.Employees.Where(e => e.IdPosition == 2).Select(e => new
            {
                e.IdEmployee,
                FullName = e.FirstName + " " + e.LastName
            }).ToList();

            CustomerComboBox.ItemsSource = _context.Customers.Select(c => new
            {
                c.IdCustomer,
                c.PhoneNumber
            }).ToList();

            var availableCars = _context.FleetCars
                .Where(fc => !_context.Orders
                .Any(o => o.FleetCars.IdDriver == fc.IdDriver && o.DateEnd == null))
                .Select(fc => new
                {
                    fc.IdFleetCar,
                    CarInfo = fc.Cars.CarModel.Name + " " + fc.Cars.CarModel.CarBrand.Name
                })
                .ToList();

            FleetCarComboBox.ItemsSource = availableCars;


            TariffComboBox.ItemsSource = _context.Tariff.Select(t => new
            {
                t.IdTariff,
                t.Name,
                t.Cost 
            }).ToList();


            AdditionalServiceComboBox.ItemsSource = _context.AdditionalService.Select(s => new
            {
                s.IdAdditionalService,
                s.Name,
                s.Cost
            }).ToList();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateTime(TimeStartTextBox.Text))
            {
                MessageBox.Show("Введите корректное время начала в формате ЧЧ:ММ.");
                return;
            }

            if (!string.IsNullOrWhiteSpace(TimeEndTextBox.Text) && !ValidateTime(TimeEndTextBox.Text))
            {
                MessageBox.Show("Введите корректное время окончания в формате ЧЧ:ММ или оставьте поле пустым.");
                return;
            }

            if (string.IsNullOrWhiteSpace(DepartureAddressTextBox.Text) ||
                string.IsNullOrWhiteSpace(ArrivalAddressTextBox.Text) ||
                CustomerComboBox.SelectedItem == null ||
                FleetCarComboBox.SelectedItem == null ||
                DispatcherComboBox.SelectedItem == null ||
                TariffComboBox.SelectedItem == null ||
                !DateStartPicker.SelectedDate.HasValue)
            {
                MessageBox.Show("Пожалуйста, заполните все обязательные поля.");
                return;
            }

            if (!int.TryParse(TotalCostTextBox.Text, out int totalCost) ||
                !int.TryParse(NumberOfPassengersTextBox.Text, out int numberOfPassengers))
            {
                MessageBox.Show("Пожалуйста, введите корректные числовые значения для суммы и количества пассажиров.");
                return;
            }

            _order.DateStart = DateStartPicker.SelectedDate.Value.Date + TimeSpan.Parse(TimeStartTextBox.Text);
            if (!string.IsNullOrWhiteSpace(TimeEndTextBox.Text) && DateEndPicker.SelectedDate.HasValue)
            {
                _order.DateEnd = DateEndPicker.SelectedDate.Value.Date + TimeSpan.Parse(TimeEndTextBox.Text);
            }
            else
            {
                _order.DateEnd = null; 
            }
            _order.DepartureAddress = DepartureAddressTextBox.Text;
            _order.ArrivalAddress = ArrivalAddressTextBox.Text;
            _order.TotalCost = totalCost;
            _order.NumberOfPassengers = numberOfPassengers;
            _order.Notes = NotesTextBox.Text;
            _order.IdCustomer = (int)CustomerComboBox.SelectedValue;
            _order.IdFleetCar = (int)FleetCarComboBox.SelectedValue;
            _order.IdDispatcher = (int)DispatcherComboBox.SelectedValue;
            _order.IdTariff = (int)TariffComboBox.SelectedValue;

            if (_isNewOrder)
            {
                _context.Orders.Add(_order);
            }

            try
            {
                _context.SaveChanges();
                MessageBox.Show("Заказ успешно сохранен.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении заказа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            using (var updateContext = new FleetVehiclesEntities())
            {
                var existingServices = updateContext.OrderAdditionalService.Where(oas => oas.IdOrder == _order.IdOrder).ToList();

                foreach (var service in existingServices)
                {
                    if (!_additionalServices.Any(s => s.IdAdditionalService == service.IdAdditionalService))
                    {
                        updateContext.OrderAdditionalService.Remove(service);
                    }
                }

                foreach (var service in _additionalServices)
                {
                    var existingService = existingServices.FirstOrDefault(es => es.IdAdditionalService == service.IdAdditionalService);
                    if (existingService == null)
                    {
                        updateContext.OrderAdditionalService.Add(new OrderAdditionalService
                        {
                            IdOrder = _order.IdOrder,
                            IdAdditionalService = service.IdAdditionalService,
                            Quantity = service.Quantity
                        });
                    }
                    else
                    {
                        existingService.Quantity = service.Quantity;
                    }
                }

                try
                {
                    updateContext.SaveChanges();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при обновлении дополнительных услуг: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }
            Close();
        }


        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void AddAdditionalService_Click(object sender, RoutedEventArgs e)
        {
            var selectedService = AdditionalServiceComboBox.SelectedItem as dynamic;
            int quantity = 0;

            if (selectedService != null && int.TryParse(ServiceQuantityTextBox.Text, out quantity) && quantity > 0)
            {
                var serviceId = (int)selectedService.IdAdditionalService;
                var existingService = _additionalServices.FirstOrDefault(s => s.IdAdditionalService == serviceId);

                if (existingService == null)
                {
                    _additionalServices.Add(new AdditionalServiceView
                    {
                        IdAdditionalService = serviceId,
                        Name = selectedService.Name,
                        Quantity = quantity,
                        Cost = selectedService.Cost 
                    });
                }
                else
                {
                    existingService.Quantity = quantity;
                }

                UpdateAdditionalServicesListBox();
                UpdateTotalCost();
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите услугу и введите корректное количество.");
            }
        }


        private void RemoveAdditionalService_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                var serviceId = (int)button.Tag;
                var serviceToRemove = _additionalServices.SingleOrDefault(s => s.IdAdditionalService == serviceId);
                if (serviceToRemove != null)
                {
                    _additionalServices.Remove(serviceToRemove);
                    UpdateAdditionalServicesListBox();
                    UpdateTotalCost();
                }
            }
        }


        private void UpdateAdditionalServicesListBox()
        {
            AdditionalServicesListBox.ItemsSource = null;
            AdditionalServicesListBox.ItemsSource = _additionalServices;
        }


        private bool ValidateTime(string time)
        {
            if (string.IsNullOrWhiteSpace(time)) return true;
            return Regex.IsMatch(time, @"^([01]\d|2[0-3]):([0-5]\d)$");
        }

        private void TariffComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateTotalCost();
        }

        private void UpdateTotalCost()
        {
            decimal tariffCost = 0;
            if (TariffComboBox.SelectedItem != null)
            {
                var selectedTariff = (dynamic)TariffComboBox.SelectedItem;
                tariffCost = selectedTariff.Cost;
            }

            decimal additionalServicesCost = _additionalServices.Sum(s => s.Cost * s.Quantity);

            _order.TotalCost = (int)(tariffCost + additionalServicesCost);
            TotalCostTextBox.Text = _order.TotalCost.ToString();
        }

        private void AddTariffButton_Click(object sender, RoutedEventArgs e)
        {
            TariffCard card = new TariffCard();
            card.Closed += (s, args) => LoadComboBoxData();
            card.ShowDialog();
        }

        private void AddAdditionalServiceButton_Click(object sender, RoutedEventArgs e)
        {
            AdditionalServiceCard card = new AdditionalServiceCard();
            card.Closed += (s, args) => LoadComboBoxData();
            card.ShowDialog();
        }

        private void AddCustomerButton_Click(object sender, RoutedEventArgs e)
        {
            CustomerCard card = new CustomerCard();
            card.Closed += (s, args) => LoadComboBoxData();
            card.ShowDialog();
        }

        private void ShowFreeCar_Click(object sender, RoutedEventArgs e)
        {
            FreeCarsCard freeCarsCard = new FreeCarsCard();
            freeCarsCard.ShowDialog();
        }
    }
}
