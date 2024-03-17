using MySqlConnector;

using System;
using System.Collections.Generic;
using System.Text;

namespace PvPGameServer.DB
{
    public class TaskBase
    {
        public virtual void Process(MySqlConnection MysqlConnection, byte[] msgData) { }


        
    }
}
