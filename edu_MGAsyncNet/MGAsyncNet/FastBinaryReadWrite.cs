﻿using System;
using System.Collections.Generic;
using System.Text;

namespace MGAsyncNet
{
    public class FastBinaryReadWrite
    {
        //public static void EnsureCapacity(ref byte[] bytes, int offset, int appendLength)
        //{
        //    var newLength = offset + appendLength;

        //    // If null(most case fisrt time) fill byte.
        //    if (bytes == null)
        //    {
        //        bytes = new byte[newLength];
        //        return;
        //    }

        //    // like MemoryStream.EnsureCapacity
        //    var current = bytes.Length;
        //    if (newLength > current)
        //    {
        //        int num = newLength;
        //        if (num < 256)
        //        {
        //            num = 256;
        //            FastResize(ref bytes, num);
        //            return;
        //        }
        //        if (num < current * 2)
        //        {
        //            num = current * 2;
        //        }

        //        FastResize(ref bytes, num);
        //    }
        //}

        //public static void FastResize(ref byte[] array, int newSize)
        //{
        //    if (newSize < 0) throw new ArgumentOutOfRangeException("newSize");

        //    byte[] array2 = array;
        //    if (array2 == null)
        //    {
        //        array = new byte[newSize];
        //        return;
        //    }

        //    if (array2.Length != newSize)
        //    {
        //        byte[] array3 = new byte[newSize];
        //        Buffer.BlockCopy(array2, 0, array3, 0, (array2.Length > newSize) ? newSize : array2.Length);
        //        array = array3;
        //    }
        //}
               
        public static void WriteBoolean(ref byte[] bytes, int offset, bool value)
        {
            bytes[offset] = (byte)(value ? 1 : 0);
        }
               
        public static void WriteBooleanTrueUnsafe(ref byte[] bytes, int offset)
        {
            bytes[offset] = (byte)(1);
        }

        public static void WriteBooleanFalseUnsafe(ref byte[] bytes, int offset)
        {
            bytes[offset] = (byte)(0);
        }

        public static bool ReadBoolean(ref byte[] bytes, int offset)
        {
            return (bytes[offset] == 0) ? false : true;
        }

        public static void WriteByte(ref byte[] bytes, int offset, byte value)
        {
            bytes[offset] = value;
        }

        public static byte ReadByte(ref byte[] bytes, int offset)
        {
            return bytes[offset];
        }

        public static int WriteBytes(ref byte[] bytes, int offset, byte[] value)
        {
            Buffer.BlockCopy(value, 0, bytes, offset, value.Length);
            return value.Length;
        }

        public static byte[] ReadBytes(ref byte[] bytes, int offset, int count)
        {
            var dest = new byte[count];
            Buffer.BlockCopy(bytes, offset, dest, 0, count);
            return dest;
        }

        public static int WriteSByte(ref byte[] bytes, int offset, sbyte value)
        {
            bytes[offset] = (byte)value;
            return 1;
        }

        public static sbyte ReadSByte(ref byte[] bytes, int offset)
        {
            return (sbyte)bytes[offset];
        }

        public static unsafe int WriteSingle(ref byte[] bytes, int offset, float value)
        {
            if (offset % 4 == 0)
            {
                fixed (byte* ptr = bytes)
                {
                    *(float*)(ptr + offset) = value;
                }
            }
            else
            {
                uint num = *(uint*)(&value);
                bytes[offset] = (byte)num;
                bytes[offset + 1] = (byte)(num >> 8);
                bytes[offset + 2] = (byte)(num >> 16);
                bytes[offset + 3] = (byte)(num >> 24);
            }

            return 4;
        }

        public static unsafe float ReadSingle(ref byte[] bytes, int offset)
        {
            if (offset % 4 == 0)
            {
                fixed (byte* ptr = bytes)
                {
                    return *(float*)(ptr + offset);
                }
            }
            else
            {
                uint num = (uint)((int)bytes[offset] | (int)bytes[offset + 1] << 8 | (int)bytes[offset + 2] << 16 | (int)bytes[offset + 3] << 24);
                return *(float*)(&num);
            }
        }

        public static unsafe int WriteDouble(ref byte[] bytes, int offset, double value)
        {
            if (offset % 8 == 0)
            {
                fixed (byte* ptr = bytes)
                {
                    *(double*)(ptr + offset) = value;
                }
            }
            else
            {
                ulong num = (ulong)(*(long*)(&value));
                bytes[offset] = (byte)num;
                bytes[offset + 1] = (byte)(num >> 8);
                bytes[offset + 2] = (byte)(num >> 16);
                bytes[offset + 3] = (byte)(num >> 24);
                bytes[offset + 4] = (byte)(num >> 32);
                bytes[offset + 5] = (byte)(num >> 40);
                bytes[offset + 6] = (byte)(num >> 48);
                bytes[offset + 7] = (byte)(num >> 56);
            }

            return 8;
        }

