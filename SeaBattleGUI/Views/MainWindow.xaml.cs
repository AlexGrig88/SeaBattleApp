using SeaBattleApp;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;


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
        private Game game = ((App)Application.Current).TheGame;
        public MainWindow()
        {
            InitializeComponent();
            
            var lengthField = game.CurrentField.Rows;
            Closing += MainWindow_Closing;
            FillUniformGrid(GridFieldOwn, lengthField);
            FillUniformGrid(GridFieldOpponent, lengthField);
            FillStackCharacters(LineLettersSelf, lengthField, Orientation.Horizontal, 100, 70);
            FillStackCharacters(LineLettersOpponent, lengthField, Orientation.Horizontal, 100, 70);
            FillStackCharacters(LineNumbersSelf, lengthField, Orientation.Vertical, 80, 102);
            FillStackCharacters(LineNumbersOpponent, lengthField, Orientation.Vertical, 415, 102);

            RadioBtnCompPlayer.IsChecked = true;
        }

        private void FillStackCharacters(StackPanel stack, int length, Orientation orientation, int offsetLeft, int offsetRight)
        {
            char firstChar = 'А';
            for (int i = 0; i < length; i++)
            {
                if (orientation == Orientation.Horizontal) {
                    var textBlock = new TextBlock
                    {
                        FontSize = 18,
                        Text = new string((char)(firstChar + i >= 'Й' ? firstChar + i + 1 : firstChar + i), 1),
                        Margin = new Thickness(9, 0, 10, 0)
                    };
                    stack.Children.Add(textBlock);
                }
                else {
                    var textBlock = new TextBlock
                    {
                        FontSize = 18,
                        Text = (i + 1).ToString(),
                        Margin = new Thickness(0, 3, 0, 3)
                    };
                    stack.Children.Add(textBlock);
                }
            }
        }

        private void FillUniformGrid(UniformGrid grid, int length)
        {
            for (int i = 0; i < length * length - 1; i++) {
                var btnCell = new Button()
                {
                    Width = 30,
                    Height = 30,
                    Background = new SolidColorBrush(Color.FromRgb(250, 232, 232)),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(176, 184, 164)),
                };
                grid.Children.Add(btnCell);
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

        private void ButtonPrevPage_Click(object sender, RoutedEventArgs e)
        {
            PlayingField.Visibility = Visibility.Collapsed;
            StartingField.Visibility = Visibility.Visible;
        }

        private void RadioBtnTwoPlayers_Checked(object sender, RoutedEventArgs e)
        {
            string selfIp = $"Ваш ip адресс:  {game.TheServer.TheIpAdress}";
            TextBlockSelfIp.Text = selfIp;
            StackNetworkData.Visibility = Visibility.Visible;
        }

        private void RadioBtnComp_Checked(object sender, RoutedEventArgs e) => StackNetworkData.Visibility = Visibility.Hidden;

    }
}