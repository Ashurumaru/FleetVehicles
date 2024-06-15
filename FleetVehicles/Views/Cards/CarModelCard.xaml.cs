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
    /// Логика взаимодействия для CarModelCard.xaml
    /// </summary>
    public partial class CarModelCard : Window
    {
        private readonly FleetVehiclesEntities _context;
        private CarModel _carModel;
        private bool _isNewCarModel;

        public CarModelCard()
        {
            InitializeComponent();
            _context = new FleetVehiclesEntities();
            _carModel = new CarModel();
            DataContext = _carModel;
            LoadModelList();
        }

        private void LoadModelList()
        {
            BrandComboBox.ItemsSource = _context.CarBrand.Select(cm => new
            {
                cm.IdBrand,
                cm.Name
            }).ToList();

            var models = _context.CarModel.Select(cm => new
            {
                cm.IdModel,
                Display = cm.Name + " " + cm.CarBrand.Name,

            }).ToList();

            ModelListBox.ItemsSource = models;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ModelNameTextBox.Text) || BrandComboBox.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, заполните все обязательные поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _carModel.Name = ModelNameTextBox.Text;
            _carModel.IdBrand = (int)BrandComboBox.SelectedValue;

            _context.CarModel.Add(_carModel);
            _context.SaveChanges();
            MessageBox.Show("Модель успешно сохранена.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            LoadModelList();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void RemoveModel_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var modelId = (int)button.Tag;
            var modelToRemove = _context.CarModel.SingleOrDefault(cm => cm.IdModel == modelId);

            if (modelToRemove != null)
            {
                var relatedCars = _context.Cars.Any(c => c.IdModel == modelId);
                if (relatedCars)
                {
                    MessageBox.Show("Невозможно удалить модель, так как она используется в машинах.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                _context.CarModel.Remove(modelToRemove);
                _context.SaveChanges();
                LoadModelList();
                MessageBox.Show("Модель успешно удалена.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void AddBrandButton_Click(object sender, RoutedEventArgs e)
        {
            var brandCard = new CarBrandCard();
            brandCard.ShowDialog();
            LoadModelList();
        }
    }
}