using SeaBattleApp;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using SeaBattleApp.Models;


namespace SeaBattleGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static int START_BUTTON_ID_SELF = 0;
        private static int START_BUTTON_ID_OPPONENT = 100;
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
        private List<Ship> _shipsListOutline;

		private RotateTransform Rotate90 => new RotateTransform(90);
		private RotateTransform Rotate0 => new RotateTransform(0);
        private List<ShipImgOutline> _shipsImgOutline;
        private ShipImgOutline CurrentShipImgOutline { get; set; }
        private List<Button> ButtonsCellsSelf;
        private List<Button> ButtonsCellsOpponent;

		public MainWindow()
        {
            InitializeComponent();
            InitGameWindow();
           
            _shipsListOutline = game.createShips();
        }

        private void InitGameWindow()
        {

			var lengthField = game.CurrentField.Rows;
			RadioBtnCompPlayer.IsChecked = true;
            ButtonsCellsSelf = new List<Button>();
            ButtonsCellsOpponent = new List<Button>();
			Closing += MainWindow_Closing;
			GenerateButtonsCells(GridFieldSelf, lengthField, START_BUTTON_ID_SELF);
			GenerateButtonsCells(GridFieldOpponent, lengthField, START_BUTTON_ID_OPPONENT);
			FillStackCharacters(LineLettersSelf, lengthField, Orientation.Horizontal, 100, 70);
			FillStackCharacters(LineLettersOpponent, lengthField, Orientation.Horizontal, 100, 70);
			FillStackCharacters(LineNumbersSelf, lengthField, Orientation.Vertical, 80, 102);
			FillStackCharacters(LineNumbersOpponent, lengthField, Orientation.Vertical, 415, 102);


			_shipsImgOutline = new List<ShipImgOutline>();
            _shipsImgOutline.Add(new ShipImgOutline(ImgShip4.Name, 4, true, 1));
            _shipsImgOutline.Add(new ShipImgOutline(ImgShip3.Name, 3, true, 2));
            _shipsImgOutline.Add(new ShipImgOutline(ImgShip2.Name, 2, true, 3));
            _shipsImgOutline.Add(new ShipImgOutline(ImgShip1.Name, 1, true, 4));

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

		private void GenerateButtonsCells(UniformGrid grid, int length, int startId)
		{
			for (int i = 0; i < length * length; i++) {
				var btnCell = new Button()
				{
					Tag = startId + i,
					Width = 30,
					Height = 30,
					Background = new SolidColorBrush(Color.FromRgb(250, 232, 232)),
					BorderBrush = new SolidColorBrush(Color.FromRgb(176, 184, 164)),
				};
				btnCell.Click += ButtonCell_Click;
                if (startId == START_BUTTON_ID_SELF) {
                    ButtonsCellsSelf.Add(btnCell);
                }
                else {
                    ButtonsCellsOpponent.Add(btnCell);
                }
				grid.Children.Add(btnCell);
			}
		}

		private void ButtonCell_Click(object sender, RoutedEventArgs e)
		{
            Button thisButton = (Button)sender;
            MessageBox.Show(thisButton.Tag.ToString());
			
		}

		private void MainWindow_Closing(object? sender, CancelEventArgs e)
        {
            /*string msg = "Вы действительно хотите закрыть программу?";
            var result = MessageBox.Show(msg, Title, MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.No) {
                e.Cancel = true;
            }*/
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

		private void ButtonLeftPressed_ImgShip(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			var imageShip = (Image)sender;
			ShipImgOutline shipOutline = _shipsImgOutline.Single(sh => sh.Name == imageShip.Name);
			CurrentShipImgOutline = shipOutline;
			Cursor = Cursors.None;
            Cursor = _shipsImgOutline.Single(sh => sh.Name == imageShip.Name).GetCursor();
			
		}

		private void ButtonRightPressed_ImgShip(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
            var imageShip = (Image)sender;
			ShipImgOutline shipOutline = _shipsImgOutline.Single(sh => sh.Name == imageShip.Name);
            if (shipOutline.Length == 1) return;
			if (shipOutline.IsHorizontal) {
				imageShip.RenderTransform = Rotate90;
			}
            else {
				imageShip.RenderTransform = Rotate0;
			}
			shipOutline.IsHorizontal = !shipOutline.IsHorizontal;
		}

	}

	class ShipImgOutline
    {
        private const string PREFIX_PATH = "pack://application:,,,/Resources/Cursors/";
		public string Name { get; set; }
		public int Length { get; set; }
		public bool IsHorizontal { get; set; }
        public int CounterDownShips { get; set; }
        private static Cursor[] _cursorsImgVertical;
        private static Cursor[] _cursorsImgHorizontal;

        static ShipImgOutline()
        {
			_cursorsImgHorizontal = new Cursor[] {
				new Cursor(Application.GetResourceStream(new Uri($"{PREFIX_PATH}CursorShip1.cur")).Stream),
				new Cursor(Application.GetResourceStream(new Uri($"{PREFIX_PATH}CursorShip2H.cur")).Stream),
				new Cursor(Application.GetResourceStream(new Uri($"{PREFIX_PATH}CursorShip3H.cur")).Stream),
				new Cursor(Application.GetResourceStream(new Uri($"{PREFIX_PATH}CursorShip4H.cur")).Stream),
			};
			_cursorsImgVertical = new Cursor[] {
				new Cursor(Application.GetResourceStream(new Uri($"{PREFIX_PATH}CursorShip1.cur")).Stream),
				new Cursor(Application.GetResourceStream(new Uri($"{PREFIX_PATH}CursorShip2V.cur")).Stream),
				new Cursor(Application.GetResourceStream(new Uri($"{PREFIX_PATH}CursorShip3V.cur")).Stream),
				new Cursor(Application.GetResourceStream(new Uri($"{PREFIX_PATH}CursorShip4V.cur")).Stream),
			};
		}

		public ShipImgOutline(string name, int length , bool isHorizontal, int counterRemainderPart)
		{
			Name = name;
			IsHorizontal = isHorizontal;
            Length = length;
            CounterDownShips = counterRemainderPart;
		}

        public Cursor GetCursor() => IsHorizontal ? _cursorsImgHorizontal[Length - 1] : _cursorsImgVertical[Length - 1];
	}
}