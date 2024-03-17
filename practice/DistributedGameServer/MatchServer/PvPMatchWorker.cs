using MessagePack;
using Microsoft.Extensions.Logging;
using ServerCommon.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks.Dataflow;

namespace MatchServer
{
    class PvPMatchWorker
    {
        public Action<byte[]> SendToResponseWorkerFunc;


        ILogger Logger;

        bool IsThreadRunning = false;
        System.Threading.Thread ProcessThread = null;

        BufferBlock<byte[]> MsgBuffer = new BufferBlock<byte[]>();
        
        Dictionary<string, RequestPvPMatching> RequestMatchingMap = new ();
        

        public void Start(ILogger logger, ServerOption serverOpt)
        {
            Logger = logger;
            Logger.LogInformation("PvPMatchWorker::Start - begin");

            IsThreadRunning = true;
            ProcessThread = new System.Threading.Thread(this.Process);
            ProcessThread.Start();

            Logger.LogInformation("PvPMatchWorker::Start - end");
        }

        public void Destroy()
        {
            Logger.LogInformation("PvPMatchWorker::Destroy - begin");

            IsThreadRunning = false;
            MsgBuffer.Complete();

            ProcessThread.Join();
                        
            Logger.LogInformation("PvPMatchWorker::Destroy - end");
        }
        
        public void AddMatchingRequest(byte[] data)
        {
            MsgBuffer.Post(data);
        }

        void Process()
        {
            while (IsThreadRunning)
            {
                try
                {
                    Process_impl();
                }
                catch (Exception ex)
                {
                    if (IsThreadRunning)
                    {
                        Logger.LogError(ex.ToString());
                    }
                }
            }            
        }

        void Process_impl()
        {
            var reqData = MsgBuffer.Receive();
            var msgID = (MsgID)MsgPackHeaderInfo.ReadID(reqData);

            if (msgID == MsgID.RequestMatching)
            {
                ProcessRequestMatching(reqData);                
            }
            else if (msgID == MsgID.CancelMatching)
            {
                ProcessRequestCancelMatching(reqData);
            }
            else if (msgID == MsgID.ReloadGameServerInfo)
            {
                SendToResponseWorkerFunc(reqData);
            }
        }


        void ProcessRequestMatching(byte[] reqData)
        {
            var curReqMatching = MessagePackSerializer.Deserialize<RequestPvPMatching>(reqData);
            Logger.LogDebug($"[매칭 요청] User: {curReqMatching.UserID}, RatingScore:{curReqMatching.RatingScore}");

            // 현재 큐에 있는 요청자만으로 매칭을 다할 수 있을 때까지 반복적으로 매칭해야 한다. 
            // MsgBuffer.Receive() 호출 시 블럭킹 상태에 들어가기 때문이다.
            // 매칭이 더 이상 불가능하면 새로운 요청자가 있을 때까지는 매칭이 안되므로 매칭 로직이 멈추어도 괜찮다.
            RequestPvPMatching foundRequestMatching = null;
            foreach (var reqMatchingPair in RequestMatchingMap)
            {
                // 매칭 조건은 임시로 레이팅 차이 10 이내로 잡는다.
                if (Math.Abs(curReqMatching.RatingScore - reqMatchingPair.Value.RatingScore) <= 10)
                {
                    foundRequestMatching = reqMatchingPair.Value;
                    break;
                }
            }

            if (foundRequestMatching != null)
            {
                Logger.LogDebug($"[매칭 성사] User :{foundRequestMatching.UserID}, User: {curReqMatching.UserID}");

                // 매칭이 되면 ResponseMatching 함수를 호출한다
                var matchingResult = new UserPvPMatchingResult();
                matchingResult.UserList.Add(foundRequestMatching.UserID);
                matchingResult.UserList.Add(curReqMatching.UserID);
                var message = MessagePackSerializer.Serialize(matchingResult);
                MsgPackHeaderInfo.WriteID(message, MsgID.ResponseMatching);
                SendToResponseWorkerFunc(message);
                                
                RequestMatchingMap.Remove(foundRequestMatching.UserID);
            }
            else
            {
                // 매칭이 되지 않으면 매칭 대기리스트에 추가
                if (!RequestMatchingMap.ContainsKey(curReqMatching.UserID))
                {
                    RequestMatchingMap.Add(curReqMatching.UserID, curReqMatching);
                }
            }
        }

        void ProcessRequestCancelMatching(byte[] reqData)
        {
            var cancelMatching = MessagePackSerializer.Deserialize<RequestPvPMatching>(reqData);
            Logger.LogDebug($"[매칭 취소] User: {cancelMatching.UserID}");

            if (RequestMatchingMap.ContainsKey(cancelMatching.UserID))
            {
                RequestMatchingMap.Remove(cancelMatching.UserID);
            }
        }

    }    
}
