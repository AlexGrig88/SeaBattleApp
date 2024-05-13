using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SeaBattleGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(object? sender, CancelEventArgs e)
        {
            string msg = "Вы действительно хотите закрыть программу?";
            var result = MessageBox.Show(msg, "MyApp", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.No) {
                e.Cancel = true;
            }
        }

        private void MenuExit_Close(object sender, RoutedEventArgs e) => Close();

        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            PlayingField.Visibility = Visibility.Visible;
            StartingField.Visibility = Visibility.Collapsed;
        }
    }
}