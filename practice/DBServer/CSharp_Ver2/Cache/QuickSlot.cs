using ServerCommon;

using System;
using System.Collections.Generic;
using System.Text;

using MySqlConnector;



namespace DBServer.Cache
{
    // DB 저장 정책
    // 로그 아웃할 때 
    // 이전에 저장했을 때와 비굑해서 지정 시간이 지난 경우
    public class QuickSlot
    {
        const int MAX_SLOT_COUNT = 10; // 슬롯 최대 크기
        const int UPDATE_INTERVAL_TIME_MINUTES = 30; //업데이트 간격. 분 단위

        List<DBSlotInfo> SlotList = new List<DBSlotInfo>();

        DateTime LastUpdateTime = DateTime.Now;

        public QuickSlot()
        {
            for(int i = 0; i < MAX_SLOT_COUNT; ++i)
            {
                SlotList.Add(new DBSlotInfo());
            }
        }

        public DBPolicy UpdateSlot(DBSlotInfo slot)
        {
            var index = slot.Index;
            SlotList[index] = slot;

            if(DateTime.Now.Subtract(LastUpdateTime.Date).TotalMinutes >= UPDATE_INTERVAL_TIME_MINUTES)
            {
                LastUpdateTime = DateTime.Now;
                return DBPolicy.UPDATE;
            }

            return DBPolicy.PASS;
        }

        public void SaveDB(MySqlConnection sqlDB)
        {
            // DB 저장 쿼리 실행
        }
    }

    
}
