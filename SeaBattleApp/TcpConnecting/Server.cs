using System;
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

        public Server()
        {
            (IPAddress ip, int port) = GetIpAdressAndPort();
            TheIpAdress = ip; ThePort = port;
        }

        public void Run()
        {
            var tcpListener = new TcpListener(TheIpAdress, ThePort);
            var words = new Dictionary<string, string>()
            {
                {"red", "красный" },
                {"blue", "синий" },
                {"green", "зеленый" }
            };
            try {
                tcpListener.Start();    // запускаем сервер
                Console.WriteLine("Сервер запущен. Ожидание подключений... ");

                while (true) {
                    // получаем подключение в виде TcpClient
                    using var tcpClient = tcpListener.AcceptTcpClient();
                    // получаем объект NetworkStream для взаимодействия с клиентом
                    var stream = tcpClient.GetStream();
                    // буфер для входящих данных
                    var response = new List<byte>();
                    int bytesRead = 10;
                    while (true) {
                        // считываем данные до конечного символа
                        while ((bytesRead = stream.ReadByte()) != '\n') {
                            // добавляем в буфер
                            response.Add((byte)bytesRead);
                        }
                        var word = Encoding.UTF8.GetString(response.ToArray());

                        // если прислан маркер окончания взаимодействия,
                        // выходим из цикла и завершаем взаимодействие с клиентом
                        if (word == "END") break;

                        Console.WriteLine($"Запрошен перевод слова {word}");
                        // находим слово в словаре и отправляем обратно клиенту
                        if (!words.TryGetValue(word, out var translation)) translation = "не найдено в словаре";
                        // добавляем символ окончания сообщения 
                        translation += '\n';
                        // отправляем перевод слова из словаря
                        stream.Write(Encoding.UTF8.GetBytes(translation));
                        response.Clear();
                    }
                    break;
                }

            }
            catch (Exception ex) {
                Console.WriteLine("Что-то пошло не так");
            }
            finally {
                Console.WriteLine("Server stoped");
                tcpListener.Stop();
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
