using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeaBattleApp
{
    public class CompPlayer
    {
        // Hit - попадание в корабль
        public class Memory
        {
            private int[][] _fourDirection = new int[4][] { new int[]{0, 1}, new int[] {0, -1}, new int[] {1, 0}, new int[] {-1, 0} };
            public bool IsHitFirst { get; set; }            // первое попадание?
            public bool IsHitContinuous { get; set; }       // неприрывные попадания?
            public bool IsDestroyedShip { get; set; }       // корабль уничтожен
            public bool DirectionIsDefined { get; set; }    // определено ли направление движения вдоль прямой? (если есть 2 попадания, то да)
            public Coordinate FirstHitCoord { get; set; }
            public int HitCounter { get; set; } = 0;

            public string LastPosition { get; set; } 
            public int[] LastDirection { get; set; } 
            //public List<int[]> ExploredDirection { get; set; } = new List<int[]>();
            public List<int[]> UnexploredDirection { get; set; }
            public List<Coordinate> InProgressCoordinates { get; set; }  // Координаты с попаданиями


            public Memory()
            {
                Reset();
            }

            public void Reset()
            {
                FirstHitCoord = new Coordinate(-1, -1);
                IsHitFirst = true;
                IsHitContinuous = false;
                IsDestroyedShip = false;
                DirectionIsDefined = false;
                LastDirection = new int[] { 0, 0 };
                LastPosition = string.Empty;
                HitCounter = 0;
                UnexploredDirection = new List<int[]>();
                foreach (int[] direction in _fourDirection) {
                    UnexploredDirection.Add(direction);
                }
            }

            public string GetRandomInFourDirection(string pos)  // получение новой рандомной координаты из 4-ёх взаимно перпендикулярных направлений
            {
                /*                int randIdx = new Random().Next(0, _fourDirection.Length);
                                LastDirection = _fourDirection[randIdx];*/
                LastDirection = UnexploredDirection[new Random().Next(0, UnexploredDirection.Count)];
                UnexploredDirection.Remove(LastDirection);
                //ExploredDirection.Add(LastDirection);
                Coordinate coord = Coordinate.Parse(pos);
                if (IsNotValidDirection(coord)) return GetRandomInFourDirection(pos);  // если вышли за границы, то рекурсивно заново запускаем метод с оставшимися неисследованными направлениями
                LastPosition = new Coordinate(coord.Row + LastDirection[0], coord.Col + LastDirection[1]).GetPosition();
                return LastPosition;
            }

            public string FromTheInitialHitMoveInReverseDirection(string position)
            {
                // берём последнее направление и инвертируем и двигаемся дальше
                LastDirection[0] = -LastDirection[0];
                LastDirection[1] = -LastDirection[1];
                LastPosition = FirstHitCoord.GetPosition();
                return MoveInTheSameDirection(LastPosition);
            }

            public string MoveInTheSameDirection(string position)
            {
                Coordinate lastCoord = Coordinate.Parse(LastPosition);
                if (IsNotValidDirection(lastCoord)) return FromTheInitialHitMoveInReverseDirection(position); // если упёрлись в границу, то вернуться в начало и в обратную сторону
                string positionNew = new Coordinate(lastCoord.Row + LastDirection[0], lastCoord.Col + LastDirection[1]).GetPosition();
                LastPosition = positionNew;
                return positionNew;
            }

            internal string GetPositionFromRemainingDirections(string position)
            {
                /*                int[]? newDirection = UnexploredDirection[0];
                                foreach (var explored in ExploredDirection) {
                                    foreach (var dir in _fourDirection) {
                                        if ((dir[0] != explored[0]) || (dir[1] != explored[1])) {
                                            newDirection = dir;
                                            break;
                                        }
                                    }
                                }
                                LastDirection = newDirection?? new int[] { 0, 0 };
                                ExploredDirection.Add(LastDirection);*/
                LastDirection = UnexploredDirection[0];
                UnexploredDirection.Remove(LastDirection);
                Coordinate coord = Coordinate.Parse(position);
                if (IsNotValidDirection(coord)) return GetPositionFromRemainingDirections(position);
                LastPosition = new Coordinate(coord.Row + LastDirection[0], coord.Col + LastDirection[1]).GetPosition();
                return LastPosition;
            }

            private bool IsNotValidDirection(Coordinate coord) => coord.Row + LastDirection[0] > 9 || coord.Col + LastDirection[1] > 9 ||
                    coord.Row + LastDirection[0] < 0 || coord.Col + LastDirection[1] < 0;

        }

        public Memory TheMemory { get; set; }
        public BattleField Field { get; set; }
        public List<string> AllPositions { get; set; }
        private (string pos, bool isHorizontal, bool isUpOrRight) SavedPos { get; set; }
        private bool _wasChangeDirection = false;

        public CompPlayer(BattleField field)
        {
            Field = field;
            AllPositions = generateAllPositions(field.Rows, field.Columns);
            TheMemory = new Memory();
        }

        private List<string> generateAllPositions(int rows, int cols)
        {
            var positions = new List<string>(rows * cols);
            for (int i = 0, ch = BattleField.FIRST_CHAR_RU; i < rows; ++i)
            {
                for (int j = 1; j <= cols; j++)
                {
                    if (ch == 'Ё' || ch == 'Й') ++ch;
                    positions.Add($"{j}{(char)ch}");
                }
                ch += 1;
            }
            return positions;
        }

        public void RemoveUnvailablePositions(string pos, int len, bool isHorizontal)
        {
            var positionsForDelete = new HashSet<string>();
            positionsForDelete.Add(pos);
            addAllAroundPositions(positionsForDelete, pos);
            if (isHorizontal)
            {
                for (int i = 1; i < len; i++)
                {
                    var nextHPos = NextHorizontPos(pos, 1);
                    positionsForDelete.Add(NextHorizontPos(pos, 1));
                    addAllAroundPositions(positionsForDelete, nextHPos);
                }
            }
            else
            {
                for (int i = 1; i < len; i++)
                {
                    var nextVPos = NextVerticalPos(pos, 1);
                    positionsForDelete.Add(nextVPos);
                    addAllAroundPositions(positionsForDelete, nextVPos);
                }
            }
            AllPositions = AllPositions.Where(p => positionsForDelete.All(pos => pos != p)).ToList();
        }

        private void addAllAroundPositions(HashSet<string> positionsForDelete, string pos)
        {

            var nextHPos = NextHorizontPos(pos, 1);
            positionsForDelete.Add(nextHPos);
            positionsForDelete.Add(NextVerticalPos(nextHPos, 1));
            positionsForDelete.Add(NextVerticalPos(nextHPos, -1));
            var prevHpos = NextHorizontPos(pos, -1);
            positionsForDelete.Add(prevHpos);
            positionsForDelete.Add(NextVerticalPos(prevHpos, 1));
            positionsForDelete.Add(NextVerticalPos(prevHpos, -1));

            positionsForDelete.Add(NextVerticalPos(pos, 1));
            positionsForDelete.Add(NextVerticalPos(pos, -1));

        }

        private string NextHorizontPos(string pos, int delta)
        {
            if (delta == 0) return pos;
            char posChar = (char)(pos[pos.Length - 1] + delta);
            if (posChar == 'Ё' || posChar == 'Й') posChar = (char)(posChar + delta);
            return pos.Substring(0, pos.Length - 1) + posChar;
        }

        private string NextVerticalPos(string pos, int delta)
        {
            if (delta == 0) return pos;
            int posNum = Convert.ToInt32(pos.Substring(0, pos.Length - 1));
            return (posNum + delta).ToString() + pos[pos.Length - 1];
        }

        public void RestoreAllPositions()
        {
            AllPositions = generateAllPositions(Field.Rows, Field.Columns);
        }

        /// <summary>
        /// Метод рассчитывает новую позицию, когда состояние компьютера, что он в процессе доуничтожения подпитого судна
        /// т.е. у него в памяти есть координата отличная от (-1, -1)
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public string ComputeNewPosition(string position)
        {
            string positionNew = "";

            if (TheMemory.IsDestroyedShip) {
                TheMemory.Reset();       // сбросить память в начальное состояние
                return AllPositions[new Random().Next(0, AllPositions.Count)];
            }

            if (TheMemory.IsHitFirst) {
                TheMemory.IsHitFirst = false;
                TheMemory.FirstHitCoord = Coordinate.Parse(position);
                positionNew = TheMemory.GetRandomInFourDirection(position); 
            }   
            else {
                if (TheMemory.IsHitContinuous) {
                    //если зашли сюда, значит было два попадания подряд в одном ходе и значит продолжаем двигаться в том же направлении
                    positionNew = TheMemory.MoveInTheSameDirection(position);
                }
                else {
                    if (TheMemory.DirectionIsDefined && TheMemory.LastPosition == TheMemory.FirstHitCoord.GetPosition()) { // если направление уже было определено, но прервалась неприрывность попаданий, то вернёмся в начало и двинемся в противоположном направлении
                        positionNew = TheMemory.FromTheInitialHitMoveInReverseDirection(position);
                    } 
                    else if (TheMemory.DirectionIsDefined) {
                        TheMemory.MoveInTheSameDirection(position);
                    }
                    else {    // направление не определено, т.е. у нас есть только одно попадание, поэтому надо сначала вернуться в точку первого попадания и затем
                        position = TheMemory.FirstHitCoord.GetPosition(); // можно затереть пришедшую позициюю т.к. она была мимо и уже удалена из списка всех позиций
                        positionNew = TheMemory.GetPositionFromRemainingDirections(position); // выбираем направление из оставшихся после первого выстрела
                    }
                }
            }

            return positionNew;
        }

        private string GetPositionFromRemainingDirections(string position)
        {
            throw new NotImplementedException();
        }

/*        private string FromTheInitialHitMoveInOppositeDirection(string position) // аргумент нам тут не важен, т.к. мы возвращаемся в место первого попадания
        {
            // берём последнее направление и инвертируем и двигаемся дальше
            TheMemory.LastDirection[0] = -TheMemory.LastDirection[0];
            TheMemory.LastDirection[1] = -TheMemory.LastDirection[1];
            TheMemory.LastPosition = TheMemory.FirstHitCoord.GetPosition();
            return MoveInTheSameDirection(TheMemory.LastPosition);
        }*/

/*        public string MoveInTheSameDirection(string position)
        {
            Coordinate lastCoord = Coordinate.Parse(TheMemory.LastPosition);
            string positionNew =  new Coordinate(lastCoord.Row + TheMemory.LastDirection[0], lastCoord.Col + TheMemory.LastDirection[1]).GetPosition();
            TheMemory.LastPosition = positionNew;
            return positionNew;
        }*/

        internal string ComputeFirstMove()
        {
            // сначала проверить если до этого были попадания, то отправить в метод расчета новой позиции, если нет, то сгенерировать рандомную позицию
            if (TheMemory.LastPosition == string.Empty) {
                return AllPositions[new Random().Next(0, AllPositions.Count)];
            }
            return ComputeNewPosition(TheMemory.LastPosition);
            
        }

        public void ClearUnsablePositions(Ship? ship, string position)
        {
            if (ship != null && TheMemory.IsDestroyedShip) {
                AllPositions = AllPositions.Except(ship.GetAllPositionsWithAround()).ToList();
            }
            else {
                AllPositions.Remove(position);
                //throw new Exception("Не найден корабль. Ошибка логики программы. Исправляй!");
            }
        }
    }
}
