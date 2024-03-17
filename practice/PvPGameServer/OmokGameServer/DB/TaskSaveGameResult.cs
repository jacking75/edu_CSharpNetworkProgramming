using MessagePack;
using MySqlConnector;

using System;
using System.Collections.Generic;
using System.Text;

namespace PvPGameServer.DB
{
    public class TaskSaveGameResult : TaskBase
    {
        public override void Process(MySqlConnection MysqlConnection, byte[] msgData) 
		{ 
		}
       
    }
}
