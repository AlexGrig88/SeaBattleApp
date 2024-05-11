﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace SeaBattleApp.TcpConnecting
{
    public class Server
    {
        Regex _ipV4Regex = new Regex(@"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}");
        public int ThePort { get; set; }
        public IPAddress TheIpAdress { get; set; }
        private TcpListener _tcpListener;
        private NetworkStream _stream;

        public Server()
        {
            (IPAddress ip, int port) = GetIpAdressAndPort();
            TheIpAdress = ip; ThePort = port;
        }

        public bool TryStart()
        {
            _tcpListener = new TcpListener(TheIpAdress, ThePort);
            try {
                _tcpListener.Start();
                // получаем подключение в виде TcpClient
                var tcpClient = _tcpListener.AcceptTcpClient();
                // получаем объект NetworkStream для взаимодействия с клиентом
                _stream = tcpClient.GetStream();
                return true;
            }
            catch (Exception) {
                _tcpListener.Stop();
                return false;
            }
        }

        public string RunExchange(string battlefieldAsStr, Action<string> action)
        {
            try {
                    
                byte[] bufferInputData = new byte[Client.BUFFER_SIZE];    // максимальная длина буфера 300 
                action?.Invoke("Ждём когда соперник расставит корабли...");
                // считываем данные
                int resBytes = _stream.Read(bufferInputData);
                var opponentBattlefieldStr = Encoding.ASCII.GetString(bufferInputData).Trim('.'); // очищаем неинформационные данные
                        
                battlefieldAsStr = opponentBattlefieldStr.PadRight(Client.BUFFER_SIZE, '.');
                _stream.Write(Encoding.ASCII.GetBytes(battlefieldAsStr));
                return opponentBattlefieldStr;
            }
            catch (Exception ex) {
                action?.Invoke("Что-то пошло не так. Сервер остановлен.");
                _tcpListener.Stop();
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

        public (IPAddress, int) GetIpAdressAndPort()
        {
            IPAddress ipV4 = IPAddress.Parse("255.255.255.255");
            IPAddress[] addresses = Dns.GetHostAddresses(Dns.GetHostName());
            foreach (IPAddress address in addresses) {
                if (_ipV4Regex.IsMatch(address.ToString())) {
                    ipV4 = address;
                    break;
                }
            }
            int port = new Random().Next(50000, 65000);
            return (ipV4, port);
        }
    }
}