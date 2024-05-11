using System.Text;

namespace SeaBattleApp.Models
{

    public class BattleField
    {
        const int ROW_MAX_VALUE = 31;
        const int COLUMN_MAX_VALUE = 31;
        private int[][] _around = new int[8][] {
                new int[] { 1, 1 }, new int[] {1, 0 }, new int[] {0, 1 }, new int[]{-1, -1 },
                new int[]{-1, 0 }, new int[] {0, -1}, new int[] {1, -1 }, new int[] {-1, 1 }
                };

        public const char FIRST_CHAR_RU = 'А';

        public enum CellState { Unexplored = 0, Empty, BurningShip, DestroyedShip };   // состояние ячейки изменяется путём прибавления нового состояния к начальному 0
        public const int MarkIsAShip = 5;             //  метка видимого корабля
        public static int MarkAShipInvisible => 6;      // метка невидимого корабля                                     

        private bool _isItMyField;      // для выбора невидимого или видимого отображения корабля
        private int _rows;
        private int _columns;

        public List<Ship> ShipsList { get; set; }
        public int[,] Field { get; set; }
        public int Rows
        {
            get => _rows;
            set
            {
                if (value > ROW_MAX_VALUE)
                    throw new Exception($"КОЛЛИЧЕСТВО СТРОК НЕ ДОЛЖНО БЫТЬ БОЛЬШЕ {ROW_MAX_VALUE}");
                _rows = value;
            }
        }
        public int Columns
        {
            get => _columns;
            set
            {
                if (value > COLUMN_MAX_VALUE)
                    throw new Exception($"КОЛЛИЧЕСТВО СТОЛБЦОВ НЕ ДОЛЖНО БЫТЬ БОЛЬШЕ {COLUMN_MAX_VALUE}");
                _columns = value;
            }
        }

        public Dictionary<int, int> ExpectedMapLengthByCounter { get; }    // счётчик, определяющий колличество кораблей (в зависимости от длины(палубности)), возможных для размещения
        public Dictionary<int, int> CurrentMapLengthByCounter { get; }     // текущий счетчик кораблей на поле
        public int ShipsCounter { get; set; }

        public BattleField(bool isItMyField, int rows = 10, int columns = 10)
        {
            _isItMyField = isItMyField;
            ShipsCounter = 10;
            ExpectedMapLengthByCounter = new Dictionary<int, int>() { { 1, 4 }, { 2, 3 }, { 3, 2 }, { 4, 1 } };
            CurrentMapLengthByCounter = new Dictionary<int, int>() { { 1, 0 }, { 2, 0 }, { 3, 0 }, { 4, 0 } };
            Rows = rows;
            Columns = columns;
            Field = new int[Rows, Columns];
            ShipsList = new List<Ship>();
        }

        /// <summary>
        /// Формат строки для передачи: battfield;ChipsCounter;False,False,4,4,99: ... и так до конца списка кораблей
        /// максимальная длина буфера данных 100 + 2 + (5 + 5 + 4) * 10 + 51 разделителя = 293(проверить через дебаг) + 7 точек в конце, чтобы наверняка, итого 300
        /// При этом сервер ответит длиной как минимум меньшей, поэтому у полученных данных сервером надо взять длину, отнять длину серверного ответа и разность прибавить точками к серверному ответу
        /// </summary>
        /// <returns></returns>
        public string GetBattlefieldAsString()
        {
            StringBuilder sb = new StringBuilder(FieldToString());
            sb.Append(';');
            string chipsCounterAsStr = ShipsCounter < 10 ? "0" + ShipsCounter : ShipsCounter.ToString();
            sb.Append(chipsCounterAsStr).Append(';');
            var sbList = new StringBuilder();
            foreach (var ship in ShipsList) {
                sbList.Append(ship.ToSimpleString()).Append(':');
            }
            sb.Append(sbList.ToString().TrimEnd(':'));
            // Надо продебажить, определить максимальную длину возможной строки и добавлять каждый раз нехватающей длины справа точками PadRight()
            // А на приёме тримить их
            return sb.ToString();
        }

