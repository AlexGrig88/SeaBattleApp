using SeaBattleApp;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace SeaBattleGUI.Views
{
    /// <summary>
    /// Interaction logic for StatisticsUserControl.xaml
    /// </summary>
    public partial class StatisticsUserControl : UserControl
    {
		private MainWindow? _mainWindow;

		public StatisticsUserControl()
        {
			_mainWindow = Application.Current.MainWindow as MainWindow;
			InitializeComponent();
		}

		private void ButtonReturnFromStatistics_Click(object sender, RoutedEventArgs e)
		{
			
			if (_mainWindow != null) {
                this.Visibility = Visibility.Collapsed;
                _mainWindow.PlayingField.Visibility = Visibility.Collapsed;
                _mainWindow.StartingField.Visibility = Visibility.Visible;
			}
			
		}
    }
}
