using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Shoes
{
    /// <summary>
    /// Логика взаимодействия для ProductPage.xaml
    /// </summary>
    public partial class ProductPage : Page
    {
        private User _user;

        public ProductPage()
        {
            InitializeComponent();

            var currentProducts = ValievaShoesEntities.GetContext().Product.ToList();
            ProductListView.ItemsSource = currentProducts;

            UpdateProducts();

            // Если пользователь не авторизован (гость)
           // BntAdd.Visibility = Visibility.Collapsed;
            Wrp.Visibility = Visibility.Hidden;
            WrpNoClient.Visibility = Visibility.Hidden;
        }

        public ProductPage(User user)
        {
            InitializeComponent();
            _user = user;

            FIOTB.Text = $"{user.UserLastName} {user.UserFirstName} {user.UserPatronymic}";

            ApplyRolePermissions();
            UpdateProducts();
        }

        private void ApplyRolePermissions()
        {
            //if (_user == null || _user.RoleID == 1)
            //{
            //    // Нет пользователя — скрываем добавление
            //    BntAdd.Visibility = Visibility.Collapsed;
            //    WrpNoClient.Visibility = Visibility.Hidden;
            //}

            //// Кнопка "Добавить"
            //BntAdd.Visibility = (_user.RoleID == 2) ? Visibility.Visible : Visibility.Collapsed;

            //// Кнопки "Редактировать"/"Удалить" в ListView
            //ProductListView.ItemContainerGenerator.StatusChanged += (s, e) =>
            //{
            //    if (ProductListView.ItemContainerGenerator.Status != System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
            //        return;

            //    foreach (var item in ProductListView.Items)
            //    {
            //        var container = ProductListView.ItemContainerGenerator.ContainerFromItem(item) as ListViewItem;
            //        if (container != null)
            //        {
            //            var delEditPanel = FindVisualChild<StackPanel>(container, "DelEdit");
            //            if (delEditPanel != null)
            //            {
            //                delEditPanel.Visibility = (_user.RoleID == 2) ? Visibility.Visible : Visibility.Collapsed;
            //            }
            //        }
            //    }
            //};
            if (_user == null || _user.RoleID == 1)
            {
                // Гость или обычный пользователь
                BntAdd.Visibility = Visibility.Collapsed;
                WrpNoClient.Visibility = Visibility.Collapsed; // Скрываем панель поиска/фильтров

                // Скрываем кнопки редактирования/удаления
                ProductListView.ItemContainerGenerator.StatusChanged += (s, e) =>
                {
                    if (ProductListView.ItemContainerGenerator.Status != System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
                        return;

                    foreach (var item in ProductListView.Items)
                    {
                        var container = ProductListView.ItemContainerGenerator.ContainerFromItem(item) as ListViewItem;
                        if (container != null)
                        {
                            var delEditPanel = FindVisualChild<StackPanel>(container, "DelEdit");
                            if (delEditPanel != null)
                                delEditPanel.Visibility = Visibility.Collapsed;
                        }
                    }
                };
            }
            else if (_user.RoleID == 2)
            {
                // Администратор — всё доступно
                BntAdd.Visibility = Visibility.Visible;
                WrpNoClient.Visibility = Visibility.Visible;

                ProductListView.ItemContainerGenerator.StatusChanged += (s, e) =>
                {
                    if (ProductListView.ItemContainerGenerator.Status != System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
                        return;

                    foreach (var item in ProductListView.Items)
                    {
                        var container = ProductListView.ItemContainerGenerator.ContainerFromItem(item) as ListViewItem;
                        if (container != null)
                        {
                            var delEditPanel = FindVisualChild<StackPanel>(container, "DelEdit");
                            if (delEditPanel != null)
                                delEditPanel.Visibility = Visibility.Visible;
                        }
                    }
                };
            }
            else if (_user.RoleID == 3)
            {
                // Менеджер — фильтры видны, кнопки добавления и редактирования скрыты
                BntAdd.Visibility = Visibility.Collapsed;
                WrpNoClient.Visibility = Visibility.Visible;

                ProductListView.ItemContainerGenerator.StatusChanged += (s, e) =>
                {
                    if (ProductListView.ItemContainerGenerator.Status != System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
                        return;

                    foreach (var item in ProductListView.Items)
                    {
                        var container = ProductListView.ItemContainerGenerator.ContainerFromItem(item) as ListViewItem;
                        if (container != null)
                        {
                            var delEditPanel = FindVisualChild<StackPanel>(container, "DelEdit");
                            if (delEditPanel != null)
                                delEditPanel.Visibility = Visibility.Collapsed;
                        }
                    }
                };
            }
        }

        // Универсальный метод поиска визуального дочернего элемента по имени
        private T FindVisualChild<T>(DependencyObject parent, string name) where T : FrameworkElement
        {
            if (parent == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T tChild && tChild.Name == name)
                    return tChild;

                var result = FindVisualChild<T>(child, name);
                if (result != null) return result;
            }

            return null;
        }

        private void UpdateProducts()
        {
            var currentProducts = ValievaShoesEntities.GetContext().Product.ToList();

            if (ComboType.SelectedIndex == 1)
                currentProducts = currentProducts.Where(p => p.ProductImporter == "Kari").ToList();
            else if (ComboType.SelectedIndex == 2)
                currentProducts = currentProducts.Where(p => p.ProductImporter == "Обувь для вас").ToList();

            currentProducts = currentProducts.Where(p =>
                p.ProductName.ToLower().Contains(TBoxSearch.Text.ToLower()) ||
                p.ProductArticleNumber.ToLower().Contains(TBoxSearch.Text.ToLower()) ||
                p.ProductManufacturer.ToLower().Contains(TBoxSearch.Text.ToLower()) ||
                p.ProductImporter.ToLower().Contains(TBoxSearch.Text.ToLower()) ||
                p.ProductCategory.ToLower().Contains(TBoxSearch.Text.ToLower()) ||
                p.ProductDescription.ToLower().Contains(TBoxSearch.Text.ToLower())
            ).ToList();

            if (RBtnDown.IsChecked == true)
                currentProducts = currentProducts.OrderByDescending(p => p.ProductQuantityInStock).ToList();
            if (RBtnUp.IsChecked == true)
                currentProducts = currentProducts.OrderBy(p => p.ProductQuantityInStock).ToList();

            ProductListView.ItemsSource = currentProducts;
        }

        private void TBoxSearch_TextChanged(object sender, TextChangedEventArgs e) => UpdateProducts();
        private void ComboType_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateProducts();
        private void RBtnUp_Checked(object sender, RoutedEventArgs e) => UpdateProducts();
        private void RBtnDown_Checked(object sender, RoutedEventArgs e) => UpdateProducts();

        private void EditBtn_Click(object sender, RoutedEventArgs e)
        {
            var selectedProduct = (sender as Button).DataContext as Product;
            Manager.MainFrame.Navigate(new AddEditPage(selectedProduct));
        }

        private void DelBtn_Click(object sender, RoutedEventArgs e)
        {
            var currentProduct = (sender as Button).DataContext as Product;

            var currentOrderProducts = ValievaShoesEntities.GetContext().OrderProduct
                .Where(op => op.ProductArticleNumber == currentProduct.ProductArticleNumber).ToList();

            if (currentOrderProducts.Count > 0)
            {
                MessageBox.Show("Невозможно удалить запись о товаре, т.к. существуют записи о заказах с ним");
                return;
            }

            if (MessageBox.Show("Вы точно хотите выполнить удаление?", "Внимание!",
                MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    ValievaShoesEntities.GetContext().Product.Remove(currentProduct);
                    ValievaShoesEntities.GetContext().SaveChanges();
                    UpdateProducts();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }

        private void BntAdd_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new AddEditPage(null));
        }

        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UpdateProducts();
        }
    }
}
