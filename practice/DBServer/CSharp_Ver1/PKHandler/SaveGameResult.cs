using Dapper;
using MessagePack;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ServerCommon;


namespace DBServer.PKHandler
{
    class SaveGameResult : Base
    {
        public SaveGameResult(MySqlConnection mysqlConnection)
        {
            MySqlConnection = mysqlConnection;
        }

        public override void Process(MQPacketHeadInfo mqHead, byte[] mqData)
        {
            try
            {
                ProcessImpl(mqHead, mqData);
                Console.WriteLine("savegameresult !!!! ");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

       
        void ProcessImpl(MQPacketHeadInfo mqHead, byte[] mqData)
        {
            var senderServerIndex = mqHead.SenderIndex;
            var userUniqueId = mqHead.UserUniqueId;

            var reqData = MessagePackSerializer.Deserialize<MQNTFSaveGameResult>(mqData);

            var query = @"update OmokGameRecord set WinCount = CASE when UID = 
                                    @WinUserIndex then WinCount + 1 else WinCount end,
                                     LoseCount = CASE when UID = @LoseUserId then LoseCount+1  
                                    else LoseCount end where UserNo in (@WinUserIndex, @LoseUserId)";
            MySqlConnection.Execute(query,
               new
               {
                   WinUserIndex = reqData.WinUserUId,
                   LoseUserId = reqData.DefeatUserUId
               });
        }
    }
}
