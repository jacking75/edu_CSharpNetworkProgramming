using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Concurrent;

namespace MGAsyncNet
{
    /// <summary>
    /// 지정 범위 기반의 유니크 ID(번호) 할당기. 
    /// 동시에 같은 ID를 할당 받지 않는다. 그러나 재사용 될 수는 있다.
    /// 서버(프로세스) 마다 중복 되지 않는다.
    /// </summary>
    public class RangeedUniqueIdGenerator
    {
        ConcurrentBag<UInt64> UIDSet = new ConcurrentBag<UInt64>();
        UInt64 StartNumber = 1;
        UInt64 MaxCount = 1;

        
        public void Reset(UInt64 startNumber, UInt64 maxCount)
        {
            StartNumber = startNumber;
            MaxCount = maxCount;

            Generate();
        }

        public UInt64 Retrieve()
        {
            if(UIDSet.TryTake(out UInt64 result) == false)
            {
                return 0;
            }

            return result;
        }

        public bool Release(UInt64 UID)
        {
            UIDSet.Add(UID);
            return true;
        }


        void Generate()
        {
            var count = (UInt64)UIDSet.Count;

            for (UInt64 i = 0; i < count; ++i)
            {
                UIDSet.TryTake(out UInt64 result);
            }

            for (UInt64 i = StartNumber; i < MaxCount; ++i)
            {
                UIDSet.Add(i);
            }
        }
        
    }

}
