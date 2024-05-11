
namespace SeaBattleApp.Models
{

    public class Ship
    {
        const int MAX_LENGTH = 4;
        private int _length;  // колличество палуб
        private Coordinate _beginCoord;
        private int[][] around = new int[8][] {
                new int[] { 1, 1 }, new int[] {1, 0 }, new int[] {0, 1 }, new int[]{-1, -1},
                new int[]{-1, 0 }, new int[] {0, -1}, new int[] {1, -1 }, new int[] {-1, 1}
                };

        public bool IsHorizontalOrientation { get; set; }
        public bool IsDestroyed { get; set; }
        public int CounterRemainingParts { get; set; }
        public int Length
        {
            get => _length;
            set
            {
                if (value < 1) throw new Exception("ДЛИНА КОРАБЛЯ ДОЛЖНА БЫТЬ БОЛЬШЕ 0");
                if (value > MAX_LENGTH) throw new Exception($"ДЛИНА КОРАБЛЯ НЕ ДОЛЖНА БЫТЬ БОЛЬШЕ {MAX_LENGTH}");
                _length = value;
            }
        }
        public Coordinate BeginCoord   // top left coordinate
        {
            get => _beginCoord;
            set => _beginCoord = value;
        }

        public Ship(int length, bool isHorizontalOrientation = false)
        {
            Length = length;
            IsHorizontalOrientation = isHorizontalOrientation;
            IsDestroyed = false;
            CounterRemainingParts = length;
        }

        public Ship(Coordinate beginCoord, int length, bool isHorizontalOrientation = true)
        {
            BeginCoord = beginCoord;
            Length = length;
            IsHorizontalOrientation = isHorizontalOrientation;
            IsDestroyed = false;
            CounterRemainingParts = length;
        }

        public List<Coordinate> GetAllCoordinates()
        {
            var coords = new List<Coordinate>();
            for (int i = 0; i < Length; i++)
            {
                if (IsHorizontalOrientation)
                    coords.Add(new Coordinate(BeginCoord.Row, BeginCoord.Col + i));
                else
                    coords.Add(new Coordinate(BeginCoord.Row + i, BeginCoord.Col));
            }
            return coords;
        }

        public List<string> GetAllPositionsWithAround() => GetAllPositions().Union(GetAroundPositions()).ToList(); // избавляемся от повторяющихся позиций, которые попали через обнаружение в окружающих позициях

        public ISet<string> GetAllPositions() => GetAllCoordinates().Select(coord => coord.GetPosition()).ToHashSet();

        public ISet<string> GetAroundPositions()
        {
            var positions = new HashSet<string>();
            foreach (var pos in GetAllPositions())
            {
                foreach (var delta in around)
                {
                    string resPos = "";
                    char posChar = pos[^1];
                    if (pos.Length == 3)
                        resPos += $"{int.Parse(pos[..2]) + delta[0]}{(char)(posChar + delta[1] >= 'Й' ? posChar + delta[1] + 1 : posChar + delta[1])}";
                    else
                        resPos += $"{int.Parse(pos[..1]) + delta[0]}{(char)(posChar + delta[1] >= 'Й' ? posChar + delta[1] + 1 : posChar + delta[1])}";
                    positions.Add(resPos);
                }
            }
            return positions.Where(p => Game.regexValidPosition.IsMatch(p)).ToHashSet(); // отфильтровываем позиции за границей
        }

        public override bool Equals(object? obj)
        {
            //Check for null and compare run-time types.
            if (obj == null || !GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                Ship other = (Ship)obj;
                return Length == other.Length && BeginCoord.Row == other.BeginCoord.Row && BeginCoord.Col == other.BeginCoord.Col;
            }
        }

        public override int GetHashCode() => HashCode.Combine(Length, BeginCoord.Col, BeginCoord.Row);

        public string ToSimpleString() => IsHorizontalOrientation.ToString() + "," + IsDestroyed.ToString() + "," +
                CounterRemainingParts + "," + Length + "," + BeginCoord.ToSimpleString();
 
        public static Ship FromSimpleString(string shipsAsStr)
        {
            string[] res = shipsAsStr.Split(',');
            bool isHorizontalOrientation = Convert.ToBoolean(res[0]);
            bool isDestroyed = Convert.ToBoolean(res[1]);
            int counterRemainingParts = Convert.ToInt32(res[2]);
            int length = Convert.ToInt32(res[3]);
            Coordinate beginCoord = new Coordinate(Convert.ToInt32(res[4][..1]), Convert.ToInt32(res[4][1..]));
            var ship = new Ship(beginCoord, length, isHorizontalOrientation);
            ship.IsDestroyed = isDestroyed;
            ship.CounterRemainingParts = counterRemainingParts;
            return ship;
        }
    }
}