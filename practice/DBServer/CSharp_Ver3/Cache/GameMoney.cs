using MySqlConnector;
using CloudStructures.Structures;

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

        MySqlConnection SqlDBConn;
        string RedisKeyGameMoney;

        public Int64 OldMoney { get; private set; }
        

        public void Init(MySqlConnection sqlDB, UInt64 uid, Int64 money, Int32 diamond)
        {
            SqlDBConn = sqlDB;
            OldMoney = money;
            SetCache(uid, money, diamond);
        }

        public DBPolicy UpdateMoneyDiamond(UInt64 uid, Int64 money, Int32 diamond)
        {
            var redisId = new RedisString<CacheGameMoney>(Manager.RedisConn, RedisKeyGameMoney, null);
            var cacheData = redisId.GetAsync().Result.Value;

            var dbPolicy = DBPolicy.PASS;

            var moneyUpdate = DBPolicy.PASS;
            if (money != 0)
            {
                moneyUpdate = UdateMoney(cacheData, money);
            }

            if(diamond != 0)
            {
                cacheData.Diamond += diamond;
            }


            if(moneyUpdate == DBPolicy.UPDATE || diamond != 0)
            {
                OldMoney = cacheData.Money;
                SaveDB(SqlDBConn);
                dbPolicy = DBPolicy.UPDATE;
            }
            
            SetCache(uid, money, diamond);
            
            return dbPolicy;
        }

        void SetCache(UInt64 uid, Int64 money, Int32 diamond)
        {
            RedisKeyGameMoney = $"UC_Money_{uid}";

            var saveData = new CacheGameMoney() { Money = money, Diamond = diamond };
            var redisId = new RedisString<CacheGameMoney>(Manager.RedisConn, RedisKeyGameMoney, null);
            redisId.SetAsync(saveData);
        }

        DBPolicy UdateMoney(CacheGameMoney cacheData, Int64 money)
        {
            cacheData.Money += money;

            if (cacheData.Money < 0)
            {
                cacheData.Money = 0;
            }

            if (Math.Abs(cacheData.Money - OldMoney) > MAX_CHANGE_MONEY)
            {
                return DBPolicy.UPDATE;
            }

            return DBPolicy.PASS;
        }
                
        public void SaveDB(MySqlConnection sqlDB)
        {
            // DB 저장 쿼리 실행
        }

        public class CacheGameMoney
        {
            public Int64 Money;
            public Int32 Diamond;
        }
    }
}
