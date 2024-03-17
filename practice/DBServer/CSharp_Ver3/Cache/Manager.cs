using MySqlConnector;
using CloudStructures;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace DBServer.Cache
{
    public class Manager
    {
        ConcurrentDictionary<UInt64, User> Users = new ConcurrentDictionary<UInt64, User>();

        public static RedisConnection RedisConn;


        public static void Init(string redisConnectionString)
        {
            var config = new RedisConfig("basic", redisConnectionString);
            RedisConn = new RedisConnection(config);
        }

        public User AddUser(UInt64 uid)
        {
            var user = new User();

            if (Users.TryAdd(uid, user) == false)
            {
                return null;
            }

            return user;
        }

        public void RemoveUser(UInt64 uid)
        {
            Users.TryRemove(uid, out var temp);
        }


        public User GetUser(UInt64 uid)
        {
            if(Users.TryGetValue(uid, out var user))
            {
                return user;
            }

            return null;
        }

        // 유저의 모든 캐시 데이터를 다 저장한다.
        public void SaveDBUserData(MySqlConnection sqlDB, UInt64 uid)
        {
            var user = GetUser(uid);
            if(user == null)
            {
                return;
            }

            user.SaveDB(sqlDB);
        }

        
    }

    // DB 동작 정책
    public enum DBPolicy
    {
        PASS = 0,   // DB 관련 처리를 하지 않는다.
        UPDATE = 1, // DB 업데이트 처리를 한다.
    }
}