        public string FieldToString()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < Field.GetLength(0); i++) {
                for (int j = 0; j < Field.GetLength(1); j++) {
                    sb.Append(Field[i, j]);
                }
            }
            return sb.ToString();
        }

        public int[,] StringToField(string fieldStr)
        {
            int[,] restoredField = new int[10, 10];
            int lenVertical = Field.GetLength(0);
            int lenHorizont = Field.GetLength(1);
            string cell = "";
            for (int i = 0; i < lenVertical; i++) {
                for (int j = 0; j < lenHorizont; j++) {
                    cell = new string(fieldStr[i * lenHorizont + j], 1);
                    restoredField[i, j] = int.Parse(cell);
                }
            }
            return restoredField;
        }

        public bool TryAddTheShip(Ship ship, Coordinate beginCoord, out string errorMassage)
        {
            errorMassage = "КОРАБЛЬ РАЗМЕЩЁН";

            var validCoords1 = beginCoord.Row >= 0 || beginCoord.Col >= 0 || beginCoord.Row < _rows || beginCoord.Col < _columns;
            var validCoords2 = ship.IsHorizontalOrientation ?
                               beginCoord.Col + ship.Length <= _columns :
                               beginCoord.Row + ship.Length <= _rows;
            if (!validCoords1 || !validCoords2)
            {
                errorMassage = "КООРДИНАТЫ ЛЕЖАТ ЗА ПРЕДЕЛАМИ ПОЛЯ";
                return false;
            }

            if (CurrentMapLengthByCounter[ship.Length] >= ExpectedMapLengthByCounter[ship.Length])
            {
                errorMassage = $"ЧИСЛО КОРАБЛЕЙ С ДЛИНОЙ {ship.Length} ДОЛЖНО БЫТЬ НЕ БОЛЬШЕ {ExpectedMapLengthByCounter[ship.Length]}";
                return false;
            }

            if (!TryPutInTheField(ship, beginCoord, out string msg))
            {
                errorMassage = msg;
                return false;
            }
            CurrentMapLengthByCounter[ship.Length]++;
            ship.BeginCoord = beginCoord;
            ShipsList.Add(ship);
            return true;
        }

        private bool TryPutInTheField(Ship ship, Coordinate begCoord, out string errorMassage)
        {
            errorMassage = "ok";
            for (int i = 0, j = 0; i < ship.Length && j < ship.Length;)
            {
                if (Field[begCoord.Row + i, begCoord.Col + j] == MarkIsAShip || Field[begCoord.Row + i, begCoord.Col + j] == MarkAShipInvisible)
                {
                    errorMassage = "ПРИСУТСТВУЮТ ПЕРЕСЕКАЮЩИЕСЯ ЯЧЕЙКИ";
                    return false;
                }
                if (!ValidAdjacentCells(begCoord.Row + i, begCoord.Col + j, out string errorMsg))
                {
                    errorMassage = errorMsg;
                    return false;
                }
                if (ship.IsHorizontalOrientation) ++j;
                else ++i;
            }
            for (int i = 0, j = 0; i < ship.Length && j < ship.Length;)
            {
                Field[begCoord.Row + i, begCoord.Col + j] = _isItMyField ? MarkIsAShip : MarkAShipInvisible;
                if (ship.IsHorizontalOrientation) ++j;
                else ++i;
            }
            return true;
        }

        private bool ValidAdjacentCells(int rowPos, int colPos, out string errorMsg)
        {
            errorMsg = "Ok";
            for (int i = 0; i < _around.GetLength(0); i++)
            {
                if (IsNotValidBoundaries(rowPos, colPos, i))
                    continue;
                if (Field[rowPos + _around[i][0], colPos + _around[i][1]] == MarkIsAShip || Field[rowPos + _around[i][0], colPos + _around[i][1]] == MarkAShipInvisible)
                {
                    errorMsg = "НЕВОЗМОЖНО УСТАНОВИТЬ КОРАБЛЬ КАСАЮЩИЙСЯ СОСЕДНЕГО КОРАБЛЯ";
                    return false;
                }
            }
            return true;
        }

        public (bool, Ship?) TryHitTheShip(Coordinate coord, ref bool IsDestroyedShip)
        {
            Ship? targetShip = ShipsList.FirstOrDefault(ship => ship.GetAllCoordinates().Contains(coord));
            bool isHit = false;

            if (Field[coord.Row, coord.Col] == (int)CellState.Empty ||
                    Field[coord.Row, coord.Col] == (int)CellState.DestroyedShip ||
                        Field[coord.Row, coord.Col] == (int)CellState.BurningShip)
            {
                IsDestroyedShip = false;
                return (false, targetShip);
            }
            else if (Field[coord.Row, coord.Col] == (int)CellState.Unexplored)
            {
                Field[coord.Row, coord.Col] = (int)CellState.Empty;
                IsDestroyedShip = false;
                return (false, targetShip);
            }
            else if (Field[coord.Row, coord.Col] == MarkIsAShip || Field[coord.Row, coord.Col] == MarkAShipInvisible)
            {
                Field[coord.Row, coord.Col] = (int)CellState.BurningShip;
                isHit = true;
            }
            else
            {
                throw new Exception("Error!!!.Ячейки с таким состоянием не существует. Это баг, надо проверять.");
            }

            if (targetShip != null)
            {
                --targetShip.CounterRemainingParts;
                if (targetShip.CounterRemainingParts > 0)
                {
                    IsDestroyedShip = false;
                    return (isHit, targetShip);
                }
                else
                {
                    Field[coord.Row, coord.Col] = (int)CellState.DestroyedShip;
                    MarkAroundEmpty(targetShip);
                    IsDestroyedShip = true;
                    ShipsList.Remove(targetShip);
                    --ShipsCounter;
                }
            }
            return (isHit, targetShip);
        }

        private void MarkAroundEmpty(Ship ship)
        {
            for (int i = 0; i < _around.GetLength(0); i++)
            {
                foreach (var coord in ship.GetAllCoordinates())
                {
                    if (IsNotValidBoundaries(coord.Row, coord.Col, i) || Field[coord.Row + _around[i][0], coord.Col + _around[i][1]] == (int)CellState.DestroyedShip)
                    {
                        continue;
                    }
                    if (Field[coord.Row + _around[i][0], coord.Col + _around[i][1]] == (int)CellState.BurningShip)
                    {
                        Field[coord.Row + _around[i][0], coord.Col + _around[i][1]] = (int)CellState.DestroyedShip;
                    }
                    else
                    {
                        Field[coord.Row + _around[i][0], coord.Col + _around[i][1]] = (int)CellState.Empty;
                    }
                }
            }
        }

        private bool IsNotValidBoundaries(int row, int col, int i) => row + _around[i][0] < 0 || row + _around[i][0] >= Rows ||
                    col + _around[i][1] < 0 || col + _around[i][1] >= Columns;

    }


}