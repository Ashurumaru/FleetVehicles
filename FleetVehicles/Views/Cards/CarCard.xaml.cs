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
    /// Логика взаимодействия для CarCard.xaml
    /// </summary>
    public partial class CarCard : Window
    {
        private FleetVehiclesEntities _context;
        private FleetCars _car;
        private int _currentUserId;
        public CarCard(int currentUserId, FleetCars car = null)
        {
            InitializeComponent();
            _currentUserId = currentUserId;
            _context = new FleetVehiclesEntities();
            _car = car ?? new FleetCars();
            //LoadData();
            DataContext = this;
        }
    }
}
