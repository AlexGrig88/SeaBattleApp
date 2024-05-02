using System.Drawing;

namespace SeaBattleApp;

class Program
{

    static void Main(string[] args)
    {
        
        Play();

    }

    static void Play()
    {
        var game = new Game();
        var border = new string('*', game.Greeting.Length);
        Console.WriteLine($"{border}\n{game.Greeting}\n{border}\n");

        Console.WriteLine("Вам даны 10 кораблей:");
        char shV = BattleField.MarkIsAShip == 5 ? 'S' : '5';
        Console.WriteLine($"4-ре 1-палубных:   {shV}\t|  {shV}\t|  {shV}\t|  {shV}");
        Console.WriteLine($"3-ри 2-палубных:   {shV}{shV}\t|  {shV}{shV}\t|  {shV}{shV}");
        Console.WriteLine($"2-ва 3-палубных:   {shV}{shV}{shV}\t|  {shV}{shV}{shV}");
        Console.WriteLine($"1-ин 4-палубный:   {shV}{shV}{shV}{shV}");
        var shipsOutside = game.createShips();
        ShowGameBoard(game);

        Console.WriteLine("\nТеперь вы готовы для размещения вашей флотилии.\n");


        // Добавить событие на добавления корабля в поле(отрисовка моего поля с кораблями)
        game.AddShipEvent += HandleAddingAShip;
        game.ByShotEvent += ShowGameBoard;
/*
        int testCounter = 2;
        while (testCounter > 0) {*/
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
            string coords = ReadValidCoords(game);

            string orientation = "г";
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
            (bool, string) resultWithMsg = game.TryAddTheShipOnTheField(targetShip, Coordinate.ParseRu(coords));
            if (!resultWithMsg.Item1) {
                WriteLineColor($"{resultWithMsg.Item2}. Попробуйте ещё раз!\n", ConsoleColor.Red);
                goto tryCoordinates;
            }
            //--testCounter;
        }

        WriteLineColor("Все корабли установлены.\n", ConsoleColor.Magenta);
        ShowGameBoard(game);

        Console.WriteLine("Тперерь можете стрелять по вражеским кораблям!\nНаведите вашу пушку и пли (введите координату)!!!!\n");

