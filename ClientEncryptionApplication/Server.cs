using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ClientEncryptionApplication
{
    /// <summary>
    /// класс харанящий пару- запрос(к серверу)-ответ(от сервера) 
    /// </summary>
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

    /// <summary>
    /// ответ(от сервера) 
    /// </summary>
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
    /// <summary>
    /// резельтат операции на сервере
    /// </summary>
    [Serializable]
  public  enum ResultResponse
    {
        Sucsesfull,
        Error
    }





    /// <summary>
    /// запрос(к серверу) 
    /// </summary>
    [Serializable]
  public  class Request
    {
        public string Message { get; set; }
        public OperationRequest Operation { get; set; }

        public Request()
        {

        }

        public Request(OperationRequest operation, string message)
        {
            Operation = operation;
            Message = message;
        }
    }
    /// <summary>
    /// список оперпций
    /// </summary>
    [Serializable]
 public   enum OperationRequest
    {
        Encoding,
        Decoding
    }




    class XMLSerializationProvider:ISerializationProvider
    {
        public byte[] Serialize(Request data)
        {
            if (data == null)
            {
                return new byte[0];
            }
            else
            {
                try
                {
                    XmlSerializer formatter = new XmlSerializer(typeof(Request));
                    byte[] serialize;
                    using (MemoryStream stream = new MemoryStream())
                    {
                        formatter.Serialize(stream, data);
                        serialize = stream.ToArray();
                    }

                
                    return serialize;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    return null;

                }
            }
        }


        public Response Deserialize(byte[] data)
        {


            if (data == null)
            {
                return new Response(ResultResponse.Error, String.Empty);
            }
            else
            {
                if (data.Length == 0)
                {
                    return new Response(ResultResponse.Sucsesfull, String.Empty);
                }
                else
                {
                    try
                    {


                        Response rec;
                        XmlSerializer formatter = new XmlSerializer(typeof(Response));
                        using (MemoryStream stream = new MemoryStream(data))
                        {
                            rec = (Response)formatter.Deserialize(stream);
                        }

                        return rec;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);

                        //zagleshka
                        //   return new Response(ResultResponse.Sucsesfull, "zagleshka");
                        return new Response(ResultResponse.Error, String.Empty);

                    }
                }
            }
        }

    }

        class BinarySerializationProvider:ISerializationProvider
    {
       
        public byte[] Serialize(Request data)
        {
            if (data == null)
            {
                return new byte[0];
            }
            else
            {
                try
                {
                    using (MemoryStream stream = new MemoryStream())
                    {
                        new BinaryFormatter().Serialize(stream, data);
                        return Convert.FromBase64String(Convert.ToBase64String(stream.ToArray()));
                    }                  
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    return null;

                }
            }
        }


        public Response Deserialize(byte[] data)
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
                    try
                    {

                      
                        Response resp;
                        using (MemoryStream stream = new MemoryStream(data))
                        {
                            resp = (Response)new BinaryFormatter().Deserialize(stream);
                        }


                        return resp;
                    }
                      catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);

                        //zagleshka
                        return new Response(ResultResponse.Sucsesfull, "zagleshka");
                       // return null;

                    }
                }
            }
        }
    

    }
   
    public interface ISerializationProvider
    {
        /// <summary>
        /// сериализация запроса к серверу
        /// </summary>
        /// <param name="data">запрос к серверу</param>
        /// <returns>массив byte[]</returns>
        byte[] Serialize(Request data);
        /// <summary>
        ///  десериализация запроса от сервера
        /// </summary>
        /// <param name="data">массив byte[]-ответ сервера</param>
        /// <returns>ответ(от сервера)-type Response </returns>
        Response Deserialize(byte[] data);
    }

  
    class ServerHandler
    {
        const int port = 8888;
        const string address = "127.0.0.1";



        /// <summary>
        /// соединение с сервером
        /// </summary>
        /// <param name="request">запрос к серверу</param>
        /// <param name="serializ">класс сериализатор\десериализатор</param>
        /// <returns>DeEncryptionResult</returns>
        public async Task<DeEncryptionResult> Handler(Request request, ISerializationProvider serializ)
        {
           

            TcpClient client = null;
            try
            {
                client = new TcpClient(address, port);
                NetworkStream stream = client.GetStream();



                byte[] data = serializ.Serialize(request);
                // отправка сообщения
                await stream.WriteAsync(data, 0, data.Length);

                // получаем ответ
                data = new byte[1024]; // буфер для получаемых данных

                byte[] result = new byte[0];

              


              
                int bytes = 0;
                do
                {

                    bytes = await stream.ReadAsync(data, 0, data.Length);
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


                return new DeEncryptionResult(request, serializ.Deserialize(result));

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