        public static unsafe double ReadDouble(ref byte[] bytes, int offset)
        {
            if (offset % 8 == 0)
            {
                fixed (byte* ptr = bytes)
                {
                    return *(double*)(ptr + offset);
                }
            }
            else
            {
                uint num = (uint)((int)bytes[offset] | (int)bytes[offset + 1] << 8 | (int)bytes[offset + 2] << 16 | (int)bytes[offset + 3] << 24);
                ulong num2 = (ulong)((int)bytes[offset + 4] | (int)bytes[offset + 5] << 8 | (int)bytes[offset + 6] << 16 | (int)bytes[offset + 7] << 24) << 32 | (ulong)num;
                return *(double*)(&num2);
            }
        }

        public static unsafe int WriteInt16(ref byte[] bytes, int offset, short value)
        {
            fixed (byte* ptr = bytes)
            {
                *(short*)(ptr + offset) = value;
            }

            return 2;
        }

        public static unsafe short ReadInt16(ref byte[] bytes, int offset)
        {
            fixed (byte* ptr = bytes)
            {
                return *(short*)(ptr + offset);
            }
        }

        public static unsafe int WriteInt32(ref byte[] bytes, int offset, int value)
        {
            fixed (byte* ptr = bytes)
            {
                *(int*)(ptr + offset) = value;
            }

            return 4;
        }

        public static unsafe void WriteInt32Unsafe(ref byte[] bytes, int offset, int value)
        {
            fixed (byte* ptr = bytes)
            {
                *(int*)(ptr + offset) = value;
            }
        }

        public static unsafe int ReadInt32(ref byte[] bytes, int offset)
        {
            fixed (byte* ptr = bytes)
            {
                return *(int*)(ptr + offset);
            }
        }

        public static unsafe int WriteInt64(ref byte[] bytes, int offset, long value)
        {
            fixed (byte* ptr = bytes)
            {
                *(long*)(ptr + offset) = value;
            }

            return 8;
        }

        public static unsafe long ReadInt64(ref byte[] bytes, int offset)
        {
            fixed (byte* ptr = bytes)
            {
                return *(long*)(ptr + offset);
            }
        }

        public static unsafe int WriteUInt16(ref byte[] bytes, int offset, ushort value)
        {
            fixed (byte* ptr = bytes)
            {
                *(ushort*)(ptr + offset) = value;
            }

            return 2;
        }

        public static unsafe ushort ReadUInt16(ref byte[] bytes, int offset)
        {
            fixed (byte* ptr = bytes)
            {
                return *(ushort*)(ptr + offset);
            }
        }

        public static unsafe int WriteUInt32(ref byte[] bytes, int offset, uint value)
        {
            fixed (byte* ptr = bytes)
            {
                *(uint*)(ptr + offset) = value;
            }

            return 4;
        }

        public static unsafe uint ReadUInt32(ref byte[] bytes, int offset)
        {
            fixed (byte* ptr = bytes)
            {
                return *(uint*)(ptr + offset);
            }
        }

        public static unsafe int WriteUInt64(ref byte[] bytes, int offset, ulong value)
        {
            fixed (byte* ptr = bytes)
            {
                *(ulong*)(ptr + offset) = value;
            }

            return 8;
        }

        public static unsafe ulong ReadUInt64(ref byte[] bytes, int offset)
        {
            fixed (byte* ptr = bytes)
            {
                return *(ulong*)(ptr + offset);
            }
        }

        public static int WriteChar(ref byte[] bytes, int offset, char value)
        {
            return WriteUInt16(ref bytes, offset, (ushort)value);
        }

        public static char ReadChar(ref byte[] bytes, int offset)
        {
            return (char)ReadUInt16(ref bytes, offset);
        }

        public static int WriteString(ref byte[] bytes, int offset, string value)
        {
            return StringEncoding.UTF8.GetBytes(value, 0, value.Length, bytes, offset);
        }

        public static string ReadString(ref byte[] bytes, int offset, int count)
        {
            return StringEncoding.UTF8.GetString(bytes, offset, count);
        }

        public static unsafe int WriteDecimal(ref byte[] bytes, int offset, decimal value)
        {
            fixed (byte* ptr = bytes)
            {
                *(Decimal*)(ptr + offset) = value;
            }

            return 16;
        }

        public static unsafe decimal ReadDecimal(ref byte[] bytes, int offset)
        {
            fixed (byte* ptr = bytes)
            {
                return *(Decimal*)(ptr + offset);
            }
        }

