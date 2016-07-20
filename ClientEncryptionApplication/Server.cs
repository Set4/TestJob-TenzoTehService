using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace ClientEncryptionApplication
{

    public class DeEncryptionResult
    {
        public Response _Response { get; private set; }
        public Request _Request { get; private set; }

        public DeEncryptionResult(Request _Request, Response _Response)
        {
            this._Request = _Request;
            this._Response = _Response;
        }
    }


    //otvet
    [Serializable]
   public class Response
    {
        public string Message { get; set; }
        public ResultResponse Result { get; set; }

      public Response()
        { }

        public Response(ResultResponse result, string message)
        {
            Message = message;
            Result = result;
        }
    }
    [Serializable]
  public  enum ResultResponse
    {
        Sucsesfull,
        Error
    }





    //zapros
    [Serializable]
  public  class Request
    {
        public string Message { get; private set; }
        public OperationRequest Operation { get; private set; }

        public Request()
        {

        }

        public Request(OperationRequest operation, string message)
        {
            Operation = operation;
            Message = message;
        }
    }
    [Serializable]
 public   enum OperationRequest
    {
        Encoding,
        Decoding
    }


    static class SerializationProvider
    {
       
        public static byte[] Serialize(Request data)
        {
            if (data == null)
            {
                return new byte[0];
            }
            else
            {
                MemoryStream streamMemory = new MemoryStream();
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(streamMemory, data);
                return streamMemory.GetBuffer();
            }
        }


        public static Response Deserialize(byte[] data)
        {
 
          
            if (data == null)
            {
                return new Response(ResultResponse.Error, String.Empty);
            }
            else
            {
                if (data.Length == 0)
                {
                    return new Response( ResultResponse.Sucsesfull,String.Empty);
                }
                else
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    
                    MemoryStream ms = new MemoryStream(data);
                    return (Response)formatter.Deserialize(ms);
                }
            }
        }
    

    }
   


    class ServerHandler
    {
        const int port = 8888;
        const string address = "127.0.0.1";


      

        public async Task<DeEncryptionResult> Handler(Request request)
        {

            TcpClient client = null;
            try
            {
                client = new TcpClient(address, port);
                NetworkStream stream = client.GetStream();



                byte[] data = SerializationProvider.Serialize(request);
                // отправка сообщения
                await stream.WriteAsync(data, 0, data.Length);

                // получаем ответ
                data = new byte[1024]; // буфер для получаемых данных

                byte[] result = new byte[0];

              


              
                int bytes = 0;
                do
                {

                    bytes = await stream.ReadAsync(data, 0, data.Length);
                    //~~
                    result = result.Concat(data).ToArray();

                }
                while (stream.DataAvailable);


                return new DeEncryptionResult(request, SerializationProvider.Deserialize(result));

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return new DeEncryptionResult(request, new Response(ResultResponse.Error, "Произошла ошибка"));
            }
            finally
            {
                if (client != null)
                    client.Close();
            }
        }
    }

  
}
