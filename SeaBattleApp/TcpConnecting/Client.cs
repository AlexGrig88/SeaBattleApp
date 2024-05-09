using SeaBattleApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Unicode;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SeaBattleApp.TcpConnecting
{
    public class Client
    {
        public int ThePort { get; set; }
        public string TheIpAdress { get; set; }
        private TcpClient _tcpClient;

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
                return true;
            }
            catch (Exception ex) {
                return false;
            }
        }

        /// <summary>
        /// Отправляет моё поле + счётчик не потопленных кораблей, в виде строки на сервер 2-го игрока для инициализации OpponentField.Field и OpponentField.ShipsCounter
        /// </summary>
        /// <param name="myField"></param>
        public string SyncFields(string myFieldAsStr)
        {
            try {
                NetworkStream stream = _tcpClient.GetStream();
                // буфер для входящих данных
                byte[] opponentFieldResponse = new byte[myFieldAsStr.Length];

                stream.Write(Encoding.UTF8.GetBytes(myFieldAsStr));     //отправляем наши данные (поле в строку + 2 числа в конце - это ShipsCounter, чтобы определить победителя (от 01 до 10))
                stream.Read(opponentFieldResponse);
                return Encoding.UTF8.GetString(opponentFieldResponse);
            }
            catch (Exception ex) {
                Console.WriteLine("Что-то пошло не так! Соединение разорвано.");
                Console.WriteLine(ex.Message);
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
    }
}
