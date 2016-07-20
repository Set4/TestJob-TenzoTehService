using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EncryptionServer
{
    /// <summary>
    /// работа с клиентом сервера
    /// </summary>
    class ClientHandler
    {
        public TcpClient client;
        public ClientHandler(TcpClient tcpClient)
        {
            client = tcpClient;
        }

        /// <summary>
        /// обработка соединения с клиентом(получение сообщения, кодирование, отпревка результата)
        /// </summary>
        public void Process()
        {
            NetworkStream stream = null;
            try
            {
                stream = client.GetStream();
                byte[] data = new byte[64]; // буфер для получаемых данных
             
                 
                 
                    byte[] result = new byte[0];
                    int bytes = 0;
                do
                {

                    bytes = stream.Read(data, 0, data.Length);

                    //~~
                    if (bytes < data.Length)
                    {
                        byte[] newdata = new byte[bytes];
                        Array.Copy(data, 0, newdata, 0, bytes);
                        result = result.Concat(newdata).ToArray();
                    }
                    else
                        result = result.Concat(data).ToArray();



                }
                while (stream.DataAvailable);

                Request rec = new XMLSerializationProvider().Deserialize(result);

                Response response = new EncryptionClass().Operation(rec);


                data = new XMLSerializationProvider().Serialize(response);


                    stream.Write(data, 0, data.Length);
              
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error: " + ex.Message);
            }
            finally
            {
                if (stream != null)
                    stream.Close();
              
            }
        }

     

    }
}
