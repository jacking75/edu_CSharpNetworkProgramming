using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PvPGameServer.Enum;


namespace PvPGameServer
{
    public class UserManager
    {
        public int MaxUserCount { get; private set; }
        
        Dictionary<string, User> UserMap = new Dictionary<string, User>();

        List<User> UserObjPool = new List<User>();
        Queue<int> UserObjIndexPool = new Queue<int>();

        public void Init(int maxUserCount)
        {
            MaxUserCount = maxUserCount;

            CreateUserObjPool(maxUserCount);
        }

        public ErrorCode AddUser(string sessionID)
        {
            // 들어갈 수 있는 방이 정해져 있고, 방에서 중복 검사를 하므로 여기서는 유저ID로 중복 검사는 하지 않는다

            if (UserMap.ContainsKey(sessionID))
            {
                return ErrorCode.ADD_USER_DUPLICATION;
            }

            var user = AllocUserObj();
            if (user == null)
            {
                return ErrorCode.LOGIN_FULL_USER_COUNT;
            }

            user.Use(sessionID);
            UserMap.Add(sessionID, user);
            return ErrorCode.None;
        }

        public ErrorCode SetLogin(string sessionID, string userID, int roomNum)
        {
            var user = GetUser(sessionID);
            if(user == null)
            {
                return ErrorCode.LOGIN_INVALID_SESSION_ID;
            }
                        
            user.SetLogin(userID, roomNum);            
            return ErrorCode.None;        
        }

        public ErrorCode RemoveUser(string sessionID)
        {
            var user = GetUser(sessionID);
            if (user == null)
            {
                return ErrorCode.REMOVE_USER_SEARCH_FAILURE_USER_ID;
            }

            UserMap.Remove(sessionID);

            var userIndex = user.Index;
            ReleaseUserObj(userIndex);

            return ErrorCode.None;
        }

        public User GetUser(string sessionID)
        {
            UserMap.TryGetValue(sessionID, out var user);
            return user;
        }

        public User GetUserByIndex(int index)
        {
            return UserObjPool[index];
        }

        // 접속 종료 예정. 이 유저는 지정된 시간 이내에 접속을 끊어야 한다.
        // 이 상태 이후 요청에 대한 응답도 하면 안 된다
        public void ReserveCloseNetwork(string sessionID, DateTime time)
        {
            if(UserMap.TryGetValue(sessionID, out var user))
            {
                user.SetReserveCloseNetwork(time);
            }
        }

      
        void CreateUserObjPool(int maxUserCount)
        {
            for (var i = 0; i < maxUserCount; ++i)
            {
                var user = new User();
                user.Init(i);

                UserObjIndexPool.Enqueue(i);
                UserObjPool.Add(user);
            }             
        }

        User AllocUserObj()
        {
            if(UserObjIndexPool.Count < 1)
            {
                return null;
            }

            var index = UserObjIndexPool.Dequeue();
            return UserObjPool[index];
        }

        void ReleaseUserObj(int index)
        {
            UserObjIndexPool.Enqueue(index);
            UserObjPool[index].Clear();
        }
    }

    
    
}
