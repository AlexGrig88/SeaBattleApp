using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeaBattleApp.Models
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
        public static Coordinate Parse(string coords)
        {
            coords = coords.ToUpper();
            int res = int.MaxValue;
            if (coords.Length == 2 && !int.TryParse(coords.Substring(0, 1), out res)) {
                throw new Exception("Error Parsing coordinate!");
            }
            else if (coords.Length == 3 && !int.TryParse(coords.Substring(0, 2), out res)) {
                throw new Exception("Error Parsing coordinate!");
            }
            else {
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

        public string ToSimpleString() => "" + Row + Col;
        public static Coordinate FromSimpleString(string rowCol) => new Coordinate(int.Parse(new string(rowCol[0], 1)), int.Parse(new string(rowCol[1], 1)));
    }
}