        public static unsafe int WriteGuid(ref byte[] bytes, int offset, Guid value)
        {
            fixed (byte* ptr = bytes)
            {
                *(Guid*)(ptr + offset) = value;
            }

            return 16;
        }

        public static unsafe Guid ReadGuid(ref byte[] bytes, int offset)
        {
            fixed (byte* ptr = bytes)
            {
                return *(Guid*)(ptr + offset);
            }
        }

#region Timestamp/Duration
        public static unsafe int WriteTimeSpan(ref byte[] bytes, int offset, TimeSpan timeSpan)
        {
            checked
            {
                long ticks = timeSpan.Ticks;
                long seconds = ticks / TimeSpan.TicksPerSecond;
                int nanos = (int)(ticks % TimeSpan.TicksPerSecond) * Duration.NanosecondsPerTick;

                fixed (byte* ptr = bytes)
                {
                    *(long*)(ptr + offset) = seconds;
                    *(int*)(ptr + offset + 8) = nanos;
                }

                return 12;
            }
        }

        public static unsafe TimeSpan ReadTimeSpan(ref byte[] bytes, int offset)
        {
            checked
            {
                fixed (byte* ptr = bytes)
                {
                    var seconds = *(long*)(ptr + offset);
                    var nanos = *(int*)(ptr + offset + 8);

                    if (!Duration.IsNormalized(seconds, nanos))
                    {
                        throw new InvalidOperationException("Duration was not a valid normalized duration");
                    }
                    long ticks = seconds * TimeSpan.TicksPerSecond + nanos / Duration.NanosecondsPerTick;
                    return TimeSpan.FromTicks(ticks);
                }
            }
        }

        public static unsafe int WriteDateTime(ref byte[] bytes, int offset, DateTime dateTime)
        {
            dateTime = dateTime.ToUniversalTime();

            // Do the arithmetic using DateTime.Ticks, which is always non-negative, making things simpler.
            long secondsSinceBclEpoch = dateTime.Ticks / TimeSpan.TicksPerSecond;
            int nanoseconds = (int)(dateTime.Ticks % TimeSpan.TicksPerSecond) * Duration.NanosecondsPerTick;

            fixed (byte* ptr = bytes)
            {
                *(long*)(ptr + offset) = (secondsSinceBclEpoch - Timestamp.BclSecondsAtUnixEpoch);
                *(int*)(ptr + offset + 8) = nanoseconds;
            }

            return 12;
        }

        public static unsafe DateTime ReadDateTime(ref byte[] bytes, int offset)
        {
            fixed (byte* ptr = bytes)
            {
                var seconds = *(long*)(ptr + offset);
                var nanos = *(int*)(ptr + offset + 8);

                if (!Timestamp.IsNormalized(seconds, nanos))
                {
                    throw new InvalidOperationException(string.Format(@"Timestamp contains invalid values: Seconds={0}; Nanos={1}", seconds, nanos));
                }
                return Timestamp.UnixEpoch.AddSeconds(seconds).AddTicks(nanos / Duration.NanosecondsPerTick);
            }
        }

        internal static class Timestamp
        {
            internal static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            internal const long BclSecondsAtUnixEpoch = 62135596800;
            internal const long UnixSecondsAtBclMaxValue = 253402300799;
            internal const long UnixSecondsAtBclMinValue = -BclSecondsAtUnixEpoch;
            internal const int MaxNanos = Duration.NanosecondsPerSecond - 1;

            internal static bool IsNormalized(long seconds, int nanoseconds)
            {
                return nanoseconds >= 0 &&
                    nanoseconds <= MaxNanos &&
                    seconds >= UnixSecondsAtBclMinValue &&
                    seconds <= UnixSecondsAtBclMaxValue;
            }
        }

        internal static class Duration
        {
            public const int NanosecondsPerSecond = 1000000000;
            public const int NanosecondsPerTick = 100;
            public const long MaxSeconds = 315576000000L;
            public const long MinSeconds = -315576000000L;
            internal const int MaxNanoseconds = NanosecondsPerSecond - 1;
            internal const int MinNanoseconds = -NanosecondsPerSecond + 1;

            internal static bool IsNormalized(long seconds, int nanoseconds)
            {
                // Simple boundaries
                if (seconds < MinSeconds || seconds > MaxSeconds ||
                    nanoseconds < MinNanoseconds || nanoseconds > MaxNanoseconds)
                {
                    return false;
                }
                // We only have a problem is one is strictly negative and the other is
                // strictly positive.
                return Math.Sign(seconds) * Math.Sign(nanoseconds) != -1;
            }
        }

#endregion
    }

    internal static class StringEncoding
    {
        public static Encoding UTF8 = new UTF8Encoding(false);
    }
}
