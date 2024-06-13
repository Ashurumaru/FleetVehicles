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

        private void LoadFleetCarsData()
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
                                     VinNumber = fleetCar.VinNumber,
                                     RegistrationNumber = fleetCar.RegistrationNumber,
                                     ColorName = color.Name
                                 }).ToList()
                                 .Select(fc => new FleetCarView
                                 {
                                     FleetCarID = fc.FleetCarID,
                                     CarInfo = fc.CarBrand + " " + fc.CarModel,
                                     DriverName = fc.DriverFirstName + " " + fc.DriverLastName,
                                     VinNumber = fc.VinNumber,
                                     RegistrationNumber = fc.RegistrationNumber,
                                     ColorName = fc.ColorName
                                 }).ToList();

                FleetCarList.ItemsSource = fleetCars;
            }
        }

        private void ShowFleetCarCard_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                var fleetCar = button.CommandParameter as FleetCarView;
                if (fleetCar != null)
                {
                    using (var context = new FleetVehiclesEntities())
                    {
                        var fleetCarData = context.FleetCars.SingleOrDefault(fc => fc.IdFleetCar == fleetCar.FleetCarID);
                        if (fleetCarData != null)
                        {
                            CarCard card = new CarCard(_currentUserId, fleetCarData);
                            card.Closed += Card_Closed;
                            card.Show();
                        }
                        else
                        {
                            MessageBox.Show("Автомобиль не найден.");
                        }
                    }
                }
                else
                {
                    CarCard card = new CarCard(_currentUserId);
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
            CarCard card = new CarCard(_currentUserId);
            card.Closed += Card_Closed;
            card.Show();
        }
    }
}
