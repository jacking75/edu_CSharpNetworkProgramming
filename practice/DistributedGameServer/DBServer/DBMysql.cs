using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Dapper;
using MySqlConnector;

namespace DBServer
{
    public class DBMysql
    {
        MySqlConnection DBConn;

        public bool Init(string connString)
        {
            try
            {
                DBConn = new MySqlConnection(connString);
                return true;
            }
            catch(Exception)
            {
                return false;
            }
        }

        
        public async Task<int> CreateUser(string userID, string userPW)
        {
            try
            {
                var val = await DBConn.ExecuteAsync("insert users(id, pw, reg_dt) values(@id, @pw, date_format(now(), '%Y/%c/%e'));", new { id = userID, pw = userPW });
                return val;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return -1;
            }
        }

        public async Task<bool> LoginUser(string userID, string userPW)
        {
            try
            {
                var val = await DBConn.QuerySingleOrDefaultAsync<DBUser>("select * from users where id=@id", new { id = userID });

                if (val == null || val.pw != userPW)
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
    
        class DBUser
        {
            public string id = "";
            public string pw = "";
        }
    }
}
