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
    /// Логика взаимодействия для CarBrandCard.xaml
    /// </summary>
    public partial class CarBrandCard : Window
    {
        private readonly FleetVehiclesEntities _context;
        private CarBrand _carBrand;

        public CarBrandCard()
        {
            InitializeComponent();
            _context = new FleetVehiclesEntities();
            _carBrand = new CarBrand();
            DataContext = _carBrand;
            LoadCarBrandList();
        }

        private void LoadCarBrandList()
        {
            var carBrands = _context.CarBrand.Select(cb => new
            {
                cb.IdBrand,
                cb.Name
            }).ToList();

            CarBrandListBox.ItemsSource = carBrands;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(BrandNameTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, введите название бренда.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _carBrand.Name = BrandNameTextBox.Text;

            _context.CarBrand.Add(_carBrand);
            _context.SaveChanges();
            MessageBox.Show("Бренд автомобиля успешно сохранен.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            LoadCarBrandList();
            ClearForm();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void RemoveCarBrand_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var brandId = (int)button.Tag;
            var brandToRemove = _context.CarBrand.SingleOrDefault(cb => cb.IdBrand == brandId);

            if (brandToRemove != null)
            {
                var relatedCarModels = _context.CarModel.Any(cm => cm.IdBrand == brandId);
                if (relatedCarModels)
                {
                    MessageBox.Show("Невозможно удалить бренд, так как он используется в моделях автомобилей.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                _context.CarBrand.Remove(brandToRemove);
                _context.SaveChanges();
                LoadCarBrandList();
                MessageBox.Show("Бренд автомобиля успешно удален.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ClearForm()
        {
            BrandNameTextBox.Text = string.Empty;
            _carBrand = new CarBrand();
            DataContext = _carBrand;
        }
    }
}