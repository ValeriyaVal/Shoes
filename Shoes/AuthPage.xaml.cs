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
    /// Логика взаимодействия для AuthPage.xaml
    /// </summary>
    public partial class AuthPage : Page
    {
        public AuthPage()
        {
            InitializeComponent();

        }

        private void BtnGuest_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new ProductPage());
            TBLogin.Text = "";
            TBPassword.Text = "";

        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string login = TBLogin.Text;
            string password = TBPassword.Text;

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Есть пустые поля");
                return;
            }

            var user = ValievaShoesEntities.GetContext().User.
                FirstOrDefault(p => p.UserLogin == login && p.UserPassword == password);

            if (user != null)
            {
                Manager.MainFrame.Navigate(new ProductPage(user));
                TBLogin.Text = "";
                TBPassword.Text = "";
            }
            else
            {
                MessageBox.Show("Неверный логин или пароль");

            }
        }
    }
}
