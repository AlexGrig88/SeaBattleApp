using SeaBattleApp.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Unicode;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SeaBattleApp.TcpConnecting
{
    public class Client
    {
        public const int BUFFER_SIZE_INIT_FIELD = 300;
        public const int BUFFER_SIZE_SHOT = 2;
        public int ThePort { get; set; }
        public string TheIpAdress { get; set; }
        private TcpClient _tcpClient;
        private NetworkStream _stream;
        public bool IsConnected
        {
            get => _tcpClient?.Connected ?? false;
        }
    

    public Client(string ip, int port)
        {
            TheIpAdress = ip;
            ThePort = port;
        }

        public bool TryConnect()
        {
            try {
                _tcpClient = new TcpClient();
                _tcpClient.Connect(TheIpAdress, ThePort);
                _stream = _tcpClient.GetStream();
                return true;
            }
            catch (Exception) {
                return false;
            }
        }

        /// <summary>
        /// Отправляет моё поле + счётчик не потопленных кораблей, в виде строки на сервер 2-го игрока для инициализации OpponentField.Field, OpponentField.ShipsCounter, OpponentField.ShipsList
        /// </summary>
        /// <param name="myField"></param>
        public string RunExchange(string myFieldAsStr, Action<string> action)
        {
            try {
                //NetworkStream stream = _tcpClient.GetStream();
                // буфер для входящих данных
                myFieldAsStr = myFieldAsStr.PadRight(BUFFER_SIZE_INIT_FIELD, '.');
                byte[] bufferResponse = new byte[BUFFER_SIZE_INIT_FIELD];

                _stream.Write(Encoding.ASCII.GetBytes(myFieldAsStr));     //отправляем наши данные (поле в строку + 2 числа в конце - это ShipsCounter, чтобы определить победителя (от 01 до 10))
                action?.Invoke("Ждём когда соперник расставит корабли...");
                _stream.Read(bufferResponse);
                return Encoding.ASCII.GetString(bufferResponse).Trim('.');
            }
            catch (Exception ex) {
                action?.Invoke("Что-то пошло не так! Соединение разорвано.");
                action?.Invoke(ex.Message);
                _tcpClient.Close();
                throw;
            }
        }

        public void Disconect()
        {
            try {
                _tcpClient.Close();
            }
            catch (Exception ex) {
                throw new ApplicationException(ex.Message);
            }
        }

        public void WritePositionToSecondPlayer(string position, Action<string> action)
        {
            try {
                position = Coordinate.Parse(position).ToSimpleString();    // т.к. у нас отправка фиксированной длину с 2-мя координатами в виде строки
                _stream.Write(Encoding.ASCII.GetBytes(position));     
            }
            catch (Exception ex) {
                action?.Invoke("Что-то пошло не так! Соединение разорвано.");
                action?.Invoke(ex.Message);
                _tcpClient.Close();
                throw;
            }
        }

        internal string ReadPositionFromSecondPlayer(Action<string>? action)
        {
            try {

                byte[] bufferResponse = new byte[BUFFER_SIZE_SHOT];
                action?.Invoke("Ждём когда соперник расставит корабли...");
                _stream.Read(bufferResponse);
                string coordAsStr = Encoding.ASCII.GetString(bufferResponse);
                return new Coordinate(int.Parse(coordAsStr[..1]), int.Parse(coordAsStr[1..])).GetPosition();
            }
            catch (Exception ex) {
                action?.Invoke("Что-то пошло не так! Соединение разорвано.");
                action?.Invoke(ex.Message);
                _tcpClient.Close();
                throw;
            }
        }
    }
}
