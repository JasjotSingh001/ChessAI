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

namespace ChessWPF
{
    /// <summary>
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class HomePage : Page
    {
        private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public HomePage()
        {
            InitializeComponent();
        }
        private void StartLocalGame_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new LocalGamePage());
        }

        private void StartAIGame_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AIGamePage());
        }
    }
}