        bool isTheWinner = false;
        while (true) 
        {
            PlayerMove(game, ref isTheWinner);
            if (isTheWinner) break;
            CompMove(game, ref isTheWinner);
            if (isTheWinner) break;
        }
        Console.WriteLine("\n" + border + "Конец игры" + border);

    }

    private static void PlayerMove(Game game, ref bool isTheWinner)
    {
        string targetCoords = ReadValidCoords(game);
        bool shipIsDestroyed = false;
        bool isMyMove = true;
        (bool isSuccess, Ship? ship) = game.TryShootAtTheTarget(Coordinate.ParseRu(targetCoords), isMyMove, ref shipIsDestroyed);
        while (isSuccess) {
            if (!shipIsDestroyed) {
                Console.WriteLine("Вы молодец, подбили корабль! Стреляйте ещё раз (введите координату)!!!");
            }
            else {
                Console.WriteLine("УРА!!!!!!!!!!!!\nКорабль уничтожен!!!\nСтреляйте ещё раз (введите координату)!!!");
                if (game.CurrentField.ShipsCounter == 0) {
                    Console.WriteLine("О ДА!!! ВЫ ЖЕ ПОБЕДИЛИ!!!! КРАСАВЧИК!!!");
                    isTheWinner = true;
                    return;
                }
            }
            targetCoords = ReadValidCoords(game);
            (isSuccess, ship) = game.TryShootAtTheTarget(Coordinate.ParseRu(targetCoords), isMyMove, ref shipIsDestroyed);
        }
        Console.WriteLine("Вы не попали. Стреляет компьютер... Нажмите клавишу!");
        Console.ReadLine();
    }

    private static void CompMove(Game game, ref bool isTheWinner)
    {
        var size = game.TheCompPlayer.AllPositions.Count;
        var random = new Random();
        var randPosition = game.TheCompPlayer.AllPositions[random.Next(0, size)];
        Console.WriteLine("Компьютер выстрелил по позиции : " + randPosition);
        bool shipIsDestroyed = false;
        (bool isSuccess, Ship? ship) = game.TryShootAtTheTarget(Coordinate.ParseRu(randPosition), false, ref shipIsDestroyed);
        while (isSuccess) {
            if (!shipIsDestroyed) {
                Console.WriteLine("Соперник попал в ваш корабль. Думает куда дальше выстрелить...");
                Console.ReadKey();
            }
            else {
                Console.WriteLine("Плохи дела. Компьютер потопил ваш корабль. Думает...");
                Console.ReadKey();
                if (game.CurrentField.ShipsCounter == 0) {
                    Console.WriteLine("Увы! Вы проиграли, у вас не осталось ни одного корабля.");
                    isTheWinner = true;
                    return;
                }
            }
            if (ship != null && shipIsDestroyed) {
                game.TheCompPlayer.AllPositions = game.TheCompPlayer.AllPositions.Except(ship.GetAllPositionsWithAround()).ToList();
            }
            else if (ship != null) {
                game.TheCompPlayer.AllPositions.Remove(randPosition);
            }
            // game.TheCompPlayer.AllPositions.Remove(randPosition); логика перенесена в класс Game, в метод TryShootAtTheTarget
            size -= 1; 
            randPosition = game.TheCompPlayer.AllPositions[random.Next(0, size)];
            Console.WriteLine("Компьютер выстрелил по позиции : " + randPosition);
            (isSuccess, ship) = game.TryShootAtTheTarget(Coordinate.ParseRu(randPosition), false, ref shipIsDestroyed);
        }
        Console.WriteLine("Компьютер промахнулся. Теперь ваш черед.");
    }

    private static void ShowGameBoard(Game game)
    {
        Console.WriteLine("\nВаше поле:");
        ShowBattleField(game.MyField);
        Console.WriteLine("\nПротивника поле:");
        ShowBattleField(game.OpponentField);
    }

    static string ReadValidCoords(Game game) {
        string coords = Console.ReadLine()?.ToUpper() ?? "DDD";
        while (!game.IsValidRuCoordinate(coords))
        {
            WriteLineColor("Координата не корректная!\nПопробуйте ещё раз!\n", ConsoleColor.Red);
            coords = Console.ReadLine()?.ToUpper() ?? "DDD";
        }
        return coords;
    }

    static void HandleAddingAShip(object sender, Ship? ship) {
        var game = (Game)sender;
        if (ship != null)
            WriteLineColor($"Установлен {ship.Length}-палубный корабль.", ConsoleColor.Green);
        Console.WriteLine("Ваше поле:");
        ShowBattleField(game.CurrentField);

    }

    static void ShowBattleField(BattleField battleField)
    {
        int[,] field = battleField.Field;
        var charCol = BattleField.FIRST_CHAR_RU;
        Console.Write("    ");
        for (int i = 0; i < field.GetLength(1); i++)
        {
            var ch = charCol == 'Й' || charCol == 'Ё' ? ++charCol : charCol;
            Console.Write($"{charCol++} ");
        }
        Console.WriteLine("\n   " + new string('-', field.GetLength(1) * 2));

        for (int i = 0; i < field.GetLength(0); ++i) {
            var indent = (i + 1) < 10 ? " " : ""; 
            Console.Write($"{i + 1}{indent}| ");
            for (int j = 0; j < field.GetLength(1); ++j)
            {
                
                char shipImg = '.';
                if (field[i, j] == BattleField.MarkIsAShip) {
                    shipImg = 'S';
                    Console.ForegroundColor = ConsoleColor.Green;
                }
                else if (field[i, j] == (int)BattleField.CellState.Unexplored) { //|| field[i, j] == BattleField.MarkAShipInvisible) {
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
                Console.Write(shipImg + " ");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            Console.WriteLine();
        }
        System.Console.WriteLine();
    }

    public static void WriteLineColor(string s, ConsoleColor color)
    {
        ConsoleColor prev = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(s);
        Console.ForegroundColor = prev;
    }

}
