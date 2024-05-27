
using SeaBattleApp.Models;
using SeaBattleApp.TcpConnecting;
using System.Text.RegularExpressions;

namespace SeaBattleApp;

class Program
{
    private static Game game = new Game();

    static void Main(string[] args)
    {
        // Добавить события
        game.FieldStatusChangedEvent += ShowGameBoardVer2;
        game.WriteMessageForPlayerEvent += Console.WriteLine;
        game.DelayMoveCompEvent += ComputerThinks;

		while (PlayNext()) { }
        Console.WriteLine("\n\t*** Конец игры ***\t\n");
    }

    public static bool PlayNext()
    {
/*        var game = new Game();
        game.FieldStatusChangedEvent += ShowGameBoardVer2;
        game.WriteMessageForPlayerEvent += Console.WriteLine;*/
        bool oneMoreTime = true;
        Console.WriteLine("\nВыберите режим игры:\n1 - Игра с очень умным компьютером\n2 - Игра на двоих по локальной сети\n3 - Выход");
        var choice = Console.ReadLine();
        string userName = "";
 
        switch (choice) {
            case "1":
                game.ModeGame = Game.Mode.SinglePlayer;
                Console.WriteLine($"Вы выбрали режим игры с компьютером.\nЗдравствуйте игрок {game.Player1.Username}. ");
                Console.WriteLine("Если это ваш никнейм, нажмите Enter, или введите свой никнейм: ");
                userName = Console.ReadLine() ?? "Anon";
                game.Player1.Username = string.IsNullOrWhiteSpace(userName) ? "Anon" : userName;
                Console.WriteLine("\nОтлично, начнём игру!");
                break;
            case "2":
                game.ModeGame = Game.Mode.TwoPlayers;
                Console.WriteLine($"Вы выбрали режим игры с компьютером.\nЗдравствуйте игрок {game.Player1.Username}. ");
                Console.WriteLine("Если это ваш никнейм, нажмите Enter, или введите свой никнейм: ");
                userName = Console.ReadLine() ?? "Anon";
                game.Player1.Username = string.IsNullOrWhiteSpace(userName) ? "Anon" : userName;
               
            repeat:
                Console.WriteLine("Определитесь кто из вас будет ходить первым. Нажмите 1 - если Вы, 2 - если Соперник\n" +
                    "Не забывайте - у вас должны быть разные ответы выбора. Если оба игрока выбирут 2, то игра зависнет и потребуется перезапустить её. Если оба выбрали 1, то подождите немного.): ");
                var choiceRole = Console.ReadLine();
                if (choiceRole == "1") {
                    game.IsClientPlayer = true;
                    Console.WriteLine($"Вот ваш ip адрес: {game.TheServer.TheIpAdress}.\nСкажите его 2-му игроку, чтобы установить соединение.");
                    if (TryGetInitClient(game)) {
                        Console.WriteLine("Ждём, когда 2-ой игрок введёт данные!!!!!!!!!!!!!!!!!!!!!!");
                        if (game.TheServer.TryStart()) {
                            Console.WriteLine("Принят сигнал!!!!!!!!!!!!!!!!!!!!!!");
                        }
                        Console.WriteLine("\nОтлично, начнём игру!");
                    } 
                    else {
                        Console.WriteLine("Соединение не произошло. Попробуйте ещё раз.");
                        goto repeat;
                    }
                }
                else if (choiceRole == "2") {
                    game.IsClientPlayer = false;
                    Console.WriteLine($"Вот ваш ip адрес: {game.TheServer.TheIpAdress}.\nСкажите его 2-му игроку, чтобы установить соединение.");
                    Console.WriteLine("Ждём, когда 2-ой игрок введёт данные!!!!!!!!!!!!!!!!!!!!!!");
                    if (game.TheServer.TryStart()) {
                        Console.WriteLine("Принят сигнал!!!!!!!!!!!!!!!!!!!!!!");
                    }
                    if (TryGetInitClient(game)) {
                        Console.WriteLine("\nОтлично, начнём игру!");
                    }
                    else {
                        Console.WriteLine("Соединение не произошло. Попробуйте ещё раз.");
                        goto repeat;
                    }
                }
                else {
                    Console.WriteLine("Пожалуйста, следуйте инструкциям. Попробуйте сначала.");
                    return oneMoreTime;
                }
                break;

            case "3":
                Console.WriteLine("Вы выбрали выход, до свидания!");
                return !oneMoreTime;
            default:
                Console.WriteLine("Такого режима не существует. Выбирите другой!");
                return oneMoreTime;
        }


        var border = new string('*', game.Greeting.Length + 6);
        Console.WriteLine($"\n{border}\n {game.Player1.Username} {game.Greeting}\n{border}\n");

        if (game.ModeGame == Game.Mode.SinglePlayer) {
            Console.WriteLine("Ждём соперника, когда он расставит свои корабли.");
            game.InitCompPlayer();
            Console.WriteLine("\nСоперник готов к сражению!\n");
        }

        Console.WriteLine("Вам даны 10 кораблей:");
        char shV = BattleField.MarkIsAShip == 5 ? 'S' : '5';
        Console.WriteLine($"4-ре 1-палубных:   {shV}\t|  {shV}\t|  {shV}\t|  {shV}");
        Console.WriteLine($"3-ри 2-палубных:   {shV}{shV}\t|  {shV}{shV}\t|  {shV}{shV}");
        Console.WriteLine($"2-ва 3-палубных:   {shV}{shV}{shV}\t|  {shV}{shV}{shV}");
        Console.WriteLine($"1-ин 4-палубный:   {shV}{shV}{shV}{shV}");
        Console.WriteLine("И 2 поля.");
        ShowGameBoardVer2(game);

        var shipsOutside = game.createShips();
        Console.WriteLine("\nРасположите корабли на вашем поле.\n");


        while (shipsOutside.Count > 0) {
            Console.WriteLine($"Выберите корабль нужной палубности и разместите его на поле указав координату начала и ориентацию.\nВсего осталось {shipsOutside.Count} кораблей.");
            Console.WriteLine("Скольки палубный вы хотите добавить на поле (от 1 до 4)?: ");
            int len;
            while (!(int.TryParse(Console.ReadLine(), out len) && len > 0 && len < 5)) {
                WriteLineColor("Стольки палубных кораблей не существует.\nВведите целое число в диапазоне [1, 4]: ", ConsoleColor.Red);
            }

            Ship? targetShip = null;
            try {
                targetShip = game.ChooseTheShip(shipsOutside, len);
            }
            catch (Exception ex) {
                WriteLineColor($"{ex.Message}\nКораблей с такой палубностью не найдено! Попробуйте ещё раз!\n", ConsoleColor.Red);
                continue;
            }
            Console.WriteLine("Корабль найден и готов к установке на поле.");


        // label for goto
        tryCoordinates:
            Console.Write("Вводите координату (сначала номер строки, потом букву столбца, без пробелов): ");
            string position = game.ReadValidPosition();

            string orientation = "в";
            if (len != 1)  // для однопалубных не спрашиваем ориентацию
            {
                Console.WriteLine("Введите ориентацию корабля (в - вертикальная, г - горизонтальная): ");
                orientation = Console.ReadLine()!.ToLower();
            }

            while (orientation != "в" && orientation != "г") {
                WriteLineColor("Такая ориентация не поддерживается!\nПопробуй ещё раз!\n", ConsoleColor.Red);
                orientation = Console.ReadLine()!.ToLower();
            }

            targetShip.IsHorizontalOrientation = orientation == "г" ? true : false;
            (bool, string) resultWithMsg = game.TryAddTheShipOnTheField(targetShip, Coordinate.Parse(position));
            if (!resultWithMsg.Item1) {
                WriteLineColor($"{resultWithMsg.Item2}. Попробуйте ещё раз!\n", ConsoleColor.Red);
                goto tryCoordinates;
            }
        }

        WriteLineColor("Все корабли установлены.\n", ConsoleColor.Magenta);


        if (game.ModeGame == Game.Mode.TwoPlayers) {
            Console.WriteLine("Нажмите Enter и подождите, когда соперник подтвердит установку его поля вашим.");
            Console.ReadLine();
            game.ExecuteSettingOpponentBattlefield2();     // 2 раза, т.к. сначала сообщение отправляется, а потом читается (или наоборот)
            game.ExecuteSettingOpponentBattlefield2();
            ShowGameBoardVer2(game);
            if (game.IsClientPlayer) {
                Console.WriteLine("Тперерь можете стрелять по вражеским кораблям!\nНаведите пушку и пли! (введите координату): \n");
            }
            else {
                Console.WriteLine("Стреляет противник");
            }
            bool isTheWinner;
            while (true) {
                if (game.IsClientPlayer) {
                    isTheWinner = false;
                    game.PlayerClientMove(ref isTheWinner);
                    if (isTheWinner) break;
                }
                else {
                    isTheWinner = false;
                    game.PlayerServerMove(ref isTheWinner);
                    if (isTheWinner) break;
                }
            }
        }
        else {
            ShowGameBoardVer2(game);
            Console.WriteLine("Тперерь можете стрелять по вражеским кораблям!\nНаведите пушку и пли! (введите координату): \n");
            bool isTheWinner;
            while (true) {
                isTheWinner = false;
                game.PlayerMove(ref isTheWinner);
                if (isTheWinner) break;

                isTheWinner = false;
                game.CompMove2(ref isTheWinner);
                if (isTheWinner) break;
            }
        }

        Console.WriteLine(game.SaveOrUpdatePlayerStatistics());
        Console.WriteLine("\nХотите сыграть ещё раз (да/любой другой ввод означает нет)? ");
        var answer = Console.ReadLine();
        if (answer == "да") {
            game.ResetGameStatus(); 
            return oneMoreTime;
        }
        else {
            if (game?.TheClient.IsConnected ?? false) {
                game.TheClient.Disconect();
            }
            if (game?.TheServer.IsStarted ?? false) {
                game.TheServer.Stop();
            }
            return false;
        }
    }

