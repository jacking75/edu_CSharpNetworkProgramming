using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Text;

namespace DBServer.Cache
{
    // 돈, 다이아몬드 등의 재화

    // DB 저장 정책
    // 다이아몬드: 즉시
    // 돈: 이전 저장에 비해 지정 크기 이상의 변동이 있으면 저장한다.

    public class GameMoney
    {
        Int32 MAX_CHANGE_MONEY = 10_000;

        public Int64 OldMoney { get; private set; }
        public Int64 Money { get; private set; }
        public Int32 Diamond { get; private set; }

        public void Set(Int64 money, Int32 diamond)
        {
            OldMoney = Money = money;
            Diamond = diamond;
        }

        public DBPolicy UpdateMoney(Int64 money)
        {
            Money += money;

            if(Money < 0)
            {
                Money = 0;
            }

            if(Math.Abs(Money - OldMoney) > MAX_CHANGE_MONEY)
            {
                OldMoney = Money;
                return DBPolicy.UPDATE;
            }

            return DBPolicy.PASS;
        }

        public DBPolicy UpdateDiamond(Int32 diamond)
        {
            if(diamond <= 0)
            {
                return DBPolicy.PASS;
            }

            Diamond += diamond;

            if(Diamond < 0)
            {
                Diamond = 0;
            }

            return DBPolicy.UPDATE;
        }

        public void SaveDB(MySqlConnection sqlDB)
        {
            // DB 저장 쿼리 실행
        }

    }
}
