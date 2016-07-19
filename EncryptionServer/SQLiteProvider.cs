using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;
using System.Data.SQLite;
using System.Data;

namespace EncryptionServer
{
    class SQLiteProvider
    {
       const  string baseName = "encryption_db.db3";
        const string path = "";

        public char GetSymbol(char symbol)
        {
            SQLiteFactory factory = (SQLiteFactory)DbProviderFactories.GetFactory("System.Data.SQLite");
            using (SQLiteConnection connection = (SQLiteConnection)factory.CreateConnection())
            {
                connection.ConnectionString = "Data Source = " +path+ baseName;
                connection.Open();

                using (SQLiteCommand command = new SQLiteCommand(connection))
                {
                    command.CommandText = @"CREATE TABLE [workers] (
                    [id] integer PRIMARY KEY AUTOINCREMENT NOT NULL,
                    [name] char(100) NOT NULL,
                    [family] char(100) NOT NULL,
                    [age] int NOT NULL,
                    [profession] char(100) NOT NULL
                    );";
                    command.CommandType = CommandType.Text;
                  //  command.ExecuteReaderAsync
                    command.ExecuteNonQuery();
                }
            }

            return 'q';
        }

      
    }
}
