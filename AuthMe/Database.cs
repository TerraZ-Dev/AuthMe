using Mono.Data.Sqlite;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;
using TShockAPI.DB;

namespace AuthMe
{
    public class Manager
    {
        public static IDbConnection DB;
        internal static void Initailize()
        {
            IQueryBuilder builder = null;
            switch (TShock.Config.Settings.StorageType)
            {
                default:
                    return;

                case "mysql":
                    var hostport = TShock.Config.Settings.MySqlHost.Split(':');
                    DB = new MySqlConnection();
                    DB.ConnectionString = string.Format("Server={0}; Port={1}; Database={2}; Uid={3}; Pwd={4};",
                        hostport[0],
                        hostport.Length > 1 ? hostport[1] : "3306",
                        TShock.Config.Settings.MySqlDbName,
                        TShock.Config.Settings.MySqlUsername,
                        TShock.Config.Settings.MySqlPassword);
                    builder = new MysqlQueryCreator();
                    break;
                case "sqlite":
                    DB = new SqliteConnection(string.Format("uri=file://tshock//{0},Version=3", "AuthMe.sqlite"));
                    builder = new SqliteQueryCreator();
                    break;
            }

            SqlTable table = new SqlTable("AuthMe",
                new SqlColumn("From", MySqlDbType.Text),
                new SqlColumn("To", MySqlDbType.Text));

            new SqlTableCreator(DB, builder).EnsureTableStructure(table);
        }

        public static string GetFirstAccount(string toAccount)
        {
            using (var reader = DB.QueryReader("SELECT * FROM AuthMe WHERE To=@0", toAccount))
            {
                if (reader.Read())
                {
                    return reader.Get<string>("From");
                }
            }
            return "";
        }
        public static string GetSecondAccount(string fromAccount)
        {
            using (var reader = DB.QueryReader("SELECT * FROM AuthMe WHERE From=@0", fromAccount))
            {
                if (reader.Read())
                {
                    return reader.Get<string>("To");
                }
            }
            return "";
        }

        public static void Add(string from, string to)
        {
            DB.Query("INSERT INTO AuthMe VALUES(@0, @1)", from, to);
        }
        public static void Remove(string from)
        {
            DB.Query("DELETE FROM AuthMe WHERE From=@0", from);
        }
    }
}
