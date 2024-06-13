﻿using FleetVehicles.Views.Pages;
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

namespace FleetVehicles.Views
{
    /// <summary>
    /// Логика взаимодействия для DashboardView.xaml
    /// </summary>
    public partial class DashboardView : Window
    {
        private int _currentUserId;
        public DashboardView(int currentUserId)
        {
            _currentUserId = currentUserId;
            InitializeComponent();
        }

        private void OrdersPage_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ManagementOrderPage(_currentUserId));
        }

        private void CarsPage_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ManagementCarPage(_currentUserId));

        }

        private void PersonalAccountPage_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ReportPage_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void btnLogOut_Click(object sender, RoutedEventArgs e)
        {
            LoginView loginView = new LoginView();
            loginView.Show();
            this.Close();
        }
    }
}
