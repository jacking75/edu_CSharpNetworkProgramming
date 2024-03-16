﻿using System.Collections.Generic;
using System.Net;

namespace FastSocketLite.SocketBase.Utils
{
    /// <summary>
    /// ip utility
    /// </summary>
    static public class IPUtility
    {
        #region Private Members
        /// <summary>
        /// A 클래스: 10.0.0.0-10.255.255.255
        /// </summary>
        static private long ipABegin, ipAEnd;
        /// <summary>
        /// B 클래스: 172.16.0.0-172.31.255.255   
        /// </summary>
        static private long ipBBegin, ipBEnd;
        /// <summary>
        /// C 클래스: 192.168.0.0-192.168.255.255
        /// </summary>
        static private long ipCBegin, ipCEnd;
        #endregion

        #region Constructors
        /// <summary>
        /// static new
        /// </summary>
        static IPUtility()
        {
            ipABegin = ConvertToNumber("10.0.0.0");
            ipAEnd = ConvertToNumber("10.255.255.255");

            ipBBegin = ConvertToNumber("172.16.0.0");
            ipBEnd = ConvertToNumber("172.31.255.255");

            ipCBegin = ConvertToNumber("192.168.0.0");
            ipCEnd = ConvertToNumber("192.168.255.255");
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// ipaddress convert to long
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        static public long ConvertToNumber(string ipAddress)
        {
            return ConvertToNumber(IPAddress.Parse(ipAddress));
        }
        /// <summary>
        /// ipaddress convert to long
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        static public long ConvertToNumber(IPAddress ipAddress)
        {
            var bytes = ipAddress.GetAddressBytes();
            return bytes[0] * 256 * 256 * 256 + bytes[1] * 256 * 256 + bytes[2] * 256 + bytes[3];
        }
        /// <summary>
        /// true이면 인트라넷 IP
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        static public bool IsIntranet(string ipAddress)
        {
            return IsIntranet(ConvertToNumber(ipAddress));
        }
        /// <summary>
        /// true이면 인트라넷 IP
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        static public bool IsIntranet(IPAddress ipAddress)
        {
            return IsIntranet(ConvertToNumber(ipAddress));
        }
        /// <summary>
        /// true이면 인트라넷 IP
        /// </summary>
        /// <param name="longIP"></param>
        /// <returns></returns>
        static public bool IsIntranet(long longIP)
        {
            return ((longIP >= ipABegin) && (longIP <= ipAEnd) ||
                    (longIP >= ipBBegin) && (longIP <= ipBEnd) ||
                    (longIP >= ipCBegin) && (longIP <= ipCEnd));
        }
        

        static public IPAddress GetLocalIntranetIP()
        {
            var list = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
            foreach (var child in list)
            {
                if (IsIntranet(child)) return child;
            }

            return null;
        }
        
        static public List<IPAddress> GetLocalIntranetIPList()
        {
            var list = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
            var result = new List<IPAddress>();
            foreach (var child in list)
            {
                if (IsIntranet(child)) result.Add(child);
            }

            return result;
        }
        #endregion
    }
}