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
using FleetVehicles.Views.Pages;

namespace FleetVehicles.Views.Cards
{
    /// <summary>
    /// Логика взаимодействия для FreeCarsCard.xaml
    /// </summary>
    public partial class FreeCarsCard : Window
    {
        public FreeCarsCard()
        {
            InitializeComponent();
            Frame.Navigate(new ManagementCarPage(3, true));
        }
    }
}
