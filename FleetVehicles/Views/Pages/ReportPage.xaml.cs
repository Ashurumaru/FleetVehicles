using FleetVehicles.Data;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Xceed.Words.NET;
using Xceed.Document.NET;

namespace FleetVehicles.Views.Pages
{
    public partial class ReportPage : Page
    {
        public ReportPage()
        {
            InitializeComponent();
            LoadEmployees();
        }

        private void LoadEmployees()
        {
            using (var context = new FleetVehiclesEntities())
            {
                var employees = context.Employees.Select(e => new
                {
                    e.IdEmployee,
                    FullName = e.FirstName + " " + e.LastName
                }).ToList();

                EmployeeComboBox.ItemsSource = employees;
                EmployeeComboBox.SelectedIndex = -1;
            }
        }

        private void GenerateOrderReportButton_Click(object sender, RoutedEventArgs e)
        {
            DateTime? startDate = StartDatePicker.SelectedDate;
            DateTime? endDate = EndDatePicker.SelectedDate;
            var selectedEmployee = EmployeeComboBox.SelectedItem as dynamic;
            int? employeeId = selectedEmployee?.IdEmployee;

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Word Document (*.docx)|*.docx",
                Title = "Сохранить отчет по заказам",
                FileName = "OrderReport.docx"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                GenerateOrderReport(startDate, endDate, employeeId, saveFileDialog.FileName);
            }
        }

