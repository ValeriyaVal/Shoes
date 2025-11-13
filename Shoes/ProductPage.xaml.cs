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
using System.Windows.Navigation;
using System.Windows.Shapes;


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
            Wrp.Visibility = Visibility.Hidden;
            WrpNoClient.Visibility = Visibility.Hidden;

        }

        public ProductPage(User user)
        {
            InitializeComponent();
            _user = user;

            FIOTB.Text = user.UserLastName + " " + user.UserFirstName + " " + user.UserPatronymic;
            if (user.RoleID==1)
                WrpNoClient.Visibility = Visibility.Hidden;
            //switch (user.RoleID)
            //{
            //    case 1:
            //        //RoleTB.Text = "Авторизованный клиент"; 
            //        WrpNoClient.Visibility = Visibility.Hidden; break;
            //    case 2:
            //        RoleTB.Text = "Менеджер"; break;
            //    case 3:
            //        RoleTB.Text = "Администратор"; break;
            //}

            //ComboType.SelectedIndex = 0;


        }
        private void UpdateProducts()
        {
            var currentProducts = ValievaShoesEntities.GetContext().Product.ToList();

            if (ComboType.SelectedIndex == 0 || ComboType.SelectedIndex < 0) { }
            else if (ComboType.SelectedIndex == 1)
                currentProducts = currentProducts.Where(p => p.ProductImporter== "Kari").ToList();
            else if (ComboType.SelectedIndex == 2)
                currentProducts = currentProducts.Where(p => p.ProductImporter == "Обувь для вас").ToList();

            currentProducts = currentProducts.Where(p => p.ProductName.ToLower().Contains(TBoxSearch.Text.ToLower()) 
            || p.ProductArticleNumber.ToLower().Contains(TBoxSearch.Text.ToLower()) 
            || p.ProductManufacturer.ToLower().Contains(TBoxSearch.Text.ToLower()) 
            || p.ProductImporter.ToLower().Contains(TBoxSearch.Text.ToLower())
            || p.ProductCategory.ToLower().Contains(TBoxSearch.Text.ToLower())
            || p.ProductDescription.ToLower().Contains(TBoxSearch.Text.ToLower())).ToList();


            ProductListView.ItemsSource = currentProducts.ToList();

            if (RBtnDown.IsChecked.Value)
                currentProducts = currentProducts.OrderByDescending(p=>p.ProductQuantityInStock).ToList();
            if (RBtnUp.IsChecked.Value)
                currentProducts = currentProducts.OrderBy(p => p.ProductQuantityInStock).ToList();

            ProductListView.ItemsSource = currentProducts;
            

        }



        private void TBoxSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateProducts();
        }

        private void ComboType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateProducts();
        }

        private void RBtnUp_Checked(object sender, RoutedEventArgs e)
        {
            UpdateProducts();
        }

        private void RBtnDown_Checked(object sender, RoutedEventArgs e)
        {
            UpdateProducts();
        }

        private void EditBtn_Click(object sender, RoutedEventArgs e)
        {
            var selectedProduct = (sender as Button).DataContext as Product;
            Manager.MainFrame.Navigate(new AddEditPage(selectedProduct));

        }

        private void DelBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BntAdd_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new AddEditPage(null));
        }
    }
}
