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

namespace FleetVehicles.Views
{
    /// <summary>
    /// Логика взаимодействия для LoginView.xaml
    /// </summary>
    public partial class LoginView : Window
    {
        private string _password;
        public LoginView()
        {
            InitializeComponent();
        }

        private void LogIn_Click(object sender, RoutedEventArgs e)
        {
            string login = txLogin.Text;
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(_password))
            {
                ErroeMessage.Text = "Введите логин и пароль.";
                return;
            }

            try
            {
                using (var context = new FleetVehiclesEntities())
                {
                    var user = context.Employees.SingleOrDefault(u => u.Login == login && u.Password == _password);
                    if (user != null)
                    {
                        DashboardView dashbord = new DashboardView(user.IdEmployee);
                        MessageBox.Show($"Вы зашли под учетной записью {user.FirstName} {user.LastName} {user.Position.Name}", "Добро пожаловать");
                        dashbord.Show();
                        this.Close();
                    }
                    else
                    {
                        ErroeMessage.Text = "Неверный логин или пароль.";
                        ErroeMessage.Opacity = 1;
                    }
                }
            }
            catch (Exception ex)
            {
                ErroeMessage.Text = "Ошибка: " + ex.Message;
                ErroeMessage.Opacity = 1;
            }
        }

        private void password_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var passwordBox = (PasswordBox)sender;
            _password = passwordBox.Password;
        }
    }
}
