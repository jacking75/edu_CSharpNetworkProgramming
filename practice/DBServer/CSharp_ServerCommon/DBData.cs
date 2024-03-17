using System;
using System.Collections.Generic;
using System.Text;

namespace ServerCommon
{
    public class DBSlotInfo
    {
        public byte Index;
        public UInt16 SkillCode;
    }

    public class DBItem
    {
        public UInt32 Code;
        public Int32 BuyMoney;  // 아이템 구매 시 지불할 돈
        public Int32 BuyDiamond; // 아이템 구매 시 지불할 다이아몬드
    }
}
