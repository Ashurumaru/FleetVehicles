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
    /// Логика взаимодействия для ColorCard.xaml
    /// </summary>
    public partial class ColorCard : Window
    {
        private readonly FleetVehiclesEntities _context;
        private CarColor _carColor;
        private bool _isNewCarColor;

        public ColorCard()
        {
            InitializeComponent();
            _context = new FleetVehiclesEntities();
            _carColor = new CarColor();
            DataContext = _carColor;
            LoadColorList();
        }

        private void LoadColorList()
        {
            var colors = _context.CarColor.Select(c => new
            {
                c.IdColor,
                Display = c.Name
            }).ToList();

            ColorListBox.ItemsSource = colors;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ColorNameTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, заполните все обязательные поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _carColor.Name = ColorNameTextBox.Text;

            _context.CarColor.Add(_carColor);
            _context.SaveChanges();
            MessageBox.Show("Цвет машины успешно сохранен.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            LoadColorList();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void RemoveColor_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var colorId = (int)button.Tag;
            var colorToRemove = _context.CarColor.SingleOrDefault(c => c.IdColor == colorId);

            if (colorToRemove != null)
            {
                var relatedCars = _context.FleetCars.Any(fc => fc.IdColor == colorId);
                if (relatedCars)
                {
                    MessageBox.Show("Невозможно удалить цвет, так как он используется в машинах.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                _context.CarColor.Remove(colorToRemove);
                _context.SaveChanges();
                LoadColorList();
                MessageBox.Show("Цвет успешно удален.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}