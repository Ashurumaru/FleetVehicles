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
    /// Логика взаимодействия для CarBodyCard.xaml
    /// </summary>
    public partial class CarBodyCard : Window
    {
        private readonly FleetVehiclesEntities _context;
        private BodyType _carBody;
        private bool _isNewCarBody;

        public CarBodyCard()
        {
            InitializeComponent();
            _context = new FleetVehiclesEntities();
            _carBody = new BodyType();
            DataContext = _carBody;
            LoadBodyList();
        }

        private void LoadBodyList()
        {
            var bodies = _context.BodyType.Select(cb => new
            {
                cb.IdBody,
                Display = cb.Name
            }).ToList();

            BodyListBox.ItemsSource = bodies;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(BodyNameTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, заполните все обязательные поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _carBody.Name = BodyNameTextBox.Text;

            _context.BodyType.Add(_carBody);
            _context.SaveChanges();
            MessageBox.Show("Тип кузова успешно сохранен.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            LoadBodyList();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void RemoveBody_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var bodyId = (int)button.Tag;
            var bodyToRemove = _context.BodyType.SingleOrDefault(b => b.IdBody == bodyId);

            if (bodyToRemove != null)
            {
                var relatedCars = _context.Cars.Any(c => c.IdBody == bodyId);
                if (relatedCars)
                {
                    MessageBox.Show("Невозможно удалить тип кузова, так как он используется в машинах.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                _context.BodyType.Remove(bodyToRemove);
                _context.SaveChanges();
                LoadBodyList();
                MessageBox.Show("Тип кузова успешно удален.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}