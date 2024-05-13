
using System;
using System.Text;
using System.Text.RegularExpressions;
using SeaBattleApp.Models;
using SeaBattleApp.TcpConnecting;
using static System.Formats.Asn1.AsnWriter;
using static SeaBattleApp.Game;

namespace SeaBattleApp
{
    public class Game
    {

        public const string HIT_FLAG = "Hit";
        public const string MISSED_FLAG = "Mis";
        public const string ACK_FLAG = "Ack";   // подтверждение полученного сообщения, не требует обработки на другой стороне
        public const string GAME_OVER = "Gor";
        // кто сервер а кто клиент определяется в зависимости от очередности хода. Если первый ходишь, то ты клиент
        public Server TheServer { get; set; } 
        public Client TheClient { get; set; }
        public bool IsClientPlayer { get; set; }    // клиент ходит первым
        public bool IsReadyServerData { get; set; }

        public event Action<Game> FieldStatusChangedEvent;
        public event Action<string> WriteMessageForPlayerEvent;

        public static Regex regexValidPosition = new Regex(@"^([1-9]|10)[А-ЕЖЗИК]$");

        public enum Mode { SinglePlayer, TwoPlayers }

        public Mode ModeGame { get; set; }

        public string Greeting => "Добро пожаловать на игру \"Морской бой!\"";
        public BattleField MyField { get; private set; }
        public BattleField OpponentField { get; private set; }
        public BattleField CurrentField { get; private set; }
        public CompPlayer TheCompPlayer { get; private set; }
        public Player Player1 { get; set; }


        public Game(Mode mode = Mode.SinglePlayer)
        {
            ModeGame = mode;
            MyField = new BattleField(true, 10, 10);
            OpponentField = new BattleField(false, 10, 10);
            CurrentField = MyField;
            Player1 = new Player(1, "Anon");
            TheClient = new Client("", 0);
            TheServer = new Server();
        }

        // не отрабатывает корректно, почему???
        public void ResetGameStatus()
        {
            OpponentField = new BattleField(false, 10, 10);
            MyField = new BattleField(false, 10, 10);
            CurrentField = MyField;
            Player1 = new Player(1, "Annon");
        }

        public void InitCompPlayer()
        {
            TheCompPlayer = new CompPlayer(OpponentField);
            PlaceOpponentShips();
        }

/*        public bool TrySynchronizeWithSecondPlayer()
        {
            
            WriteMessageForPlayerEvent?.Invoke("Ожидание получения соединения с серевером... ");
            for (int i = 0; i < 10; i++) {
                Thread.Sleep(1000);
                Console.Write('.');
            }
            if (TheClient.TryConnect()) {
                WriteMessageForPlayerEvent?.Invoke("\nСОЕДИНЕНИЕ ПРОИЗОШЛО.\n ");
                return true;
            }
            else {
                WriteMessageForPlayerEvent?.Invoke("Не удалось подключиться к серверу. Проверте правильность введёных ip и порта. Попробуйте сначала!");
                return false;
            }
            
        }*/

        public void ExecuteSettingOpponentBattlefield()
        {
            if (IsClientPlayer) {
                string myBattlefielAsStr = MyField.GetBattlefieldAsString();
                string opponentFieldAsStr = TheClient.ExchangeSelfFields(myBattlefielAsStr, WriteMessageForPlayerEvent);
                InitOpponentBattlefield(opponentFieldAsStr);
            }
            else {
                string myBattlefielAsStr = MyField.GetBattlefieldAsString();
                string opponentFieldAsStr = TheServer.ExchangeSelfFields(myBattlefielAsStr, WriteMessageForPlayerEvent);
                InitOpponentBattlefield(opponentFieldAsStr);
            }
        }

        public void ExecuteSettingOpponentBattlefield2()
        {
            if (IsClientPlayer) {
                string answer = TheClient.SetOpponentField(MyField.GetBattlefieldAsString(), WriteMessageForPlayerEvent);
                if (answer == ACK_FLAG) {
                    WriteMessageForPlayerEvent?.Invoke("Поле оппонента утсановлено.");
                }
            } else {
                string opponentFieldAsStr = TheServer.HandleSettingOpponentField(WriteMessageForPlayerEvent);
                InitOpponentBattlefield(opponentFieldAsStr);
            }
            IsClientPlayer = !IsClientPlayer;   
        }


