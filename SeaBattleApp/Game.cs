
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


        public void PlaceOpponentShips()
        {
            var size = OpponentField.Rows * OpponentField.Columns;
            var shipsOutside = createShips();
            var arrOfLen = new int[] { 4, 3, 3, 2, 2, 2, 1, 1, 1, 1 };
            var arrOfBool = new bool[] { true, false };
            var random = new Random();
            foreach (int len in arrOfLen)
            {
                var ship = ChooseTheShip(shipsOutside, len);
                ship.IsHorizontalOrientation = arrOfBool[random.Next(0, 2)];
                var randIdx = random.Next(0, size);
                while (!OpponentField.TryAddTheShip(ship, Coordinate.Parse(TheCompPlayer.AllPositions[randIdx]), out string errorMsg))
                {
                    randIdx = random.Next(0, size);
                    ship.IsHorizontalOrientation = arrOfBool[random.Next(0, 2)];
                }
                
                TheCompPlayer.RemoveUnvailablePositions(TheCompPlayer.AllPositions[randIdx], len, ship.IsHorizontalOrientation);
                size = TheCompPlayer.AllPositions.Count;
            }
            TheCompPlayer.RestoreAllPositions();   // компьютер восстанавливает в памяти все возможные позиции
        }

        public Ship ChooseTheShip(List<Ship> ships, int length)
        {
            Ship? ship = ships.Find(sh => sh.Length == length);
            if (ship == null) throw new Exception("The ship not found!");
            ships.Remove(ship);
            return ship;
        }

        public (bool, Ship?) TryShootAtTheTarget(Coordinate coord, bool isItMyMove, ref bool shipIsDestroyed) {
            (bool isSuccess, Ship? ship) = (false, null);
            if (isItMyMove) {  // логика для живого игрока
                CurrentField = OpponentField;
                (isSuccess, ship) = CurrentField.TryHitTheShip(coord, ref shipIsDestroyed);
            }
            else {  // логика для компьютера
                CurrentField = MyField;
                (isSuccess, ship) = CurrentField.TryHitTheShip(coord, ref shipIsDestroyed);
                
            }
            ByShotEvent?.Invoke(this);
            return (isSuccess, ship);    
        }
    }
}