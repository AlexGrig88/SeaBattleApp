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
        private const string ABOUT = @"Классический морской бой.
Правила размещения кораблей (флота):
Игровое поле — обычно квадрат 10×10 у каждого игрока, на котором размещается флот кораблей. Горизонтали обычно нумеруются сверху вниз, а вертикали помечаются буквами слева направо. При этом используются буквы русского алфавита от «А» до «К» (буквы «Ё» и «Й» обычно пропускаются). Размещаются:

1 корабль — ряд из 4 клеток («четырёхпалубный»; линкор)
2 корабля — ряд из 3 клеток («трёхпалубные»; крейсера)
3 корабля — ряд из 2 клеток («двухпалубные»; эсминцы)
4 корабля — 1 клетка («однопалубные»; торпедные катера)\

При размещении корабли не могут касаться друг друга сторонами и углами.

Рядом со «своим» полем чертится «чужое» такого же размера, только пустое. Это участок моря, где плавают корабли противника.

При попадании в корабль противника — на чужом поле ставится крестик, при холостом выстреле — точка. Попавший стреляет ещё раз.

Самыми уязвимыми являются линкор и торпедный катер: первый из-за крупных размеров, в связи с чем его сравнительно легко найти, а второй из-за того, что топится с одного удара, хотя его найти достаточно сложно.";
        public MainWindow()
        {
            InitializeComponent();
            Closing += MainWindow_Closing;
            for (int i = 0; i < 99; i++)
            {
                var rect = new Rectangle();
                rect.Width = 30;
                rect.Height = 30;
                rect.Stroke = new SolidColorBrush(Color.FromRgb(100,150,150));
                rect.Fill = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                rect.StrokeThickness = 1;
                GridFieldOwn.Children.Add(rect);
            }
        }

        private void MainWindow_Closing(object? sender, CancelEventArgs e)
        {
            string msg = "Вы действительно хотите закрыть программу?";
            var result = MessageBox.Show(msg, Title, MessageBoxButton.YesNo, MessageBoxImage.Warning);
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

        private void AboutMenu_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(ABOUT, Title, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}