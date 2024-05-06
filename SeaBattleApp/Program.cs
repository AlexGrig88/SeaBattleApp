
using SeaBattleApp.Models;

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
        ShowGameBoardVer2(game);

        Console.WriteLine("\nТеперь вы готовы для размещения вашей флотилии.\n");


        // Добавить событие на добавления корабля в поле(отрисовка моего поля с кораблями)
        game.FieldStatusChangedEvent += ShowGameBoardVer2;
        game.WriteMessageForPlayerEvent += Console.WriteLine;



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
            (bool, string) resultWithMsg = game.TryAddTheShipOnTheField(targetShip, Coordinate.Parse(position));
            if (!resultWithMsg.Item1) {
                WriteLineColor($"{resultWithMsg.Item2}. Попробуйте ещё раз!\n", ConsoleColor.Red);
                goto tryCoordinates;
            }
        }

        WriteLineColor("Все корабли установлены.\n", ConsoleColor.Magenta);

        ShowGameBoardVer2(game);

        Console.WriteLine("Тперерь можете стрелять по вражеским кораблям!\nНаведите пушку и пли! (введите координату): \n");

        bool isTheWinner;
       
        while (true) 
        {
            isTheWinner = false;
            game.PlayerMove(ref isTheWinner);
            if (isTheWinner) break;

            isTheWinner = false;
            game.CompMove2(ref isTheWinner);
            if (isTheWinner) break;
        }
        Console.WriteLine("\n" + border + "Конец игры" + border);
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

/*    static void ShowBattleField(BattleField battleField)
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
        Console.WriteLine();
    }*/

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

}
