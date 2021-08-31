using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace laster40Net.Util
{
    /// <summary>
    /// bit convert 의 성능 개선 버젼
    /// ( 직접 포인터로 접근해서 값을 set/get 하도록 구현 )
    /// </summary>
    public class FastBitConverter
    {
        public unsafe static bool ToBoolean(byte[] value, int startIndex)
        {
            fixed (byte* pointer = &(value[startIndex]))
            {
                return *(((bool*)pointer));
            }
        }
        public unsafe static char ToChar(byte[] value, int startIndex)
        {
            fixed (byte* pointer = &(value[startIndex]))
            {
                return *(((char*)pointer));
            }
        }
        public unsafe static Double ToDouble(byte[] value, int startIndex)
        {
            fixed (byte* pointer = &(value[startIndex]))
            {
                return *(((double*)pointer));
            }
        }
        public unsafe static short ToInt16(byte[] value, int startIndex)
        {
            fixed (byte* pointer = &(value[startIndex]))
            {
                return *(((short*)pointer));
            }
        }
        public unsafe static Int32 ToInt32(byte[] value, int startIndex)
        {
            fixed (byte* pointer = &(value[startIndex]))
            {
                return *(((int*)pointer));
            }
        }
        public unsafe static Int64 ToInt64(byte[] value, int startIndex)
        {
            fixed (byte* pointer = &(value[startIndex]))
            {
                return *(((long*)pointer));
            }
        }
        public unsafe static Single ToSingle(byte[] value, int startIndex)
        {
            fixed (byte* pointer = &(value[startIndex]))
            {
                return *(((float*)pointer));
            }
        }
        public unsafe static UInt16 ToUInt16(byte[] value, int startIndex)
        {
            fixed (byte* pointer = &(value[startIndex]))
            {
                return *(((ushort*)pointer));
            }
        }
        public unsafe static UInt32 ToUInt32(byte[] value, int startIndex)
        {
            fixed (byte* pointer = &(value[startIndex]))
            {
                return *(((UInt32*)pointer));
            }
        }
        public unsafe static UInt64 ToUInt64(byte[] value, int startIndex)
        {
            fixed (byte* pointer = &(value[startIndex]))
            {
                return *(((ulong*)pointer));
            }
        }
        public unsafe static void GetBytes(bool value, byte[] buffer, int startIndex)
        {
            fixed (byte* pointer = buffer)
            {
                *((bool*)(pointer + startIndex)) = value;
            }
        }
        public unsafe static void GetBytes(char value, byte[] buffer, int startIndex)
        {
            fixed (byte* pointer = buffer)
            {
                *((char*)(pointer + startIndex)) = value;
            }
        }

        public unsafe static void GetBytes(double value, byte[] buffer, int startIndex)
        {
            fixed (byte* pointer = buffer)
            {
                *((double*)(pointer + startIndex)) = value;
            }
        }

        public unsafe static void GetBytes(float value, byte[] buffer, int startIndex)
        {
            fixed (byte* pointer = buffer)
            {
                *((float*)(pointer + startIndex)) = value;
            }
        }
        public unsafe static void GetBytes(int value, byte[] buffer, int startIndex)
        {
            fixed (byte* pointer = buffer)
            {
                *((int*)(pointer + startIndex)) = value;
            }
        }
        public unsafe static void GetBytes(long value, byte[] buffer, int startIndex)
        {
            fixed (byte* pointer = buffer)
            {
                *((long*)(pointer + startIndex)) = value;
            }
        }
        public unsafe static void GetBytes(short value, byte[] buffer, int startIndex)
        {
            fixed (byte* pointer = buffer)
            {
                *((short*)(pointer + startIndex)) = value;
            }
        }
        public unsafe static void GetBytes(uint value, byte[] buffer, int startIndex)
        {
            fixed (byte* pointer = buffer)
            {
                *((uint*)(pointer + startIndex)) = value;
            }
        }
        public unsafe static void GetBytes(ulong value, byte[] buffer, int startIndex)
        {
            fixed (byte* pointer = buffer)
            {
                *((ulong*)(pointer + startIndex)) = value;
            }
        }
        public unsafe static void GetBytes(ushort value, byte[] buffer, int startIndex)
        {
            fixed (byte* pointer = buffer)
            {
                *((ushort*)(pointer + startIndex)) = value;
            }
        }

    }

}
