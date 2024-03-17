using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Text;

namespace DBServer.Cache
{
    public class User
    {
        object LockObj = new object();

        public GameMoney GameMoneyObj = new GameMoney();
        public QuickSlot QuickSlotObj = new QuickSlot();


        public void Lock()
        {
            System.Threading.Monitor.Enter(LockObj);
        }

        public void UnLock()
        {
            System.Threading.Monitor.Exit(LockObj);
        }

        public void SaveDB(MySqlConnection sqlDB)
        {
            // 이 함수는 로그아웃할 때 사용할 예정이므로 lock을 걸지 않는다.
        }
    }
}
