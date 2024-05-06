
using System;
using System.Text.RegularExpressions;

namespace SeaBattleApp
{
    public class Game
    {
        public delegate void AddShipHandler(object sender, Ship ship);
        public event AddShipHandler? AddShipEvent;
        public event Action<Game> ByShotEvent;

        public static Regex regexValidPosition = new Regex(@"^([1-9]|10)[А-ЕЖЗИК]$");

        public enum Mode { SinglePlayer, TwoPlayers }

        public Mode ModeGame { get; init; }

        public string Greeting => "Добро пожаловать на игру \"Морской бой!\"";
        public BattleField MyField { get; }
        public BattleField OpponentField { get; }
        public BattleField CurrentField { get; private set; }
        public CompPlayer TheCompPlayer { get; private set; }  


        public Game(Mode mode = Mode.SinglePlayer)
        {
            ModeGame = mode;
            MyField = new BattleField(true, 10, 10);
            OpponentField = new BattleField(false, 10, 10);
            CurrentField = MyField;
            if (mode == Mode.SinglePlayer)
            {
                TheCompPlayer = new CompPlayer(OpponentField);
                PlaceOpponentShips();
            }
            else 
            {
                // Тут будет назначаться поле противника, как this.OpponentField = gameOther.MyField
            }
        }

        public void PlaceOpponentShips()
        {
            var size = OpponentField.Rows * OpponentField.Columns;
            var shipsOutside = createShips();
            var arrOfLen = new int[] { 4, 3, 3, 2, 2, 2, 1, 1, 1, 1 };
            var arrOfBool = new bool[] { true, false };
            var random = new Random();
            foreach (int len in arrOfLen) {
                var ship = ChooseTheShip(shipsOutside, len);
                ship.IsHorizontalOrientation = arrOfBool[random.Next(0, 2)];
                var randIdx = random.Next(0, size);
                while (!OpponentField.TryAddTheShip(ship, Coordinate.Parse(TheCompPlayer.AllPositionsComp[randIdx]), out string errorMsg)) {
                    randIdx = random.Next(0, size);
                    ship.IsHorizontalOrientation = arrOfBool[random.Next(0, 2)];
                }

                ship.BeginCoord = Coordinate.Parse(TheCompPlayer.AllPositionsComp[randIdx]);
                TheCompPlayer.ClearUnsablePositions(ship, true);
                size = TheCompPlayer.AllPositionsComp.Count;
            }
            TheCompPlayer.RestoreAllPositionsComp();   // компьютер восстанавливает в памяти все возможные позиции
        }

        public void CompMove2(ref bool isTheWinner)
        {
            var isFirstShotInLoop = true;
            do {
                string selectedPosition = "";

                selectedPosition = TheCompPlayer.ComputeMove2(isFirstShotInLoop);
                Console.WriteLine($"Комп стреляет по позиции: {selectedPosition}");

                bool IsDestroyedShip = false;
                (bool isSuccess, Ship? ship) = TryShootAtTheTarget(Coordinate.Parse(selectedPosition), false, ref IsDestroyedShip);

                TheCompPlayer.AllPositionsForOpponent.Remove(selectedPosition);  // выстрел произошёл, можно очистить позицию из списка всех позиций у компьютера

                if (!isSuccess) {
                    Console.WriteLine("Компьютер промахнулся. Теперь ваш черед.");
                    break;
                }

                TheCompPlayer.TheMemory.PositionsInProcess.Add(selectedPosition); // успех выстрела, можно добавить в память компьютера данную розицию
                if (!IsDestroyedShip) {
                    Console.WriteLine("Соперник попал в ваш корабль. Думает куда дальше выстрелить...");
                    Console.ReadKey();
                }
                else {
                    Console.WriteLine("Плохи дела. Компьютер потопил ваш корабль. Думает... нажмите клавишу, чтобы продолжить");
                    // надо сбросить память компьютера в начальное состояние
                    TheCompPlayer.ClearUnsablePositions(ship, false);
                    TheCompPlayer.TheMemory.Reset();

                    Console.ReadKey();
                    if (CurrentField.ShipsCounter == 0) {
                        Console.WriteLine("Увы! Вы проиграли, у вас не осталось ни одного корабля.");
                        isTheWinner = true;
                        return;
                    }
                }
                isFirstShotInLoop = false;

            } while (true);

        }

        public void PlayerMove(ref bool isTheWinner)
        {
            string targetCoords = ReadValidPosition();
            bool shipIsDestroyed = false;
            bool isMyMove = true;
            (bool isSuccess, Ship? ship) = TryShootAtTheTarget(Coordinate.Parse(targetCoords), isMyMove, ref shipIsDestroyed);
            while (isSuccess) {
                if (!shipIsDestroyed) {
                    Console.WriteLine("Вы молодец, подбили корабль! Стреляйте ещё раз (введите координату)!!!");
                }
                else {
                    Console.WriteLine("УРА!!!!!!!!!!!!\nКорабль уничтожен!!!\nСтреляйте ещё раз (введите координату)!!!");
                    if (CurrentField.ShipsCounter == 0) {
                        Console.WriteLine("О ДА!!! ВЫ ЖЕ ПОБЕДИЛИ!!!! КРАСАВЧИК!!!");
                        isTheWinner = true;
                        return;
                    }
                }
                targetCoords = ReadValidPosition();
                shipIsDestroyed = false;
                (isSuccess, ship) = TryShootAtTheTarget(Coordinate.Parse(targetCoords), isMyMove, ref shipIsDestroyed);
            }
            Console.WriteLine("Вы не попали. Стреляет компьютер... Нажмите клавишу!");
            Console.ReadLine();
        }

        public string ReadValidPosition()
        {
            string coords = Console.ReadLine()?.ToUpper() ?? "DDD";
            while (!IsValidRuCoordinate(coords)) {
                Console.WriteLine("Координата не корректная!\nПопробуйте ещё раз!\n");
                coords = Console.ReadLine()?.ToUpper() ?? "DDD";
            }
            return coords;
        }

        public (bool, string) TryAddTheShipOnTheField(Ship ship, Coordinate coord) {
            if (!MyField.TryAddTheShip(ship, coord, out string errorMsg)) {
                return (false, errorMsg);
            };
            AddShipEvent?.Invoke(this, ship);
            return (true, "Success");
        }

        public List<Ship> createShips() {
            var shipsOutside = new List<Ship>();
            foreach (var pair in MyField.ExpectedMapLengthByCounter)
            {
                for (int i = 0; i < pair.Value; i++)
                    shipsOutside.Add(new Ship(pair.Key));
            }
            return shipsOutside;
        }

        public bool IsValidRuCoordinate(string coords) => regexValidPosition.IsMatch(coords.ToUpper());




        public Ship ChooseTheShip(List<Ship> ships, int length)
        {
            Ship? ship = ships.Find(sh => sh.Length == length);
            if (ship == null) throw new Exception("The ship not found!");
            ships.Remove(ship);
            return ship;
        }

        public (bool, Ship?) TryShootAtTheTarget(Coordinate coord, bool isItMyMove, ref bool IsDestroyedShip) {
            (bool isSuccess, Ship? ship) = (false, null);
            if (isItMyMove) {       
                CurrentField = OpponentField;
                (isSuccess, ship) = CurrentField.TryHitTheShip(coord, ref IsDestroyedShip);
            }
            else {          
                CurrentField = MyField;
                (isSuccess, ship) = CurrentField.TryHitTheShip(coord, ref IsDestroyedShip);
                
            }
            ByShotEvent?.Invoke(this);
            return (isSuccess, ship);    
        }


    }
}