	private static void ComputerThinks()
	{
		Console.WriteLine("Компьютер думает над следующим ходом...");
		for (int i = 0; i < 20; i++) {
			Console.Write(".");
			Thread.Sleep(10);
		}
	}

	private static void ShowGameBoardVer2(Game game)
    {
        string spaceBetweenFields = new String(' ', game.MyField.Columns - 2);
        WriteLineColor("\n\tВаше поле:" + spaceBetweenFields + spaceBetweenFields + "Противника поле:", ConsoleColor.Cyan);
        int[,] myField = game.MyField.Field;
        int[,] opponentField = game.OpponentField.Field;

        Console.Write("    ");
        PrintRuCharsInLine(myField);
        Console.Write(spaceBetweenFields);
        PrintRuCharsInLine(opponentField);
        Console.Write("\n   " + new string('-', myField.GetLength(1) * 2));
        Console.Write(spaceBetweenFields);
        Console.WriteLine(new string('-', myField.GetLength(1) * 2));

        for (int i = 0; i < myField.GetLength(0); ++i) {
            var indent = (i + 1) < 10 ? " " : "";
            Console.Write($"{i + 1}{indent}| ");

            for (int j = 0; j < myField.GetLength(1); ++j) {
                char imgShip = GetImageShip(myField, i, j);
                Console.Write(imgShip + " ");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            Console.Write("    ");
            Console.Write($"{i + 1}{indent}| ");
            for (int j = 0; j < opponentField.GetLength(1); ++j) {
                char imgShip = GetImageShip(opponentField, i, j);
                Console.Write(imgShip + " ");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            Console.WriteLine("");
        }
        Console.WriteLine();
    }

    private static void PrintRuCharsInLine(int[,] field)
    {
        var charCol = BattleField.FIRST_CHAR_RU;
        for (int i = 0; i < field.GetLength(1); i++) {
            var ch = charCol == 'Й' || charCol == 'Ё' ? ++charCol : charCol;
            Console.Write($"{charCol++} ");
        }
    }

    private static char GetImageShip(int[,] field, int i, int j)
    {
        char shipImg = '.';
        if (field[i, j] == BattleField.MarkIsAShip) {
            shipImg = 'S';
            Console.ForegroundColor = ConsoleColor.Green;
        }
        else if (field[i, j] == (int)BattleField.CellState.Unexplored || field[i, j] == BattleField.MarkAShipInvisible) {
            shipImg = '*';
            Console.ForegroundColor = ConsoleColor.Blue;
        }
        else if (field[i, j] == (int)BattleField.CellState.Empty) {
            shipImg = '-';
            Console.ForegroundColor = ConsoleColor.White;
        }
        else if (field[i, j] == (int)BattleField.CellState.BurningShip) {
            shipImg = 'B';
            Console.ForegroundColor = ConsoleColor.Red;
        }
        else if (field[i, j] == (int)BattleField.CellState.DestroyedShip) {
            shipImg = 'X';
            Console.ForegroundColor = ConsoleColor.DarkGray;
        }
        return shipImg;
    }


    public static void WriteLineColor(string s, ConsoleColor color)
    {
        ConsoleColor prev = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(s);
        Console.ForegroundColor = prev;
    }

    private static bool TryGetInitClient(Game game)
    {
       
        Console.WriteLine("Спросите у 2-го игрока его ip-адрес и введите его:");
        var choiceAddress = Console.ReadLine() ?? " ";
        Regex ipRegex = new Regex(@"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}");
        if (ipRegex.IsMatch(choiceAddress)) {
            game.TheClient = new Client(choiceAddress, game.TheServer.ThePort);
            return game.TheClient.TryConnect();
        }
        else {
            return false;
        }
    }

}
