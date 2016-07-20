using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EncryptionServer
{
    class Program
    {
       
        const int port = 8888;
        static TcpListener listener;

        
        static void Main(string[] args)
        {
            //проверка бд
            new SQLiteProvider().CreateDB();

            try
            {
                listener = new TcpListener(IPAddress.Parse("127.0.0.1"), port);
                listener.Start();
                Console.WriteLine("Ожидание подключений...");

                //слушатель
                while (true)
                {

                    //+ojidanie vvoda command ы console
                  
                    TcpClient client = listener.AcceptTcpClient();
  Console.WriteLine("Новое подключение");
                    ClientHandler clientObject = new ClientHandler(client);


                    Task task = Task.Factory.StartNew(clientObject.Process);
               
                   
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (listener != null)
                    listener.Stop();
            }
        }
    }
}
