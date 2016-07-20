using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EncryptionServer
{
    class ClientHandler
    {
        public TcpClient client;
        public ClientHandler(TcpClient tcpClient)
        {
            client = tcpClient;
        }

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

                    result = result.Concat(data).ToArray();

                 

                    }
                    while (stream.DataAvailable);

                Request rec = SerializationProvider.Deserialize(result);

                Response response = new EncryptionClass().Operation(rec);


                data = SerializationProvider.Serialize(response);


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