        private void GenerateOrderReport(DateTime? startDate, DateTime? endDate, int? employeeId, string filePath)
        {
            using (var context = new FleetVehiclesEntities())
            {
                var ordersQuery = context.Orders.AsQueryable();

                if (startDate.HasValue)
                {
                    ordersQuery = ordersQuery.Where(o => o.DateStart >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    ordersQuery = ordersQuery.Where(o => o.DateEnd <= endDate.Value);
                }

                if (employeeId.HasValue)
                {
                    ordersQuery = ordersQuery.Where(o => o.IdDispatcher == employeeId.Value || o.FleetCars.IdDriver == employeeId.Value);
                }

                var orders = ordersQuery
                    .Select(o => new
                    {
                        o.DateStart,
                        o.DateEnd,
                        CustomerName = o.Customers.PhoneNumber,
                        o.DepartureAddress,
                        o.ArrivalAddress,
                        CarInfo = o.FleetCars.Cars.CarModel.Name + " " + o.FleetCars.Cars.CarModel.CarBrand.Name,
                        DriverName = o.FleetCars.Employees.FirstName + " " + o.FleetCars.Employees.LastName,
                        DispatcherName = o.Employees.FirstName + " " + o.Employees.LastName,
                        TariffName = o.Tariff.Name,
                        o.TotalCost,
                        AdditionalServices = o.OrderAdditionalService.Select(os => os.AdditionalService.Name)
                    })
                    .ToList();

                var orderList = orders
                    .Select(o => new
                    {
                        o.DateStart,
                        o.DateEnd,
                        o.CustomerName,
                        o.DepartureAddress,
                        o.ArrivalAddress,
                        o.CarInfo,
                        o.DriverName,
                        o.DispatcherName,
                        o.TariffName,
                        o.TotalCost,
                        AdditionalServices = string.Join(", ", o.AdditionalServices)
                    })
                    .ToList();

                using (var doc = DocX.Create(filePath))
                {
                    doc.InsertParagraph("Отчет по заказам")
                        .FontSize(16)
                        .Bold()
                        .Alignment = Alignment.center;

                    doc.InsertParagraph($"Дата создания отчета: {DateTime.Now:«dd»  MMMM  yyyy}")
                        .FontSize(12)
                        .Italic()
                        .Alignment = Alignment.center;

                    doc.AddFooters();
                    doc.Footers.Odd.InsertParagraph("Автопарк «Копейка»")
                        .FontSize(12)
                        .Alignment = Alignment.center;

                    if (orderList.Any())
                    {
                        var table = doc.AddTable(orderList.Count + 2, 11);
                        table.Alignment = Alignment.center;
                        table.Design = TableDesign.TableGrid;
                        table.Rows[0].Cells[0].Paragraphs.First().Append("Дата начала").Bold();
                        table.Rows[0].Cells[1].Paragraphs.First().Append("Дата окончания").Bold();
                        table.Rows[0].Cells[2].Paragraphs.First().Append("Клиент").Bold();
                        table.Rows[0].Cells[3].Paragraphs.First().Append("Адрес отправления").Bold();
                        table.Rows[0].Cells[4].Paragraphs.First().Append("Адрес прибытия").Bold();
                        table.Rows[0].Cells[5].Paragraphs.First().Append("Машина").Bold();
                        table.Rows[0].Cells[6].Paragraphs.First().Append("Водитель").Bold();
                        table.Rows[0].Cells[7].Paragraphs.First().Append("Диспетчер").Bold();
                        table.Rows[0].Cells[8].Paragraphs.First().Append("Тариф").Bold();
                        table.Rows[0].Cells[9].Paragraphs.First().Append("Дополнительные услуги").Bold();
                        table.Rows[0].Cells[10].Paragraphs.First().Append("Общая сумма").Bold();

                        decimal totalSum = 0;
                        for (int i = 0; i < orderList.Count; i++)
                        {
                            var order = orderList[i];
                            table.Rows[i + 1].Cells[0].Paragraphs.First().Append(order.DateStart.ToString());
                            table.Rows[i + 1].Cells[1].Paragraphs.First().Append(order.DateEnd.HasValue ? order.DateEnd.Value.ToString() : "В процессе");
                            table.Rows[i + 1].Cells[2].Paragraphs.First().Append(order.CustomerName);
                            table.Rows[i + 1].Cells[3].Paragraphs.First().Append(order.DepartureAddress);
                            table.Rows[i + 1].Cells[4].Paragraphs.First().Append(order.ArrivalAddress);
                            table.Rows[i + 1].Cells[5].Paragraphs.First().Append(order.CarInfo);
                            table.Rows[i + 1].Cells[6].Paragraphs.First().Append(order.DriverName);
                            table.Rows[i + 1].Cells[7].Paragraphs.First().Append(order.DispatcherName);
                            table.Rows[i + 1].Cells[8].Paragraphs.First().Append(order.TariffName);
                            table.Rows[i + 1].Cells[9].Paragraphs.First().Append(order.AdditionalServices);
                            table.Rows[i + 1].Cells[10].Paragraphs.First().Append(order.TotalCost.HasValue ? order.TotalCost.Value.ToString() : "0.00");
                            totalSum += order.TotalCost.HasValue ? order.TotalCost.Value : 0;
                        }

                        table.Rows[orderList.Count + 1].Cells[0].Paragraphs.First().Append("ИТОГО").Bold();
                        table.Rows[orderList.Count + 1].Cells[10].Paragraphs.First().Append(totalSum.ToString("C")).Bold();

                        doc.InsertTable(table);
                    }
                    else
                    {
                        doc.InsertParagraph("За выбранный период заказы отсутствуют.");
                    }

                    doc.InsertParagraph("\n\n\n\n");
                    doc.InsertParagraph($"«__» ______ {DateTime.Now.Year}                                            _____/_____________").Alignment = Alignment.left;


                    doc.Save();
                    MessageBox.Show("Отчет по заказам успешно создан.");
                }
            }
        }

        private void GenerateFleetReportButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Word Document (*.docx)|*.docx",
                Title = "Сохранить отчет по автопарку",
                FileName = "FleetReport.docx"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                GenerateFleetReport(saveFileDialog.FileName);
            }
        }

        private void GenerateFleetReport(string filePath)
        {
            using (var doc = DocX.Create(filePath))
            {
                doc.InsertParagraph("Отчет по заказам")
                       .FontSize(16)
                       .Bold()
                       .Alignment = Alignment.center;

                doc.InsertParagraph($"Дата создания отчета: {DateTime.Now:«dd»  MMMM  yyyy}")
                    .FontSize(12)
                    .Italic()
                    .Alignment = Alignment.center;

                doc.AddFooters();
                doc.Footers.Odd.InsertParagraph("Автопарк «Копейка»")
                    .FontSize(12)
                    .Alignment = Alignment.center;

                using (var context = new FleetVehiclesEntities())
                {
                    var fleetCars = context.FleetCars.Select(fc => new
                    {
                        CarInfo = fc.Cars.CarModel.CarBrand.Name + " " + fc.Cars.CarModel.Name,
                        DriverName = fc.Employees.FirstName + " " + fc.Employees.LastName,
                        fc.VinNumber,
                        fc.RegistrationNumber,
                        ColorName = fc.CarColor.Name
                    }).ToList();

                    if (fleetCars.Any())
                    {
                        var table = doc.AddTable(fleetCars.Count + 2, 5);
                        table.Alignment = Alignment.center;
                        table.Design = TableDesign.TableGrid;

                        table.Rows[0].Cells[0].Paragraphs.First().Append("Машина").Bold();
                        table.Rows[0].Cells[1].Paragraphs.First().Append("Водитель").Bold();
                        table.Rows[0].Cells[2].Paragraphs.First().Append("VIN номер").Bold();
                        table.Rows[0].Cells[3].Paragraphs.First().Append("Регистрационный номер").Bold();
                        table.Rows[0].Cells[4].Paragraphs.First().Append("Цвет").Bold();

                        int carCount = fleetCars.Count;
                        for (int i = 0; i < fleetCars.Count; i++)
                        {
                            var car = fleetCars[i];
                            table.Rows[i + 1].Cells[0].Paragraphs.First().Append(car.CarInfo);
                            table.Rows[i + 1].Cells[1].Paragraphs.First().Append(car.DriverName);
                            table.Rows[i + 1].Cells[2].Paragraphs.First().Append(car.VinNumber);
                            table.Rows[i + 1].Cells[3].Paragraphs.First().Append(car.RegistrationNumber);
                            table.Rows[i + 1].Cells[4].Paragraphs.First().Append(car.ColorName);
                        }

                        table.Rows[fleetCars.Count + 1].Cells[0].Paragraphs.First().Append("ИТОГО").Bold();
                        table.Rows[fleetCars.Count + 1].Cells[4].Paragraphs.First().Append(carCount.ToString()).Bold();

                        doc.InsertTable(table);
                    }
                    else
                    {
                        doc.InsertParagraph("Нет данных об автомобилях для отображения.");
                    }
                }

                doc.InsertParagraph("\n\n\n\n");
                doc.InsertParagraph($"«__» ______ {DateTime.Now.Year}                                          _____/_____________").Alignment = Alignment.left;


                doc.Save();
                MessageBox.Show("Отчет по автопарку успешно создан.");
            }
        }
    }
}