        private void InitOpponentBattlefield(string opponentFieldAsStr)
        {
            string[] arr = opponentFieldAsStr.Split(';');
            OpponentField.Field = OpponentField.StringToField(arr[0]);  // Формат строки для передачи: battfield;ChipsCounter;False,False,4,4,99: ... и так до конца списка кораблей
            OpponentField.ShipsCounter = int.Parse(arr[1]);
            foreach (var shipStr in arr[2].Split(':')) {
                OpponentField.ShipsList.Add(Ship.FromSimpleString(shipStr));
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
            //WriteMessageForPlayerEvent?.Invoke("Вы не попали. Стреляет компьютер.");
            ComputerThinks();
            var isFirstShotInLoop = true;
            do {
                string selectedPosition = "";

                selectedPosition = TheCompPlayer.ComputeMove2(isFirstShotInLoop);
                WriteMessageForPlayerEvent?.Invoke($"Комп стреляет по позиции: {selectedPosition}");

                bool IsDestroyedShip = false;
                (bool isSuccess, Ship? ship) = TryShootAtTheTarget(Coordinate.Parse(selectedPosition), false, ref IsDestroyedShip);

                TheCompPlayer.AllPositionsForOpponent.Remove(selectedPosition);  // выстрел произошёл, можно очистить позицию из списка всех позиций у компьютера

                if (!isSuccess) {
                    WriteMessageForPlayerEvent?.Invoke("Компьютер промахнулся. Теперь ваш черед.");
                    break;
                }

                TheCompPlayer.TheMemory.PositionsInProcess.Add(selectedPosition); // успех выстрела, можно добавить в память компьютера данную розицию
                if (!IsDestroyedShip) {
                    WriteMessageForPlayerEvent?.Invoke("Соперник попал в ваш корабль. Думает куда дальше выстрелить...");

                    ComputerThinks();
                }
                else {
                    if (CurrentField.ShipsCounter == 0) {
                        WriteMessageForPlayerEvent?.Invoke("Увы! Вы проиграли, у вас не осталось ни одного корабля.");
                        ++Player1.DefeatCounter;
                        Player1.Score = 0;
                        isTheWinner = true;
                        return;
                    }
                    WriteMessageForPlayerEvent?.Invoke("Плохи дела. Компьютер потопил ваш корабль. Думает.");
                    // надо сбросить память компьютера в начальное состояние
                    ++TheCompPlayer.ShipLengthOpponentDict[ship?.Length ?? throw new ApplicationException("Ошибка! Проверяй логику!")];  // добавляем в память инфу о палубности потопленного корабля
                    TheCompPlayer.ClearUnsablePositions(ship, false);
                    TheCompPlayer.TheMemory.Reset();

                    ComputerThinks();
                    
                }
                isFirstShotInLoop = false;

            } while (true);
        }

        private void ComputerThinks()
        {
            Console.WriteLine("\nНе спеши, думаю.");
            for (int i = 0; i < 30; i++)
            {
                Console.Write(".");
                Thread.Sleep(3);
            }
            Console.WriteLine();
        }

        public void PlayerClientMove(ref bool isTheWinner)
        {
            do {
                string targetPosition = ReadValidPosition();
                bool shipIsDestroyed = false;
                bool isMyMove = true;
                (bool isSuccess, Ship? ship) = TryShootAtTheTarget(Coordinate.Parse(targetPosition), isMyMove, ref shipIsDestroyed);
                ++Player1.Score;
                if (!isSuccess) {
                    TheClient.WriteShot((Coordinate.Parse(targetPosition).ToSimpleString() + MISSED_FLAG), WriteMessageForPlayerEvent);
                    WriteMessageForPlayerEvent?.Invoke("Вы промахнулись. Ход переходит к соперникку.");
                    break;
                }

                TheClient.WriteShot((Coordinate.Parse(targetPosition).ToSimpleString() + HIT_FLAG), WriteMessageForPlayerEvent);
                if (!shipIsDestroyed) {
                    WriteMessageForPlayerEvent?.Invoke("Вы молодец, подбили корабль! Стреляйте ещё раз (введите координату)!!!");
                }
                else {
                    if (CurrentField.ShipsCounter == 0) {   
                        WriteMessageForPlayerEvent?.Invoke("О ДА!!! ВЫ ЖЕ ПОБЕДИЛИ!!!! КРАСАВЧИК!!!");
                        ++Player1.VictoryCounter;
                        isTheWinner = true;
                        return;
                    }
                    WriteMessageForPlayerEvent?.Invoke("УРА!!!!!!!!!!!!\nКорабль уничтожен!!!\nСтреляйте ещё раз (введите координату)!!!");
                }

            } while (true);

            IsClientPlayer = false;
        }

        public void PlayerServerMove(ref bool isTheWinner)
        {
            do {
                string targetPosition = TheServer.ReadShot(WriteMessageForPlayerEvent);
                bool shipIsDestroyed = false;
                bool isMyMove = true;
                (bool isSuccess, Ship? ship) = TryShootAtTheTarget(Coordinate.Parse(targetPosition), isMyMove, ref shipIsDestroyed);
                ++Player1.Score;
                if (!isSuccess) {
                    break;
                }
                if (!shipIsDestroyed) {
                    WriteMessageForPlayerEvent?.Invoke("Соперник попал в ваш корабль. Думает куда дальше выстрелить...");
                }
                else {
                    if (CurrentField.ShipsCounter == 0) {   
                        WriteMessageForPlayerEvent?.Invoke("Увы! Вы проиграли, у вас не осталось ни одного корабля.");
                        ++Player1.VictoryCounter;
                        isTheWinner = true;
                        return;
                    }
                    WriteMessageForPlayerEvent?.Invoke("Плохи дела. Компьютер потопил ваш корабль. Думает.");
                }

            } while (true);

            WriteMessageForPlayerEvent?.Invoke("Вы промахнулись. Ход переходит к соперникку.");
            IsClientPlayer = true;
        }

        public void PlayerMove(ref bool isTheWinner)
        {
            do {
                string targetPosition = ReadValidPosition();
                bool shipIsDestroyed = false;
                bool isMyMove = true;
                (bool isSuccess, Ship? ship) = TryShootAtTheTarget(Coordinate.Parse(targetPosition), isMyMove, ref shipIsDestroyed);
                ++Player1.Score;
                if (!isSuccess) {
                    WriteMessageForPlayerEvent?.Invoke("Вы промахнулись. Ход переходит к соперникку.");
                    break;
                }
                if (!shipIsDestroyed) {
                    WriteMessageForPlayerEvent?.Invoke("Вы молодец, подбили корабль! Стреляйте ещё раз (введите координату)!!!");
                }
                else {
                    if (CurrentField.ShipsCounter == 0) {   // ещё раз проверяем, что кораблей не осталось на поле
                        WriteMessageForPlayerEvent?.Invoke("О ДА!!! ВЫ ЖЕ ПОБЕДИЛИ!!!! КРАСАВЧИК!!!");
                        ++Player1.VictoryCounter;
                        isTheWinner = true;
                        return;
                    }
                    WriteMessageForPlayerEvent?.Invoke("УРА!!!!!!!!!!!!\nКорабль уничтожен!!!\nСтреляйте ещё раз (введите координату)!!!");
                }

            } while (true);

        }

        public string ReadValidPosition()
        {
            string coords = Console.ReadLine()?.ToUpper() ?? "DDD";
            while (!IsValidRuCoordinate(coords)) {
                WriteMessageForPlayerEvent?.Invoke("Координата не корректная!\nПопробуйте ещё раз!\n");
                coords = Console.ReadLine()?.ToUpper() ?? "DDD";
            }
            return coords;
        }

        public (bool, string) TryAddTheShipOnTheField(Ship ship, Coordinate coord) {
            if (!MyField.TryAddTheShip(ship, coord, out string errorMsg)) {
                return (false, errorMsg);
            };
            FieldStatusChangedEvent?.Invoke(this);
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
            FieldStatusChangedEvent?.Invoke(this);
            return (isSuccess, ship);    
        }

        /// <summary>
        /// Формат данных следующий: 
        /// Имя игрока: Иванов
        /// Наименьшее колличество выстрелов, за которое была унижтожена вражеская флотилия: 123
        /// Колличество побед: 100
        /// Колличество поражений: 100
        /// </summary>
        /// <param name="pathToDirectory"></param>
        /// <returns></returns>
        public string SaveOrUpdatePlayerStatistics()
        {
            var currDirectory = $"{DriveInfo.GetDrives()[0].Name}Users\\{Environment.UserName}\\Documents\\";
            if (!Directory.Exists(currDirectory)) throw new ApplicationException($"Директория {currDirectory} не найдена");
            //return CreateOrUpdateStatistics(currDirectory);
            var fullDir = $"{currDirectory}\\Sea Battle Game\\";
            if (!Directory.Exists(fullDir)) {
                Directory.CreateDirectory(fullDir);
            }
            var path = $"{fullDir}stat_{Player1.Username}.txt";
            if (!File.Exists(path)) {
                var file = File.Create(path);
                file.Close();
                File.WriteAllText(path, GetTextStat(new int[] { 100, 0, 0 })); // изначально очки равны максимуму, как худший результат
                return "Data saved";
            }
            int[] prevData = new int[3];
            int i = 0;
            foreach (var line in File.ReadAllLines(path))
            {
                if (line.Contains("Имя")) continue;
                prevData[i++] = int.Parse(line.Split(": ")[1]);
            }
            File.WriteAllText(path, GetTextStat(prevData));
            return "Data updated";
        }

        public string GetTextStat(int[] prevData)
        {
            if (Player1.Score != 0) {
                Player1.Score = Player1.Score < prevData[0] ? Player1.Score : prevData[0];
            }
            else {
                Player1.Score = prevData[0];
            }
            var username = $"Имя игрока: {Player1.Username}";
            var score = $"Наименьшее колличество выстрелов, за которое была унижтожена вражеская флотилия (100 - значит, ещё без побед): {Player1.Score}";
            var victories = $"Колличество побед: {Player1.VictoryCounter + prevData[1]}";
            var defeats = $"Колличество поражений: {Player1.DefeatCounter + prevData[2]}";
            return $"{username}\n{score}\n{victories}\n{defeats}\n";
        }

    }
}