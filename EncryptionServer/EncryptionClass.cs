using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace EncryptionServer
{
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
    public enum ResultResponse
    {
        Sucsesfull,
        Error
    }





    /// <summary>
    /// запрос(к серверу) 
    /// </summary>
    [Serializable]
    public class Request
    {
            
        public string Message { get; set; }
        public OperationRequest Operation { get;  set; }

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
    public enum OperationRequest
    {
        Encoding,
        Decoding
    }





    /// <summary>
    ///операции кодирование и декодирование
    /// </summary>
     class EncryptionClass
    {
       
        private delegate string OperationDelegate(string message, OperationRequest operation);
        private Dictionary<OperationRequest, OperationDelegate> _operations;
        private SQLiteProvider sql;


        public EncryptionClass()
        {
            _operations =
                new Dictionary<OperationRequest, OperationDelegate>
                {
            { OperationRequest.Encoding, DeEncoding },
            {OperationRequest.Decoding, DeEncoding },

                };

            sql = new SQLiteProvider();
        }

        /// <summary>
        /// обработчик операции
        /// </summary>
        /// <param name="rec"></param>
        /// <returns></returns>
        public Response Operation(Request rec)
        {
            if (rec==null||!_operations.ContainsKey(rec.Operation))
            {
                Debug.WriteLine(string.Format("Operation {0} is invalid", rec.Operation), "op");
                return new Response(ResultResponse.Error, String.Empty);
            }
            
            return new Response(ResultResponse.Sucsesfull, _operations[rec.Operation](rec.Message, rec.Operation));
        }





        /// <summary>
        /// операция кодирования/декодирования
        /// </summary>
        /// <param name="message"></param>
        /// <param name="operation"></param>
        /// <returns></returns>
 private string DeEncoding(string message, OperationRequest operation)
        {
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < message.Length; i++)
            {
               result.Append( sql.GetSymbol(message[i], operation));
            }

            return result.ToString();
        }



        /// <summary>
        /// создание словаря кодирования
        /// </summary>
        /// <returns></returns>
        public static Dictionary<char, char> Coding()
        {
            return Coding(GetAllSymbol());
        }

        /// <summary>
        /// создание словаря кодирования
        /// </summary>
        /// <returns></returns>
        private static Dictionary<char, char> Coding(List<char> symbols)
        {
            Dictionary<char, char> code = new Dictionary<char, char>();

            //sozdaem smeshenie
            Random rnd = new Random();
            int s = rnd.Next(1, symbols.Count);

            //List<char> newsymbols = symbols.GetRange(s, symbols.Count - (s + 1));
            //newsymbols.AddRange(symbols.GetRange(0, s));

            for (int i = 0, j = i + s; i < symbols.Count; i++, j++)
            {
                code.Add(symbols[i], symbols[j]);
                if (i + s + 1 == symbols.Count)
                    j = -1;
            }
            return code;
        }

        /// <summary>
        /// получение списка символов
        /// </summary>
        /// <returns></returns>
        private static List<char> GetAllSymbol()
        {
            List<char> Alphabet = new List<char>();

            for (int i = 1040; i < 1104; i++)
            {
                Alphabet.Add((char)i);

               
            }
            return Alphabet;
        }

    }




    class XMLSerializationProvider: ISerializationProvider
    {


        public byte[] Serialize(Response data)
        {
            if (data == null)
            {
                return new byte[0];
            }
            else
            {
                try
                {

                    XmlSerializer formatter = new XmlSerializer(typeof(Response));
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


        public Request Deserialize(byte[] data)
        {


            if (data == null)
            {
                return null;
            }
            else
            {
                if (data.Length == 0)
                {
                    return null;
                }
                else
                {
                    try
                    {
                        Request rec;
                        XmlSerializer formatter = new XmlSerializer(typeof(Request));

                                           using (MemoryStream stream = new MemoryStream(data))
                        {
                            rec = (Request)formatter.Deserialize(stream);
                        }

                        return rec;
                  
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                        //временная заглушка
                        //return new Request(OperationRequest.Encoding, "ААА");

                        return null;
                    }
                }
            }
        }
    }

    class BinarySerializationProvider: ISerializationProvider
    {


      



        public byte[] Serialize(Response data)
        {
            if (data == null)
            {
                return new byte[0];
            }
            else
            {
                try
                {
                    MemoryStream streamMemory = new MemoryStream();
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(streamMemory, data);
                    return streamMemory.GetBuffer();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    return null;

                }
            }
        }


        public Request Deserialize(byte[] data)
        {


            if (data == null)
            {
                return null;
            }
            else
            {
                if (data.Length == 0)
                {
                    return null;
                }
                else
                {
                    try
                    {

                        BinaryFormatter formatter = new BinaryFormatter();
                          
                        MemoryStream ms = new MemoryStream(data);
                    
                        return (Request)formatter.Deserialize(ms);
                    }
                    catch(Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                        //временная заглушка
                        return new Request(OperationRequest.Encoding, "ААА");

                    }
                }
            }
        }


    }
   

    public interface ISerializationProvider
    {
        /// <summary>
        /// сериализация ответа к  клиенту
        /// </summary>
        /// <param name="data">запрос</param>
        /// <returns>массив byte[]</returns>
        byte[] Serialize(Response data);

        /// <summary>
        ///  десериализация запроса от клиента
        /// </summary>
        /// <param name="data">массив byte[]-запрос клиента</param>
        /// <returns>запрос-type Request </returns>
        Request Deserialize(byte[] data);
    }



   
}
