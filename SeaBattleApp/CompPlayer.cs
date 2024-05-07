using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SeaBattleApp.Models;
using static SeaBattleApp.CompPlayer;

namespace SeaBattleApp
{
    public class CompPlayer
    {

        public record Direction(int Row, int Col);

        StreamWriter? writer = null;
        public Memory TheMemory { get; set; }
        public BattleField Field { get; set; }
        public List<string> AllPositionsForOpponent { get; set; }
        public List<string> AllPositionsComp { get; set; }
        public CompPlayer(BattleField field)
        {
            Field = field;
            AllPositionsForOpponent = generateAllPositions(field.Rows, field.Columns);
            AllPositionsComp = generateAllPositions(field.Columns, field.Rows);
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

        public void RestoreAllPositionsComp() => AllPositionsComp = generateAllPositions(Field.Rows, Field.Columns);

        public void ClearUnsablePositions(Ship? ship, bool isItPosionsOfComputerField)
        {
            if (ship != null && isItPosionsOfComputerField) {
                AllPositionsComp = AllPositionsComp.Except(ship.GetAllPositionsWithAround()).ToList();
            }
            else if (ship != null && !isItPosionsOfComputerField) {
                AllPositionsForOpponent = AllPositionsForOpponent.Except(ship.GetAllPositionsWithAround()).ToList(); // очищаем позиции вокруг корабля
            }
            else {
                throw new Exception("Не найден корабль. Ошибка логики программы. Исправляй!");
            }
        }

        internal string ComputeMove2(bool isFirstShotInLoop)
        {
            if (isFirstShotInLoop && TheMemory.PositionsInProcess.Count == 0) {  // клмпьютер стреляет первый раз на своём ходу и в данный момент нет ни одного раненного корабля
                return AllPositionsForOpponent[new Random().Next(0, AllPositionsForOpponent.Count)];
            }

            if (isFirstShotInLoop && TheMemory.PositionsInProcess.Count == 1) {   // клмпьютер стреляет первый раз, и есть в памяти инфа об одной подбитой 
                return GetRandomPosFromFourFreeDirection(TheMemory.PositionsInProcess[^1]);
            }
            else if (isFirstShotInLoop && TheMemory.PositionsInProcess.Count > 1) {     // если начали стрелять на новом ходу и у нас больше одного ранения
                return ShootInTheSameDirectionInLine(TheMemory.PositionsInProcess[^1]);                 // значит стреляем вдоль одной линии
            }
            // ниже идёт череда непрерывных выстрелов
            else {
                if (TheMemory.PositionsInProcess.Count == 0) {       // если на первом выстреле он потопил однопалубный и стреляет второй раз                
                    return AllPositionsForOpponent[new Random().Next(0, AllPositionsForOpponent.Count)];
                }
                if (TheMemory.PositionsInProcess.Count == 1) {                       
                    return GetRandomPosFromFourFreeDirection(TheMemory.PositionsInProcess[^1]);
                }
                else {
                    return ShootInTheSameDirectionInLine(TheMemory.PositionsInProcess[^1]);
                }
            }
        }

        private string GetRandomPosFromFourFreeDirection(string lastShotPosition)   // надо проверить те позиции по которым уже стреляли (проверить список всех позиций и исследованные направления) и выстрелить
        {
            var direction = new Direction(100, 100);
            Coordinate coord = new Coordinate(100, 120);
            try {
                do {
                    int randIdx = new Random().Next(0, TheMemory.UnexploredDirection.Count);
                    direction = TheMemory.UnexploredDirection[randIdx];
                    TheMemory.UnexploredDirection.Remove(direction);
                    TheMemory.LastDirection = direction;
                    coord = Coordinate.Parse(lastShotPosition);

                } while (IsOutsideTheField(coord, TheMemory.LastDirection) || !IsFreeDirection(coord, TheMemory.LastDirection));
            }
            catch (Exception ex) {
                Console.WriteLine("???????" + ex.Message + "???????????????????????");
                throw new Exception(ex.Message);
            }
            finally {
/*                writer.Flush();
                writer.Close();*/
            }
            return new Coordinate(coord.Row + TheMemory.LastDirection.Row, coord.Col + TheMemory.LastDirection.Col).GetPosition();
        }

        private bool IsOutsideTheField(Coordinate coord, Direction direction) => coord.Row + direction.Row > 9 || coord.Col + direction.Col > 9 ||
                   coord.Row + direction.Row < 0 || coord.Col + direction.Col < 0;

        private bool IsFreeDirection(Coordinate coord, Direction direction) // проверяем, что в списке всех позиций нет новой рассчитаной позиции
        {
            var adjacentPos = new Coordinate(coord.Row + direction.Row, coord.Col + direction.Col).GetPosition();
            if (AllPositionsForOpponent.Contains(adjacentPos)) return true;
            return false;
        }

        private string ShootInTheSameDirectionInLine(string position)
        {
            Coordinate lastCoord = Coordinate.Parse(position);
            if (IsOutsideTheField(lastCoord, TheMemory.LastDirection) || 
                !IsFreeDirection(lastCoord, TheMemory.LastDirection)) return FromTheInitialShotMoveInReverseDirection();       // если не корректно, то вернуться в начало и в обратную сторону
            return new Coordinate(lastCoord.Row + TheMemory.LastDirection.Row, lastCoord.Col + TheMemory.LastDirection.Col).GetPosition();
        }

        private string FromTheInitialShotMoveInReverseDirection()
        {
            var firstPos = TheMemory.PositionsInProcess[0];         // позицию достаём из начала списка - это первое попадание
            Coordinate coord = Coordinate.Parse(firstPos);          // здесь проверять границу не надо, т.к. это единственное направление куда нам осталось двинуться
            TheMemory.LastDirection = new Direction(-TheMemory.LastDirection.Row, -TheMemory.LastDirection.Col);
            return new Coordinate(coord.Row + TheMemory.LastDirection.Row, coord.Col + TheMemory.LastDirection.Col).GetPosition();
        }
    }

    public class Memory
    {
        private int[][] _fourDirection = new int[4][] { new int[] { 0, 1 }, new int[] { 0, -1 }, new int[] { 1, 0 }, new int[] { -1, 0 } };
        public Direction LastDirection { get; set; }
        public List<Direction> UnexploredDirection { get; set; }
        public IList<string> PositionsInProcess { get; set; }  // Координаты с попаданиями

        public Memory()
        {
            PositionsInProcess = new List<string>();
            Reset();
        }

        public void Reset()
        {
            LastDirection = new Direction(0, 0);
            UnexploredDirection = new List<Direction>() {
                    new Direction(1, 0),
                    new Direction(-1, 0),
                    new Direction(0, 1),
                    new Direction(0, -1)
                };
            PositionsInProcess.Clear();
        }
    }

}
