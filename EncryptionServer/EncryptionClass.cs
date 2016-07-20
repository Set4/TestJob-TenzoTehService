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

namespace EncryptionServer
{
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
    public enum ResultResponse
    {
        Sucsesfull,
        Error
    }





    //zapros
    [Serializable]
    public class Request
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
    public enum OperationRequest
    {
        Encoding,
        Decoding
    }






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

        public Response Operation(Request rec)
        {
            if (rec==null||!_operations.ContainsKey(rec.Operation))
            {
                Debug.WriteLine(string.Format("Operation {0} is invalid", rec.Operation), "op");
                return new Response(ResultResponse.Error, String.Empty);
            }
            
            return new Response(ResultResponse.Sucsesfull, _operations[rec.Operation](rec.Message, rec.Operation));
        }






 private string DeEncoding(string message, OperationRequest operation)
        {
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < message.Length; i++)
            {
               result.Append( sql.GetSymbol(message[i], operation));
            }

            return result.ToString();
        }




        public static Dictionary<char, char> Coding()
        {
            return Coding(GetAllSymbol());
        }

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

        private static List<char> GetAllSymbol()
        {
            List<char> Alphabet = new List<char>();

            for (int i = 1040; i < 1104; i++)
            {
                Alphabet.Add((char)i);

                ////добавляем Ё
                //if (i == 1045)
                //    Alphabet.Add((char)1025);
            }
            return Alphabet;
        }

    }




    static class SerializationProvider
    {

        public static byte[] Serialize(Response data)
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


        public static Request Deserialize(byte[] data)
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
                            formatter.Binder = new Binder();
                        MemoryStream ms = new MemoryStream(data);
                        object re = formatter.Deserialize(ms);
                        return (Request)formatter.Deserialize(ms);
                    }
                    catch(Exception ex)
                    {
                        return null;

                    }
                }
            }
        }


    }
   
 
    public class Binder : SerializationBinder
    {
        public override Type BindToType(string i_AssemblyName, string i_TypeName)
        { Type typeToDeserialize = Type.GetType(String.Format(" {0}, {1}", i_TypeName, i_AssemblyName)); return typeToDeserialize; }
    }

}
