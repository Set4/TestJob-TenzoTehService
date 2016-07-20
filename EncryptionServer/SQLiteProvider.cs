using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;
using System.Data.SQLite;
using System.Data;
using System.IO;

namespace EncryptionServer
{
    class SQLiteProvider
    {

       

       const  string baseName = "encryption_db.db3";

        /// <summary>
        /// проверка и создание, заполнение бд при первом запуске
        /// </summary>
        public void CreateDB()
        {
            if (!File.Exists(baseName))
            {
                Console.WriteLine("Создание базы ключей, ожидайте");
                SQLiteConnection.CreateFile(baseName);
                CreateTable();
                AddItemasTable(EncryptionClass.Coding());
            }
        }

        /// <summary>
        /// заполнение таблицы шифрования
        /// </summary>
        /// <param name="code">словарь соответствия символов</param>
        private void AddItemasTable(Dictionary<char,char> code)
        {
            SQLiteFactory factory = (SQLiteFactory)DbProviderFactories.GetFactory("System.Data.SQLite");
            using (SQLiteConnection connection = (SQLiteConnection)factory.CreateConnection())
            {

                connection.ConnectionString = "Data Source = " + baseName;
                connection.Open();

                using (SQLiteCommand command = new SQLiteCommand(connection))
                {

                    foreach (KeyValuePair<char, char> item in code)
                    {
                        command.CommandText = "INSERT INTO encryption(oldsymbol, newsymbol) VALUES(@oldsymbol, @newsymbol);";
                        command.Parameters.Add(new SQLiteParameter("@oldsymbol", ""+item.Key+""));
                        command.Parameters.Add(new SQLiteParameter("@newsymbol", ""+item.Value+""));
                        command.ExecuteNonQuery();
                    }
                }
            }
        }


       /// <summary>
       /// создание таблицы шифрования
       /// </summary>
        private void CreateTable()
        {
            
            SQLiteFactory factory = (SQLiteFactory)DbProviderFactories.GetFactory("System.Data.SQLite");
            using (SQLiteConnection connection = (SQLiteConnection)factory.CreateConnection())
            {

                connection.ConnectionString = "Data Source = " + baseName;
                connection.Open();

                using (SQLiteCommand command = new SQLiteCommand(connection))
                {
                    command.CommandText = @"CREATE TABLE encryption (oldsymbol CHAR (1) PRIMARY KEY ON CONFLICT FAIL NOT NULL ON CONFLICT FAIL,
newsymbol CHAR (1) UNIQUE ON CONFLICT FAIL NOT NULL ON CONFLICT FAIL);";
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();
                }
            }
        }


      


        public char GetSymbol(char symbol, OperationRequest operation)
        {
            //если символ не найден в таблице он не шифруется
            char newsymbol= symbol;
            SQLiteFactory factory = (SQLiteFactory)DbProviderFactories.GetFactory("System.Data.SQLite");
            using (SQLiteConnection connection = (SQLiteConnection)factory.CreateConnection())
            {
               
                connection.ConnectionString = "Data Source = "+ baseName;
                connection.Open();

                using (SQLiteCommand command = new SQLiteCommand(connection))
                {
                    switch (operation)
                    {
                        case OperationRequest.Encoding:
                            command.CommandText = "SELECT newsymbol FROM encryption WHERE oldsymbol = @oldsymbol;";
                            command.Parameters.Add(new SQLiteParameter("@oldsymbol", "" + symbol + "")); break;

                        case OperationRequest.Decoding:
                            command.CommandText = "SELECT oldsymbol FROM encryption WHERE newsymbol = @newsymbol;";
                            command.Parameters.Add(new SQLiteParameter("@newsymbol", "" + symbol + "")); break;

                        default: return newsymbol;
                    }

                            var response=command.ExecuteScalar();
                    if (response != null && response is String)
                    {
                        string s = (string)response;
                        if (!String.IsNullOrWhiteSpace(s) && s.Length == 1)
                            newsymbol = s[0];
                    }              
                    //

                }
            }

            return newsymbol;
        }

     
    }
}
