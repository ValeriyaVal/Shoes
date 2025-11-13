using Microsoft.Win32;
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Shoes
{
    public partial class AddEditPage : Page
    {
        // Текущий редактируемый товар
        private Product _currentProduct = new Product();
        public AddEditPage() : this(null) { }

        private string newImagePath = "";
        public AddEditPage(Product selectedProduct)
        {
            InitializeComponent();

            // Если передан товар — редактируем, иначе создаём новый
            if (selectedProduct != null)
                _currentProduct = selectedProduct;

            // Привязка данных
            DataContext = _currentProduct;

            // Настройка ComboBox категорий и производителей
            ComboCategory.ItemsSource = new string[] { "Женская обувь", "Мужская обувь" };
            ComboManufacturer.ItemsSource = new string[] { "Rieker", "Alissio Nesca", "Kari", "Marco Tozzi", "CROSBY" };

            // Устанавливаем текущее значение, если редактируем
            if (!string.IsNullOrWhiteSpace(_currentProduct.ProductCategory))
                ComboCategory.SelectedItem = _currentProduct.ProductCategory;

            if (!string.IsNullOrWhiteSpace(_currentProduct.ProductManufacturer))
                ComboManufacturer.SelectedItem = _currentProduct.ProductManufacturer;
        }

        private void ComboCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboCategory.SelectedItem != null)
                _currentProduct.ProductCategory = ComboCategory.SelectedItem.ToString();
        }

        private void ComboManufacturer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboManufacturer.SelectedItem != null)
                _currentProduct.ProductManufacturer = ComboManufacturer.SelectedItem.ToString();
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder errors = new StringBuilder();

            // Проверки на заполненность и корректность
            if (string.IsNullOrWhiteSpace(_currentProduct.ProductName))
                errors.AppendLine("Укажите наименование товара");

            if (string.IsNullOrWhiteSpace(_currentProduct.ProductCategory))
                errors.AppendLine("Выберите категорию");

            if (string.IsNullOrWhiteSpace(_currentProduct.ProductManufacturer))
                errors.AppendLine("Выберите производителя");

            if (string.IsNullOrWhiteSpace(_currentProduct.ProductImporter))
                errors.AppendLine("Укажите поставщика");

            if (_currentProduct.PriductCost <= 0)
                errors.AppendLine("Укажите корректную цену");

            if (string.IsNullOrWhiteSpace(_currentProduct.ProductUnit))
                errors.AppendLine("Укажите единицу измерения (например: пара, шт.)");

            if (_currentProduct.ProductQuantityInStock < 0)
                errors.AppendLine("Количество на складе не может быть отрицательным");

            if (_currentProduct.ProductDiscount < 0 || _currentProduct.ProductDiscount > 100)
                errors.AppendLine("Скидка должна быть в пределах от 0 до 100%");

            if (string.IsNullOrWhiteSpace(_currentProduct.ProductDescription))
                errors.AppendLine("Укажите описание товара");

            // Если есть ошибки — показать пользователю
            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString(), "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Сохраняем изображение в папку проекта, если пользователь выбрал новое
            if (!string.IsNullOrWhiteSpace(newImagePath))
            {
                try
                {
                    string imagesFolder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
                    if (!System.IO.Directory.Exists(imagesFolder))
                    {
                        System.IO.Directory.CreateDirectory(imagesFolder);
                    }

                    string fileName = System.IO.Path.GetFileName(newImagePath);
                    string destPath = System.IO.Path.Combine(imagesFolder, fileName);

                    // Если файл с таким именем уже есть, добавляем уникальный суффикс
                    if (System.IO.File.Exists(destPath))
                    {
                        string nameWithoutExt = System.IO.Path.GetFileNameWithoutExtension(fileName);
                        string ext = System.IO.Path.GetExtension(fileName);
                        destPath = System.IO.Path.Combine(imagesFolder, nameWithoutExt + "_" + DateTime.Now.Ticks + ext);
                    }

                    System.IO.File.Copy(newImagePath, destPath);
                    _currentProduct.ProductPhoto = destPath; // сохраняем путь в базу
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при сохранении изображения:\n" + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }



            // Проверка на дублирование товара по названию и производителю
            var duplicate = ValievaShoesEntities.GetContext().Product
                .FirstOrDefault(p => p.ProductName == _currentProduct.ProductName
                                  && p.ProductManufacturer == _currentProduct.ProductManufacturer
                                  && p.ProductArticleNumber != _currentProduct.ProductArticleNumber);

            if (duplicate != null)
            {
                MessageBox.Show("Товар с таким названием и производителем уже существует.",
                                "Дубликат", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Если новый товар — добавляем в контекст
            if (string.IsNullOrWhiteSpace(_currentProduct.ProductArticleNumber))
            {
                // Генерация артикля (например: P + timestamp)
                _currentProduct.ProductArticleNumber = "P" + DateTime.Now.Ticks.ToString().Substring(8);
                ValievaShoesEntities.GetContext().Product.Add(_currentProduct);
            }

            // Сохраняем изменения
            try
            {
                ValievaShoesEntities.GetContext().SaveChanges();
                MessageBox.Show("Информация сохранена", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                Manager.MainFrame.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении данных:\n" + ex.Message,
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ChangePhotoBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.jpg; *.jpeg; *.png; *.bmp)|*.jpg; *.jpeg; *.png; *.bmp|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    //BitmapImage image = new BitmapImage(new Uri(openFileDialog.FileName));

                    // Загружаем изображение для предпросмотра
                    BitmapImage image = new BitmapImage();
                    image.BeginInit();
                    image.UriSource = new Uri(openFileDialog.FileName);
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.EndInit();

                    // Проверяем размер изображения
                    if (image.PixelWidth > 300 || image.PixelHeight > 200)
                    {
                        MessageBox.Show("Размер изображения должен быть не более 300x200 пикселей. Изображение будет автоматически уменьшено.");
                    }

                    // Временно отображаем выбранное изображение
                    PhotoImage.Source = new BitmapImage(new Uri(openFileDialog.FileName));
                    newImagePath = openFileDialog.FileName;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке изображения: {ex.Message}");
                }
            }
        }
    }
}
