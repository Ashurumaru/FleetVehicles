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
    /// Логика взаимодействия для OrderCard.xaml
    /// </summary>
    public partial class OrderCard : Window
    {
        private FleetVehiclesEntities _context;
        private Orders _order;
        private int _currentUserId;
        public OrderCard(int currentUserId, Orders order = null)
        {
            InitializeComponent();
            _currentUserId = currentUserId;
            _context = new FleetVehiclesEntities();
            _order = order ?? new Orders();
            //LoadData();
            DataContext = this;
        }
    }
}
