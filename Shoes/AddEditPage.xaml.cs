using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace Shoes
{
    public partial class AddEditPage : Page
    {
        // Текущий редактируемый товар
        private Product _currentProduct = new Product();
        public AddEditPage() : this(null) { }

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
    }
}
