using MySqlConnector;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace DBServer.Cache
{
    public class Manager
    {
        ConcurrentDictionary<UInt64, User> Users = new ConcurrentDictionary<UInt64, User>();

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

    public enum DBPolicy
    {
        PASS = 0,
        UPDATE = 1,
    }
}
