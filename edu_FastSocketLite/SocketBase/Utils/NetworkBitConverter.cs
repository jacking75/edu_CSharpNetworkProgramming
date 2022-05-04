using System;
using System.Net;

namespace FastSocketLite.SocketBase.Utils
{
    /// <summary>
    /// network bit converter.
    /// </summary>
    static public class NetworkBitConverter
    {
        /// <summary>
        /// 지정된 16 비트 부호있는 정수 값을 네트워크 바이트 배열로 반환.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        static public byte[] GetBytes(short value)
        {
            return BitConverter.GetBytes(IPAddress.HostToNetworkOrder(value));
        }

        /// <summary>
        /// 지정된 32 비트 부호있는 정수 값을 네트워크 바이트 배열로 반환.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        static public byte[] GetBytes(int value)
        {
            return BitConverter.GetBytes(IPAddress.HostToNetworkOrder(value));
        }

        /// <summary>
        /// 지정된 64 비트 부호있는 정수 값을 네트워크 바이트 배열로 반환.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        static public byte[] GetBytes(long value)
        {
            return BitConverter.GetBytes(IPAddress.HostToNetworkOrder(value));
        }

        /// <summary>
        /// 네트워크 바이트 배열의 지정된 위치에서 2 바이트에서 변환 된 16 비트 부호있는 정수를 반환.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        static public short ToInt16(byte[] value, int startIndex)
        {
            return IPAddress.NetworkToHostOrder(BitConverter.ToInt16(value, startIndex));
        }

        /// <summary>
        /// 네트워크 바이트 배열의 지정된 위치에서 2 바이트에서 변환 된 32 비트 부호있는 정수를 반환.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        static public int ToInt32(byte[] value, int startIndex)
        {
            return IPAddress.NetworkToHostOrder(BitConverter.ToInt32(value, startIndex));
        }

        /// <summary>
        /// 네트워크 바이트 배열의 지정된 위치에서 2 바이트에서 변환 된 64 비트 부호있는 정수를 반환.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        static public long ToInt64(byte[] value, int startIndex)
        {
            return IPAddress.NetworkToHostOrder(BitConverter.ToInt64(value, startIndex));
        }
    }
}