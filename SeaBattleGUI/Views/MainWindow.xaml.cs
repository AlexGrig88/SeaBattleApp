using SeaBattleApp;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using SeaBattleApp.Models;
using System.Windows.Media.Imaging;


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
		private const string PREFIX_PATH = "\\Resources\\Images\\";
		private static int START_BUTTON_ID_SELF = 0;
        private static int START_BUTTON_ID_OPPONENT = 100;
        public Game TheGame { get; private set; } = ((App)Application.Current).TheGame;

		private RotateTransform Rotate90 => new RotateTransform(90);
		private RotateTransform Rotate0 => new RotateTransform(0);
        private List<ShipImgOutline> _shipsImgOutline;
		private int _counterShipsOutline;

        private ShipImgOutline? CurrentShipImgOutline { get; set; }
        private List<Button> ButtonsCellsSelf;
        private List<Button> ButtonsCellsOpponent;
		private bool IsTheWinner { get; set; }
		private bool CanMove {  get; set; }

		private Dictionary<string, string> _imgsNames = new Dictionary<string, string>
			{ {"ship", "markIsAShip.png" }, {"empty", "empty.png" }, {"burning", "burning.png" }, {"destroyed", "destroyed.png" } };

		public MainWindow()
        {
            InitializeComponent();
			StartingField.Visibility = Visibility.Visible;
			PlayingField.Visibility = Visibility.Collapsed;
			RadioBtnCompPlayer.IsChecked = true;
			StackPanelTablo.Visibility = Visibility.Collapsed;
			CanMove = true;
			InitGameWindow();

			_counterShipsOutline = 0;
            TheGame.FieldStatusChangedEvent += HandleChangedFieldStatus;
			TheGame.WriteMessageForPlayerEvent += WriteLineToStatusBarOrTablo;

		}

		private void WriteLineToStatusBarOrTablo(string message)
		{
			if (message.Contains("по позиции")) {
				TextBlockMoveOpponent.Text = message.Substring(message.IndexOf(": ") + 2);
				TextBlockMoveSelf.Text = "";
			}
			StatusBarText.Text = message;
		}

		private void InitGameWindow()
        {

			var lengthField = TheGame.CurrentField.Rows;
			
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
            _shipsImgOutline.Add(new ShipImgOutline(ImgShip4.Name, 4, true, 1, ImgShip4));
            _shipsImgOutline.Add(new ShipImgOutline(ImgShip3.Name, 3, true, 2, ImgShip3));
            _shipsImgOutline.Add(new ShipImgOutline(ImgShip2.Name, 2, true, 3, ImgShip2));
            _shipsImgOutline.Add(new ShipImgOutline(ImgShip1.Name, 1, true, 4, ImgShip1));

			ImgShotSelf.Visibility = Visibility.Hidden;
			ImgShotOpponent.Visibility = Visibility.Hidden;
			TextBlockShotSelf.Visibility = Visibility.Hidden;
			TextBlockShotOpponent.Visibility = Visibility.Hidden;

		}

		private void ToggleShotVisible(bool isSelfShot)
		{
			if (isSelfShot) {
				ImgShotSelf.Visibility = Visibility.Visible;
				TextBlockShotSelf.Visibility = Visibility.Visible;
				ImgShotOpponent.Visibility = Visibility.Hidden;
				TextBlockShotOpponent.Visibility = Visibility.Hidden;
			}
            else
            {
				ImgShotOpponent.Visibility =  Visibility.Visible;
				TextBlockShotOpponent.Visibility =  Visibility.Visible;
				ImgShotSelf.Visibility = Visibility.Hidden;
				TextBlockShotSelf.Visibility = Visibility.Hidden;
			}
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
                        Margin = new Thickness(9, 0, 10, 0),
						FontWeight = FontWeights.SemiBold
                    };
                    stack.Children.Add(textBlock);
                }
                else {
                    var textBlock = new TextBlock
                    {
                        FontSize = 18,
                        Text = (i + 1).ToString(),
                        Margin = new Thickness(0, 3, 0, 3),
						FontWeight = FontWeights.SemiBold
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
					Background = new SolidColorBrush(Color.FromRgb(242, 210, 177)),
					BorderBrush = new SolidColorBrush(Color.FromRgb(176, 184, 164)),
				};
				
                if (startId == START_BUTTON_ID_SELF) {
					btnCell.Click += ButtonCellSelf_Click;
					ButtonsCellsSelf.Add(btnCell);
				}
                else {
					btnCell.Click += ButtonCellOpponent_Click;
					ButtonsCellsOpponent.Add(btnCell);
                }
				grid.Children.Add(btnCell);
			}
		}

		private async void ButtonCellOpponent_Click(object sender, RoutedEventArgs e)
		{
			Button thisButton = (Button)sender;
			if (IsTheWinner) {
				MessageBox.Show($"Игра завершена.");
			}
			else if (_counterShipsOutline < 10) {
				MessageBox.Show($"Игрок {TheGame.Player1.Username} ещё не расставил корабли.");
			}
			else if (!CanMove) {
				MessageBox.Show("Ходит оппонент. Ожидайте!");
			}
			else {
				HandleMovePlayer(thisButton);
			}

		}

		private async void HandleMovePlayer(Button button)
		{
			int btnCoordinate = (int)button.Tag - START_BUTTON_ID_OPPONENT;     // сдвигаем, чтобы получить значение от 0 до 100

			bool shipIsDestroyed = false;
			bool isMyMove = true;
			Coordinate target = new Coordinate(btnCoordinate / 10, btnCoordinate % 10);

			TextBlockMoveSelf.Text = target.GetPosition().ToString();
			TextBlockMoveOpponent.Text = "";

			(bool isSuccess, Ship? ship) = TheGame.TryShootAtTheTarget(target, isMyMove, ref shipIsDestroyed);
			++TheGame.Player1.Score;

			if (!isSuccess) {           // если выстрел был неудачным запускаем логику для срельбы компьютера
				StatusBarText.Text = $"Игрок {TheGame.Player1.Username}, Вы промахнулись. Ход переходит к соперникку.";
				ToggleShotVisible(false);
				CanMove = false;
				bool isTheWinner = await TheGame.CompMove2Async();
				ToggleShotVisible(true);
				if (isTheWinner) {
					IsTheWinner = true;
					MessageBox.Show($"Игрок {TheGame.Player1.Username}, вы проиграли.");
				}
				CanMove = true;
				return;
			}
			
			if (!shipIsDestroyed) {
				StatusBarText.Text = $"Игрок {TheGame.Player1.Username}, вы молодец, подбили корабль! Стреляйте ещё раз!!!";
			}
			else {
				if (TheGame.CurrentField.ShipsCounter == 0) {   // ещё раз проверяем, что кораблей не осталось на поле
					MessageBox.Show($"Игрок {TheGame.Player1.Username} ВЫ ЖЕ ПОБЕДИЛИ!!!! МОЛОДЕЦ!!!");
					++TheGame.Player1.VictoryCounter;
					IsTheWinner = true;
					return;
				}
				StatusBarText.Text = "УРА!!!!!!!!!!!!Корабль уничтожен!!!Стреляйте ещё раз!!!";
			}
		}

		private void ButtonCellSelf_Click(object sender, RoutedEventArgs e)
		{
			if (CurrentShipImgOutline == null) {
				if (_counterShipsOutline == 10) {
					MessageBox.Show($"Все корабли установлены!");
					return;
				}
				MessageBox.Show("Не один корабль не выбран.");
				return;
			}
			Button thisButton = (Button)sender;
			int tag = (int)thisButton.Tag;

			Ship targetShip = new Ship(CurrentShipImgOutline.Length, CurrentShipImgOutline.IsHorizontal);
			(bool, string) resultWithMsg = TheGame.TryAddTheShipOnTheField(targetShip, new Coordinate(tag / 10, tag % 10));

			if (!resultWithMsg.Item1) {     // если валидация прошла неудачно
				MessageBox.Show($"{resultWithMsg.Item2}. Попробуйте ещё раз!");
				CurrentShipImgOutline = null;
				Cursor = Cursors.Arrow;
				return;
			}
			else {
				_counterShipsOutline++;
				CurrentShipImgOutline.CounterDownShips--;
				ChangeTextBlockCounter(CurrentShipImgOutline);
				if (CurrentShipImgOutline.CounterDownShips == 0) {		// если все корабли данной длины установлены, убираем картинку свободного корабля
					CurrentShipImgOutline.TheImage.Visibility = Visibility.Collapsed;
				}
				CurrentShipImgOutline = null;
				Cursor = Cursors.Arrow;
				if (_counterShipsOutline == 10) {
					MessageBox.Show($"Все корабли установлены!");
					ToggleShotVisible(true);
					TextBlockMoveSelf.Text = "";
					TextBlockMoveOpponent.Text = "";
					StackPanelTablo.Visibility = Visibility.Visible;

				}
			} 
			
		}

		private void ChangeTextBlockCounter(ShipImgOutline currentShipImgOutline)
		{
			int remainder = currentShipImgOutline.CounterDownShips;
			string textRemainder = $"x {remainder} шт.";
			switch (currentShipImgOutline.Length) {
				case 4: TextBlockCounter4Len.Text = remainder == 0 ? "" : textRemainder;
					break;
				case 3:
					TextBlockCounter3Len.Text = remainder == 0 ? "" : textRemainder;
					break;
				case 2:
					TextBlockCounter2Len.Text = remainder == 0 ? "" : textRemainder;
					break;
				case 1:
					TextBlockCounter1Len.Text = remainder == 0 ? "" : textRemainder;
					break;
				default: break;
			}
		}

		// обработчик для изменения статуса ячейки в поле
		private void HandleChangedFieldStatus(Game game)
		{
            List<Button> currentButtonsCells = game.CurrentField.IsItMyField ? ButtonsCellsSelf : ButtonsCellsOpponent;
			Image? image = null;
			int i = 0, j = 0;
            foreach (var btn in currentButtonsCells) {
				image = GetImageShip(game.CurrentField.Field, ref i, ref j);
				if (image != null) {
					btn.Content = image;
				}
				++j;
				if (j == 10) {
					j = 0; ++i;
				}
            }
		}

		private Image? GetImageShip(int[,] field, ref int i, ref int j)
		{
			Image shipImg = null;
			if (field[i, j] == BattleField.MarkIsAShip) {       //  || field[i, j] == BattleField.MarkAShipInvisible
				shipImg = new Image { Source = new BitmapImage(new Uri($"{PREFIX_PATH}{_imgsNames["ship"]}", UriKind.Relative)) };
			}
			else if (field[i, j] == (int)BattleField.CellState.Empty) {
				shipImg = new Image { Source = new BitmapImage(new Uri($"{PREFIX_PATH}{_imgsNames["empty"]}", UriKind.Relative)) };
			}
			else if (field[i, j] == (int)BattleField.CellState.BurningShip) {
				shipImg = new Image { Source = new BitmapImage(new Uri($"{PREFIX_PATH}{_imgsNames["burning"]}", UriKind.Relative)) };
			}
			else if (field[i, j] == (int)BattleField.CellState.DestroyedShip) {
				shipImg = new Image { Source = new BitmapImage(new Uri($"{PREFIX_PATH}{_imgsNames["destroyed"]}", UriKind.Relative)) };
			}
			return shipImg;
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
            string selfIp = $"Ваш ip адресс:  {TheGame.TheServer.TheIpAdress}";
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

		private void SelfField_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => Cursor = Cursors.Arrow;

		private void OpponentField_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => Cursor = Cursors.Arrow;

		private void ButtonStart_Click(object sender, RoutedEventArgs e)
		{
			TheGame.Player1.Username = TextBoxPlayerName.Text;
			if (RadioBtnCompPlayer.IsChecked == true) {
				TheGame.ModeGame = Game.Mode.SinglePlayer;
				TheGame.InitCompPlayer();
				/*				game.CurrentField = game.OpponentField;
								HandleChangedFieldStatus(game);
								game.CurrentField = game.MyField;*/
			}
			else {

			}
			StatisticsControl.Visibility = Visibility.Collapsed;
			StartingField.Visibility = Visibility.Collapsed;
			PlayingField.Visibility = Visibility.Visible;
		}

		private void ButtonStatistics_Click(object sender, RoutedEventArgs e)
		{
			StartingField.Visibility = Visibility.Collapsed;
			PlayingField.Visibility = Visibility.Collapsed;
			StatisticsControl.Visibility = Visibility.Visible;
		}

		private void ButtonExit_Click(object sender, RoutedEventArgs e) => Close();
	}

	class ShipImgOutline
    {
        private const string PREFIX_PATH = "pack://application:,,,/Resources/Cursors/";
		public string Name { get; set; }
		public int Length { get; set; }
		public bool IsHorizontal { get; set; }
        public int CounterDownShips { get; set; }
		public Image TheImage { get; set; }
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

		public ShipImgOutline(string name, int length , bool isHorizontal, int counterRemainderPart, Image img)
		{
			Name = name;
			IsHorizontal = isHorizontal;
            Length = length;
            CounterDownShips = counterRemainderPart;
            TheImage = img;
		}

        public Cursor GetCursor() => IsHorizontal ? _cursorsImgHorizontal[Length - 1] : _cursorsImgVertical[Length - 1];
	}
}