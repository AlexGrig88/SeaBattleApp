using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using SeaBattleApp.Models;

namespace SeaBattleApp.TcpConnecting
{
    public class Server
    {
        Regex _ipV4Regex = new Regex(@"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}");
        public int ThePort { get; set; }
        public IPAddress TheIpAdress { get; set; }
        private TcpListener _tcpListener;
        private NetworkStream _stream;
        public bool IsStarted { get; set; }

        public Server()
        {
            TheIpAdress = GetIpAdressAndPort();
            ThePort = new Random().Next(50000, 65000);
            IsStarted = false;
        }

        public bool TryStart()
        {
            _tcpListener = new TcpListener(TheIpAdress, ThePort);
            try {
                _tcpListener.Start();
                IsStarted = true;
                // получаем подключение в виде TcpClient
                var tcpClient = _tcpListener.AcceptTcpClient();
                // получаем объект NetworkStream для взаимодействия с клиентом
                _stream = tcpClient.GetStream();
                return true;
            }
            catch (Exception) {
                _tcpListener.Stop();
                IsStarted = false;
                return false;
            }
        }

        public string RunExchange(string myBattlefieldAsStr, Action<string> action)
        {
            try {
                    
                byte[] bufferInputData = new byte[Client.BUFFER_SIZE_INIT_FIELD];    // максимальная длина буфера 300 
                action?.Invoke("Ждём, когда соперник расставит корабли...");
                // считываем данные
                int resBytes = _stream.Read(bufferInputData);
                var opponentBattlefieldStr = Encoding.ASCII.GetString(bufferInputData).Trim('.'); // очищаем неинформационные данные

                myBattlefieldAsStr = myBattlefieldAsStr.PadRight(Client.BUFFER_SIZE_INIT_FIELD, '.');
                _stream.Write(Encoding.ASCII.GetBytes(myBattlefieldAsStr));

                return opponentBattlefieldStr;
            }
            catch (Exception ex) {
                action?.Invoke("Что-то пошло не так. Соединение прервалось.");
                _tcpListener.Stop();
                IsStarted=false;
                throw;
            }
        }

        public string ReadPositionFromSecondPlayer(Action<string> action)
        {
            try {
                byte[] bufferInputData = new byte[Client.BUFFER_SIZE_SHOT];
                action?.Invoke("\nЖдём, когда соперник сделает ход...\n");
                int resBytes = _stream.Read(bufferInputData);
                string coordAsStr = Encoding.ASCII.GetString(bufferInputData);
                return new Coordinate(int.Parse(coordAsStr[..1]), int.Parse(coordAsStr[1..])).GetPosition();
            }
            catch (Exception ex) {
                action?.Invoke("Что-то пошло не так. Соединение прервалось.");
                _tcpListener.Stop();
                IsStarted = false;
                throw;
            }
        }

        public void WritePositionToSecondPlayer(string coordStr, Action<string> action)
        {
            try {
                _stream.Write(Encoding.ASCII.GetBytes(coordStr));
            }
            catch (Exception ex) {
                action?.Invoke("Что-то пошло не так. Соединение прервалось.");
                _tcpListener.Stop();
                IsStarted = false;
                throw;
            }
        }


        public void Stop()
        {
            try {
                _tcpListener.Stop();
            }
            catch (Exception ex){
                throw;
            }
        }

        public IPAddress GetIpAdressAndPort()
        {
            IPAddress ipV4 = IPAddress.Parse("255.255.255.255");
            IPAddress[] addresses = Dns.GetHostAddresses(Dns.GetHostName());
            foreach (IPAddress address in addresses) {
                if (_ipV4Regex.IsMatch(address.ToString())) {
                    ipV4 = address;
                    break;
                }
            }
            return ipV4;
        }
    }
}
