using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeaBattleApp
{
    public class CompPlayer
    {

        public BattleField Field { get; set; }
        public List<string> AllPositions { get; set; }
        // private List<Pair> SavedPositions { get; set; }
        private (string pos, bool isHorizontal, bool isUpOrRight) SavedPos { get; set; }

        public CompPlayer(BattleField field)
        {
            Field = field;
            AllPositions = generateAllPositions(field.Rows, field.Columns);
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
                    positionsForDelete.Add(nextHPos);
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

        public string? GetAndSavePosition()
        {
            return null;
        }

    }
}
