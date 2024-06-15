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
    /// Логика взаимодействия для ManagementCarPage.xaml
    /// </summary>
    public partial class ManagementCarPage : Page
    {
        private int _currentUserId;

        public ManagementCarPage(int currentUserId)
        {
            InitializeComponent();
            LoadFleetCarsData();
            _currentUserId = currentUserId;
        }

        private void LoadFleetCarsData(string searchQuery = "")
        {
            using (var context = new FleetVehiclesEntities())
            {
                var fleetCars = (from fleetCar in context.FleetCars
                                 join car in context.Cars on fleetCar.IdCar equals car.IdCar
                                 join carModel in context.CarModel on car.IdModel equals carModel.IdModel
                                 join carBrand in context.CarBrand on carModel.IdBrand equals carBrand.IdBrand
                                 join driver in context.Employees on fleetCar.IdDriver equals driver.IdEmployee
                                 join color in context.CarColor on fleetCar.IdColor equals color.IdColor
                                 select new
                                 {
                                     FleetCarID = fleetCar.IdFleetCar,
                                     CarModel = carModel.Name,
                                     CarBrand = carBrand.Name,
                                     DriverFirstName = driver.FirstName,
                                     DriverLastName = driver.LastName,
                                     DriverPatronymic = driver.Patronymic,
                                     VinNumber = fleetCar.VinNumber,
                                     RegistrationNumber = fleetCar.RegistrationNumber,
                                     ColorName = color.Name
                                 }).ToList();

                if (!string.IsNullOrWhiteSpace(searchQuery))
                {
                    searchQuery = searchQuery.ToLower();
                    fleetCars = fleetCars.Where(fc =>
                        fc.CarModel.ToLower().Contains(searchQuery) ||
                        fc.CarBrand.ToLower().Contains(searchQuery) ||
                        fc.DriverFirstName.ToLower().Contains(searchQuery) ||
                        fc.DriverLastName.ToLower().Contains(searchQuery) ||
                        fc.VinNumber.ToLower().Contains(searchQuery) ||
                        fc.RegistrationNumber.ToLower().Contains(searchQuery) ||
                        fc.ColorName.ToLower().Contains(searchQuery)
                    ).ToList();
                }

                var fleetCarViews = fleetCars.Select(fc => new FleetCarView
                {
                    FleetCarID = fc.FleetCarID,
                    CarInfo = fc.CarBrand + " " + fc.CarModel,
                    DriverName = fc.DriverFirstName + " " + fc.DriverLastName + " " + fc.DriverLastName,
                    VinNumber = fc.VinNumber,
                    RegistrationNumber = fc.RegistrationNumber,
                    ColorName = fc.ColorName
                }).ToList();

                FleetCarList.ItemsSource = fleetCarViews;
            }
        }

        private void SearchFleetCarTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadFleetCarsData(SearchFleetCarTextBox.Text);
        }

        private void ResetFleetCarSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchFleetCarTextBox.Text = string.Empty;
            LoadFleetCarsData();
        }

        private void ShowFleetCarCard_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                var fleetCar = button.CommandParameter as FleetCarView;
                if (fleetCar != null)
                {
                    FleetCarCard card = new FleetCarCard(_currentUserId, fleetCar.FleetCarID);
                    card.Closed += Card_Closed;
                    card.Show();
                }
                else
                {
                    FleetCarCard card = new FleetCarCard(_currentUserId, null);
                    card.Closed += Card_Closed;
                    card.Show();
                }
            }
        }

        private void Card_Closed(object sender, EventArgs e)
        {
            LoadFleetCarsData();
        }

        private void btnCreateFleetCar_Click(object sender, RoutedEventArgs e)
        {
            FleetCarCard card = new FleetCarCard(_currentUserId, null);
            card.Closed += Card_Closed;
            card.Show();
        }

        private void DeleteCar_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var fleetCar = button?.CommandParameter as FleetCarView;
            if (fleetCar != null)
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить машину?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    using (var context = new FleetVehiclesEntities())
                    {
                        var fleetCarToRemove = context.FleetCars.SingleOrDefault(fc => fc.IdFleetCar == fleetCar.FleetCarID);
                        if (fleetCarToRemove != null)
                        {
                            var relatedOrders = context.Orders.Where(o => o.IdFleetCar == fleetCarToRemove.IdFleetCar).ToList();
                            if (relatedOrders.Any())
                            {
                                MessageBox.Show("Машина не может быть удалена, так как она участвует в одном или нескольких заказах.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }

                            context.FleetCars.Remove(fleetCarToRemove);
                            context.SaveChanges();
                            MessageBox.Show("Машина успешно удалена.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                            LoadFleetCarsData();
                        }
                        else
                        {
                            MessageBox.Show("Машина не найдена.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
        }
    }
}