using System.Diagnostics.CodeAnalysis;
using System.Drawing;

namespace SeaBattleApp
{
    public struct Coordinate
    {
        public int Row { get; }
        public int Col { get; }
        public Coordinate(int row, int col)
        {
            Row = row; 
            Col = col;
        }
        public static Coordinate ParseRu(string coords) {
            coords = coords.ToUpper();
            int res = int.MaxValue;
            if (coords.Length == 2 && !int.TryParse(coords.Substring(0, 1), out res)) {
                throw new Exception("Error Parsing coordinate!");
            }
            else if (coords.Length == 3 && !int.TryParse(coords.Substring(0, 2), out res)) {
                throw new Exception("Error Parsing coordinate!");
            }
            else  {
                if (coords[^1] > 'И') return new Coordinate(res - 1, coords[^1] - 'А' - 1);
                else return new Coordinate(res - 1, coords[^1] - 'А');
            }
        }
        
        public string GetPosition()
        {
            string row = (Row + 1).ToString();
            char col = (BattleField.FIRST_CHAR_RU + Col) >= 'Й' ? (char)(BattleField.FIRST_CHAR_RU + Col + 1) : (char)(BattleField.FIRST_CHAR_RU + Col);
            return row + col;
        }
    }

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
        public int CounterRemainingParts {get; set; }
        public int Length
        {
            get => _length;
            set {
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

        public Ship(int length, bool isHorizontalOrientation = true)
        {
            Length = length;
            IsHorizontalOrientation = isHorizontalOrientation;
            IsDestroyed = false;
            CounterRemainingParts = length;
        }

        public List<Coordinate> GetAllCoordinates()
        {
            var coords = new List<Coordinate>();
            for (int i = 0; i < Length; i++) {
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
            foreach (var pos in GetAllPositions()) {
                foreach (var delta in around) {
                    string resPos = "";
                    char posChar = pos[^1];
                    if (pos.Length == 3)
                        resPos += ($"{int.Parse(pos[..2]) + delta[0]}{(char)(posChar + delta[1] >= 'Й' ? posChar + delta[1] + 1 : posChar + delta[1])}");
                    else
                        resPos += ($"{int.Parse(pos[..1]) + delta[0]}{(char)(posChar + delta[1] >= 'Й' ? posChar + delta[1] + 1 : posChar + delta[1])}");
                    positions.Add(resPos);
                }
            }
            return positions.Where(p => Game.regexValidPosition.IsMatch(p)).ToHashSet(); // отфильтровываем позиции за границей
        }

        public override bool Equals(object? obj)
        {
            //Check for null and compare run-time types.
            if ((obj == null) || ! this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else {
                Ship other = (Ship) obj;
                return (Length == other.Length) && (BeginCoord.Row == other.BeginCoord.Row) && (BeginCoord.Col == other.BeginCoord.Col);
            }
        }

        public override int GetHashCode() => HashCode.Combine(this.Length, this.BeginCoord.Col, this.BeginCoord.Row);
    }
}