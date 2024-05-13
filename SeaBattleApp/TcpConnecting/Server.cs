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
            ThePort = 51000;
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

        public string ExchangeSelfFields(string myBattlefieldAsStr, Action<string> action)
        {
            try {
                    
                byte[] bufferInputData = new byte[Client.BUFFER_SIZE_INIT_FIELD];    // максимальная длина буфера 300 
                action?.Invoke("Жду данные от клиента (когда соперник расставит корабли)...");
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

        public string ReadShot(Action<string> action)
        {
            try {
                byte[] input = new byte[Client.BUFFER_SIZE_SHOT];    // максимальная длина буфера 300 
                action?.Invoke("Ждём, когда соперник сделает ход...");
                // считываем данные
                int resBytes = _stream.Read(input);

                string inputCoordStrFlag = Encoding.ASCII.GetString(input);
                // тут обработаем данные и отправим клиенту ответ
                string TheFlag = inputCoordStrFlag.Split(' ')[1];
                if (TheFlag == Game.HIT_FLAG) {
                    action?.Invoke("Соперник попал. Снова его ход.");
                    string output = inputCoordStrFlag.Replace(Game.HIT_FLAG, Game.ACK_FLAG);
                    _stream.Write(Encoding.ASCII.GetBytes(output));
                    return Coordinate.FromSimpleString(inputCoordStrFlag.Split(' ')[0]).GetPosition();
                }
                else if (TheFlag == Game.MISSED_FLAG) {
                    action?.Invoke("Соперник промахнулся.");
                    string output = inputCoordStrFlag.Replace(Game.MISSED_FLAG, Game.ACK_FLAG);
                    _stream.Write(Encoding.ASCII.GetBytes(output));
                    return Coordinate.FromSimpleString(inputCoordStrFlag.Split(' ')[0]).GetPosition();
                }
                else {
                    action?.Invoke("Игра окончена.");
                    _stream.Write(Encoding.ASCII.GetBytes(Game.ACK_FLAG + Game.ACK_FLAG));
                    return Coordinate.FromSimpleString(inputCoordStrFlag.Split(' ')[0]).GetPosition();
                }
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
