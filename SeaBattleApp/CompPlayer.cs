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

        public class Memory
        {
            private int[][] _fourDirection = new int[4][] { new int[] { 0, 1 }, new int[] { 0, -1 }, new int[] { 1, 0 }, new int[] { -1, 0 } };
            public int[] LastDirectionInLine { get; set; } 
            public List<int[]> UnexploredDirection { get; set; }
            public IList<string> PositionsInProcess { get; set; }  // Координаты с попаданиями

            public Memory()
            {
                PositionsInProcess = new List<string>();
                Reset();
            }

            public void Reset()
            {
                LastDirectionInLine = new int[] { 0, 0 };
                UnexploredDirection = new List<int[]>();
                foreach (int[] direction in _fourDirection) {
                    UnexploredDirection.Add(direction);
                }
                PositionsInProcess.Clear();
            }

        }
        StreamWriter writer = null;
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
            writer = new StreamWriter(@"C:\Users\UserGrig\CSharpProjects\SeaBattleGit\SeaBattleProject\SeaBattleApp\records.txt", true);
            writer.WriteLine($"{new string('=', 50)}\nЗаход в метод GetRandomPosFromFourFreeDirection:\n {nameof(lastShotPosition)} = {lastShotPosition}");
            int[] direction = { 100, 120 };
            Coordinate coord = new Coordinate(100, 120);
            int loopCounter = 1;
            try {
                do {
                    writer.WriteLine($"Заход в цикл номер {loopCounter++}");
                    int randIdx = new Random().Next(0, TheMemory.UnexploredDirection.Count);
                    writer.WriteLine($"randIdx = {randIdx}; UnexploredDirection.Count = {TheMemory.UnexploredDirection.Count}");
                    direction = TheMemory.UnexploredDirection[randIdx];
                    TheMemory.UnexploredDirection = TheMemory.UnexploredDirection.Where(d => d[0] != direction[0] || d[1] != direction[1]).ToList();
                    WriteDirectionsToFile(writer, TheMemory.UnexploredDirection);
                    TheMemory.LastDirectionInLine = direction;
                    coord = Coordinate.Parse(lastShotPosition);
                    writer.WriteLine($"direction row = {direction[0]}; direction col = {direction[1]}");
                } while (IsOutsideTheField(coord, direction) || !IsFreeDirection(coord, direction));
            }
            catch (Exception ex) {
                Console.WriteLine("???????" + ex.Message + "???????????????????????");
                writer.WriteLine("???????" + ex.Message + "???????????????????????");
                writer.WriteLine("@@@@@@@ All positions @@@@@@@@");
                foreach (var item in AllPositionsForOpponent)
                {
                    writer.Write(item + " ");
                }
                return AllPositionsForOpponent[new Random().Next(0, AllPositionsForOpponent.Count)];  // Этот костыль сломал всю логику, появилось новое исключение выхода за границу в классе Battlefield!!!!
            }
            finally {
                writer.Flush();
                writer.Close();
            }

            return new Coordinate(coord.Row + direction[0], coord.Col + direction[1]).GetPosition();
            
        }

        private static void WriteDirectionsToFile(StreamWriter writer, List<int[]> dirs)
        {
            writer.WriteLine("++++++ Print remainder directions After filtering +++++++");
            foreach (var item in dirs)
            {
                writer.WriteLine($"dir[0] = {item[0]}; dir[1] = {item[1]}");
            }
            writer.WriteLine("+++++++++++++");
        }

        private bool IsOutsideTheField(Coordinate coord, int[] direction) => coord.Row + direction[0] > 9 || coord.Col + direction[1] > 9 ||
                   coord.Row + direction[0] < 0 || coord.Col + direction[1] < 0;

        private bool IsFreeDirection(Coordinate coord, int[] direction) // проверяем, что в списке всех позиций нет новой рассчитаной позиции
        {
            var adjacentPos = new Coordinate(coord.Row + direction[0], coord.Col + direction[1]).GetPosition();
            if (AllPositionsForOpponent.Contains(adjacentPos)) return true;
            return false;
        }

        private string ShootInTheSameDirectionInLine(string position)
        {
            Coordinate lastCoord = Coordinate.Parse(position);
            if (IsOutsideTheField(lastCoord, TheMemory.LastDirectionInLine) || 
                !IsFreeDirection(lastCoord, TheMemory.LastDirectionInLine)) return FromTheInitialShotMoveInReverseDirection();       // если не корректно, то вернуться в начало и в обратную сторону
            return new Coordinate(lastCoord.Row + TheMemory.LastDirectionInLine[0], lastCoord.Col + TheMemory.LastDirectionInLine[1]).GetPosition();
        }

        private string FromTheInitialShotMoveInReverseDirection()
        {
            var firstPos = TheMemory.PositionsInProcess[0];         // позицию достаём из начала списка - это первое попадание
            Coordinate coord = Coordinate.Parse(firstPos);          // здесь проверять границу не надо, т.к. это единственное направление куда нам осталось двинуться
            TheMemory.LastDirectionInLine[0] = -TheMemory.LastDirectionInLine[0];
            TheMemory.LastDirectionInLine[1] = -TheMemory.LastDirectionInLine[1];
            return new Coordinate(coord.Row + TheMemory.LastDirectionInLine[0], coord.Col + TheMemory.LastDirectionInLine[1]).GetPosition();
        }
    }
}
