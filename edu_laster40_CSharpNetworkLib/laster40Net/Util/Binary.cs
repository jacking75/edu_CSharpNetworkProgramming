using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace laster40Net.Util
{
    internal static class FloatToInt
    {
        [StructLayout(LayoutKind.Explicit)]
        public struct IntFloat
        {
            [FieldOffset(0)]
            public float FloatValue;

            [FieldOffset(0)]
            public int IntValue;
        }

        public static float ToSingle(int value)
        {
            IntFloat uf = new IntFloat();
            uf.IntValue = value;
            return uf.FloatValue;
        }

        public static int ToInt(float value)
        {
            IntFloat uf = new IntFloat();
            uf.FloatValue = value;
            return uf.IntValue;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct IntDouble
        {
            [FieldOffset(0)]
            public double DoubleValue;

            [FieldOffset(0)]
            public long LongValue;
        }

        public static double ToDouble(long value)
        {
            IntDouble uf = new IntDouble();
            uf.LongValue = value;
            return uf.DoubleValue;
        }

        public static long ToLong(double value)
        {
            IntDouble uf = new IntDouble();
            uf.DoubleValue = value;
            return uf.LongValue;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class BinaryWriter
    {
        private byte[] _buffer = null;
        private int _index = 0;
        private int _max = 0;
        private bool _error = false;

        public byte[] Buffer { get { return _buffer; } }
        public int BufferCount { get { return (_index + 7) / 8; } }
        public int Index { get { return _index; } }
        public bool HasError { get { return _error; } }

        public BinaryWriter(int size)
        {
            _buffer = new byte[size];
            _max = _buffer.Length * 8;
        }

        public BinaryWriter(byte[] buffer)
        {
            _buffer = buffer;
            _max = _buffer.Length * 8;
        }

        /// <summary>
        /// 쓰기 - bool타입 
        /// </summary>
        /// <param name="value">값</param>
        public void WriteBool(bool value)
        {
            // 쓸수 있나 확인해 보고
            if (_index + 1 > _max)
            {
                _error = true;
                return;
            }

            if (value)
                _buffer[_index / 8] |= (byte)(1 << (7 - _index % 8));
            ++_index;
        }

        /// <summary>
        /// 쓰기 - byte
        /// </summary>
        /// <param name="value">값</param>
        public void WriteByte(byte value)
        {
            // 쓸수 있나 확인해 보고
            if (_index + 8 > _max)
            {
                _error = true;
                return;
            }

            int offset = _index % 8;
            _buffer[_index / 8] |= (byte)(value >> offset);
            if (offset != 0)
            {
                _buffer[_index / 8 + 1] |= (byte)(value << 8 - offset);
            }
            _index += 8;
        }
        /// <summary>
        /// 쓰기 - sbyte
        /// </summary>
        /// <param name="value">값</param>
        public void WriteSByte(sbyte value)
        {
            // 쓸수 있나 확인해 보고
            if (_index + 8 > _max)
            {
                _error = true;
                return;
            }
            int offset = _index % 8;
            _buffer[_index / 8] |= (byte)(value >> offset);
            if (offset != 0)
            {
                _buffer[_index / 8 + 1] |= (byte)(value << 8 - offset);
            }
            _index += 8;
        }
        /// <summary>
        /// 쓰기 - ushort
        /// </summary>
        /// <param name="value">값</param>
        public void WriteUInt16(ushort value)
        {
            // 쓸수 있나 확인해 보고
            if (_index + 16 > _max)
            {
                _error = true;
                return;
            }
            int offset = _index % 8;
            _buffer[_index / 8] |= (byte)(value >> 8 + offset);
            _buffer[_index / 8 + 1] |= (byte)(value >> offset);
            if (offset != 0)
            {
                _buffer[_index / 8 + 2] |= (byte)(value << 8 - offset);
            }
            _index += 16;
        }
        /// <summary>
        /// 쓰기 - short
        /// </summary>
        /// <param name="value">값</param>
        public void WriteInt16(short value)
        {
            if (_index + 16 > _max)
            {
                _error = true;
                return;
            }
            int offset = _index % 8;
            _buffer[_index / 8] |= (byte)(value >> 8 + offset);
            _buffer[_index / 8 + 1] |= (byte)(value >> offset);
            if (offset != 0)
            {
                _buffer[_index / 8 + 2] |= (byte)(value << 8 - offset);
            }
            _index += 16;
        }
        /// <summary>
        /// 쓰기 - uint
        /// </summary>
        /// <param name="value">값</param>
        public void WriteUInt32(uint value)
        {
            if (_index + 32 > _max)
            {
                _error = true;
                return;
            }
            int offset = _index % 8;
            _buffer[_index / 8] |= (byte)(value >> 24 + offset);
            _buffer[_index / 8 + 1] |= (byte)(value >> 16 + offset);
            _buffer[_index / 8 + 2] |= (byte)(value >> 8 + offset);
            _buffer[_index / 8 + 3] |= (byte)(value >> offset);
            if (offset != 0)
            {
                _buffer[_index / 8 + 4] |= (byte)(value << 8 - offset);
            }
            _index += 32;
        }
        /// <summary>
        /// 쓰기 - int
        /// </summary>
        /// <param name="value">값</param>
        public void WriteInt32(int value)
        {
            if (_index + 32 > _max)
            {
                _error = true;
                return;
            }
            int offset = _index % 8;
            _buffer[_index / 8] |= (byte)(value >> 24 + offset);
            _buffer[_index / 8 + 1] |= (byte)(value >> 16 + offset);
            _buffer[_index / 8 + 2] |= (byte)(value >> 8 + offset);
            _buffer[_index / 8 + 3] |= (byte)(value >> offset);
            if (offset != 0)
            {
                _buffer[_index / 8 + 4] |= (byte)(value << 8 - offset);
            }
            _index += 32;
        }
        /// <summary>
        /// 쓰기 - ulong
        /// </summary>
        /// <param name="value">값</param>
        public void WriteUInt64(ulong value)
        {
            if (_index + 64 > _max)
            {
                _error = true;
                return;
            }
            int offset = _index % 8;
            _buffer[_index / 8] |= (byte)(value >> 56 + offset);
            _buffer[_index / 8 + 1] |= (byte)(value >> 48 + offset);
            _buffer[_index / 8 + 2] |= (byte)(value >> 40 + offset);
            _buffer[_index / 8 + 3] |= (byte)(value >> 32 + offset);
            _buffer[_index / 8 + 4] |= (byte)(value >> 24 + offset);
            _buffer[_index / 8 + 5] |= (byte)(value >> 16 + offset);
            _buffer[_index / 8 + 6] |= (byte)(value >> 8 + offset);
            _buffer[_index / 8 + 7] |= (byte)(value >> offset);
            if (offset != 0)
            {
                _buffer[_index / 8 + 8] |= (byte)(value << 8 - offset);
            }
            _index += 64;
        }
        /// <summary>
        /// 쓰기 - long
        /// </summary>
        /// <param name="value">값</param>
        public void WriteInt64(long value)
        {
            if (_index + 64 > _max)
            {
                _error = true;
                return;
            }
            int offset = _index % 8;
            _buffer[_index / 8] |= (byte)(value >> 56 + offset);
            _buffer[_index / 8 + 1] |= (byte)(value >> 48 + offset);
            _buffer[_index / 8 + 2] |= (byte)(value >> 40 + offset);
            _buffer[_index / 8 + 3] |= (byte)(value >> 32 + offset);
            _buffer[_index / 8 + 4] |= (byte)(value >> 24 + offset);
            _buffer[_index / 8 + 5] |= (byte)(value >> 16 + offset);
            _buffer[_index / 8 + 6] |= (byte)(value >> 8 + offset);
            _buffer[_index / 8 + 7] |= (byte)(value >> offset);
            if (offset != 0)
            {
                _buffer[_index / 8 + 8] |= (byte)(value << 8 - offset);
            }
            _index += 64;
        }

        /// <summary>
        /// 쓰기 - uint 값을 bits 를 사용해서
        /// </summary>
        /// <param name="value">값</param>
        public void WriteUInt(uint value, int bits)
        {
            if (bits < 1 || bits > 32)
            {
                return;
            }
            if (bits != 32 && value > (0x1 << bits) - 1)
            {
                return;
            }

            if (_index + bits > _max)
            {
                _error = true;
                return;
            }

            value <<= 32 - bits;
            int offset = _index % 8;
            _buffer[_index / 8] |= (byte)(value >> 24 + offset);
            if (offset + bits > 8)
            {
                _buffer[_index / 8 + 1] |= (byte)(value >> 16 + offset);
                if (offset + bits > 16)
                {
                    _buffer[_index / 8 + 2] |= (byte)(value >> 8 + offset);
                    if (offset + bits > 24)
                    {
                        _buffer[_index / 8 + 3] |= (byte)(value >> offset);
                        if (offset + bits > 32)
                        {
                            _buffer[_index / 8 + 4] |= (byte)(value << 8 - offset);
                        }
                    }
                }
            }
            _index += bits;
        }
        /// <summary>
        /// 쓰기 - int 값을 bits 를 사용해서
        /// </summary>
        /// <param name="value">값</param>
        public void WriteInt(int value, int bits)
        {
            if (bits < 1 || bits > 32)
            {
                return;
            }

            if (bits != 32 && value > (0x1 << bits) - 1)
            {
                return;
            }

            if (_index + bits > _max)
            {
                _error = true;
                return;
            }
            value <<= 32 - bits;
            uint uvalue = (uint)value;
            int offset = _index % 8;
            _buffer[_index / 8] |= (byte)(uvalue >> 24 + offset);
            if (offset + bits > 8)
            {
                _buffer[_index / 8 + 1] |= (byte)(uvalue >> 16 + offset);
                if (offset + bits > 16)
                {
                    _buffer[_index / 8 + 2] |= (byte)(uvalue >> 8 + offset);
                    if (offset + bits > 24)
                    {
                        _buffer[_index / 8 + 3] |= (byte)(uvalue >> offset);
                        if (offset + bits > 32)
                        {
                            _buffer[_index / 8 + 4] |= (byte)(uvalue << 8 - offset);
                        }
                    }
                }
            }
            _index += bits;
        }


        public void WriteSingle(float value)
        {
            int valueInt = FloatToInt.ToInt(value);
            WriteInt32(valueInt);
        }

        public void WriteDouble(double value)
        {
            long valueLong = FloatToInt.ToLong(value);
            WriteInt64(valueLong);
        }

        public void WriteString(string value)
        {
            byte[] data = System.Text.Encoding.UTF8.GetBytes(value);
            WriteInt32(data.Length);
            foreach (var v in data)
                WriteByte(v);
        }

        public void Write(laster40Net.Util.BinaryWriter writer)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class BinaryReader
    {
        private byte[] _buffer = null;
        private int _index = 0;
        private int _max = 0;
        private bool _error = false;

        public byte[] Buffer { get { return _buffer; } }
        public int BufferCount { get { return (_index + 7) / 8; } }
        public int Index { get { return _index; } }
        public bool HasError { get { return _error; } }

        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="writer"></param>
        public BinaryReader(BinaryWriter writer)
        {
            _buffer = writer.Buffer;
            _max = writer.Index;
        }

        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="buffer"></param>
        public BinaryReader(byte[] buffer)
        {
            _buffer = buffer;
            _max = _buffer.Length * 8;
        }

        /// <summary>
        /// 읽기 - bool
        /// </summary>
        /// <returns>값</returns>
        public bool ReadBool()
        {
            bool value = false;
            if ((_index + 1) > _max)
            {
                _error = true;
                return value;
            }

            value = ((_buffer[_index / 8] >> (7 - _index % 8)) & 0x1) == 1;
            ++_index;

            return value;
        }
        /// <summary>
        /// 읽기 - byte
        /// </summary>
        /// <returns>값</returns>
        public byte ReadByte()
        {
            byte value = 0;
            if ((_index + 8) > _max)
            {
                _error = true;
                return value;
            }

            int offset = _index % 8;
            value |= (byte)(_buffer[_index / 8] << offset);
            if (offset != 0)
            {
                value |= (byte)(_buffer[_index / 8 + 1] >> 8 - offset);
            }
            _index += 8;

            return value;
        }
        /// <summary>
        /// 읽기 - sbyte
        /// </summary>
        /// <returns>값</returns>
        public sbyte ReadSByte()
        {
            sbyte value = 0;
            if ((_index + 8) > _max)
            {
                _error = true;
                return value;
            }
            int offset = _index % 8;
            value |= (sbyte)(_buffer[_index / 8] << offset);
            if (offset != 0)
            {
                value |= (sbyte)(_buffer[_index / 8 + 1] >> 8 - offset);
            }
            _index += 8;

            return value;
        }
        /// <summary>
        /// 읽기 - ushort
        /// </summary>
        /// <returns>값</returns>
        public ushort ReadUInt16()
        {
            ushort value = 0;
            if ((_index + 16) > _max)
            {
                _error = true;
                return value;
            }
            int offset = _index % 8;
            value |= (ushort)(_buffer[_index / 8] << 8 + offset);
            value |= (ushort)(_buffer[_index / 8 + 1] << offset);
            if (offset != 0)
            {
                value |= (ushort)(_buffer[_index / 8 + 2] >> 8 - offset);
            }
            _index += 16;

            return value;
        }
        /// <summary>
        /// 읽기 - short
        /// </summary>
        /// <returns>값</returns>
        public short ReadInt16()
        {
            short value = 0;
            if ((_index + 16) > _max)
            {
                _error = true;
                return value;
            }
            int offset = _index % 8;
            value |= (short)(_buffer[_index / 8] << 8 + offset);
            value |= (short)(_buffer[_index / 8 + 1] << offset);
            if (offset != 0)
            {
                value |= (short)(_buffer[_index / 8 + 2] >> 8 - offset);
            }
            _index += 16;

            return value;
        }
        /// <summary>
        /// 읽기 - uint
        /// </summary>
        /// <returns>값</returns>
        public uint ReadUInt32()
        {
            uint value = 0;
            if (_index + 32 > _max)
            {
                _error = true;
                return value;
            }
            int offset = _index % 8;

            value |= (((uint)_buffer[_index / 8]) << 24 + offset);
            value |= (((uint)_buffer[_index / 8 + 1]) << 16 + offset);
            value |= (((uint)_buffer[_index / 8 + 2]) << 8 + offset);
            value |= (((uint)_buffer[_index / 8 + 3]) << offset);
            if (offset != 0)
            {
                value |= (((uint)_buffer[_index / 8 + 4]) >> 8 - offset);
            }
            _index += 32;
            return value;
        }
        /// <summary>
        /// 읽기 - uint
        /// </summary>
        /// <returns>값</returns>
        public int ReadInt32()
        {
            int value = 0;
            if (_index + 32 > _max)
            {
                _error = true;
                return value;
            }
            int offset = _index % 8;
            value |= (((int)_buffer[_index / 8]) << 24 + offset);
            value |= (((int)_buffer[_index / 8 + 1]) << 16 + offset);
            value |= (((int)_buffer[_index / 8 + 2]) << 8 + offset);
            value |= (((int)_buffer[_index / 8 + 3]) << offset);
            if (offset != 0)
            {
                value |= (((int)_buffer[_index / 8 + 4]) >> 8 - offset);
            }
            _index += 32;
            return value;
        }

        /// <summary>
        /// 읽기 - uint
        /// </summary>
        /// <returns>값</returns>
        public ulong ReadUInt64()
        {
            ulong value = 0;
            if (_index + 64 > _max)
            {
                _error = true;
                return value;
            }
            int offset = _index % 8;
            value |= (((ulong)_buffer[_index / 8]) << 56 + offset);
            value |= (((ulong)_buffer[_index / 8 + 1]) << 48 + offset);
            value |= (((ulong)_buffer[_index / 8 + 2]) << 40 + offset);
            value |= (((ulong)_buffer[_index / 8 + 3]) << 32 + offset);
            value |= (((ulong)_buffer[_index / 8 + 4]) << 24 + offset);
            value |= (((ulong)_buffer[_index / 8 + 5]) << 16 + offset);
            value |= (((ulong)_buffer[_index / 8 + 6]) << 8 + offset);
            value |= (((ulong)_buffer[_index / 8 + 7]) << offset);
            if (offset != 0)
            {
                value |= (((ulong)_buffer[_index / 8 + 8]) >> 8 - offset);
            }
            _index += 64;
            return value;
        }
        /// <summary>
        /// 읽기 - uint
        /// </summary>
        /// <returns>값</returns>
        public long ReadInt64()
        {
            long value = 0;
            if (_index + 64 > _max)
            {
                _error = true;
                return value;
            }
            int offset = _index % 8;
            value |= (((long)_buffer[_index / 8]) << 56 + offset);
            value |= (((long)_buffer[_index / 8 + 1]) << 48 + offset);
            value |= (((long)_buffer[_index / 8 + 2]) << 40 + offset);
            value |= (((long)_buffer[_index / 8 + 3]) << 32 + offset);
            value |= (((long)_buffer[_index / 8 + 4]) << 24 + offset);
            value |= (((long)_buffer[_index / 8 + 5]) << 16 + offset);
            value |= (((long)_buffer[_index / 8 + 6]) << 8 + offset);
            value |= (((long)_buffer[_index / 8 + 7]) << offset);
            if (offset != 0)
            {
                value |= (((long)_buffer[_index / 8 + 8]) >> 8 - offset);
            }
            _index += 64;
            return value;
        }
        /// <summary>
        /// 읽기 - uint
        /// </summary>
        /// <param name="bits">크기</param>
        /// <returns>값</returns>
        public uint ReadUInt(int bits)
        {
            uint value = 0;
            if (bits < 1 || bits > 32)
            {
                _error = true;
                return value;
            }
            if (_index + bits > _max)
            {
                _error = true;
                return value;
            }
            int offset = _index % 8;
            value = (uint)_buffer[_index / 8] << 24 + offset;
            if (offset + bits > 8)
            {
                value |= (uint)_buffer[_index / 8 + 1] << 16 + offset;
                if (offset + bits > 16)
                {
                    value |= (uint)_buffer[_index / 8 + 2] << 8 + offset;
                    if (offset + bits > 24)
                    {
                        value |= (uint)_buffer[_index / 8 + 3] << offset;
                        if (offset + bits > 32)
                        {
                            value |= (uint)_buffer[_index / 8 + 4] >> 8 - offset;
                        }
                    }
                }
            }
            value >>= 32 - bits;
            _index += bits;
            return value;
        }
        /// <summary>
        /// 읽기 - uint
        /// </summary>
        /// <param name="bits">크기</param>
        /// <returns>값</returns>
        public int ReadInt(int bits)
        {
            int value = 0;
            if (bits < 1 || bits > 32)
            {
                _error = true;
                return value;
            }
            if (_index + bits > _max)
            {
                _error = true;
                return value;
            }

            int offset = _index % 8;
            value = _buffer[_index / 8] << 24 + offset;
            if (offset + bits > 8)
            {
                value |= _buffer[_index / 8 + 1] << 16 + offset;
                if (offset + bits > 16)
                {
                    value |= _buffer[_index / 8 + 2] << 8 + offset;
                    if (offset + bits > 24)
                    {
                        value |= _buffer[_index / 8 + 3] << offset;
                        if (offset + bits > 32)
                        {
                            value |= _buffer[_index / 8 + 4] >> 8 - offset;
                        }
                    }
                }
            }
            value >>= 32 - bits;
            _index += bits;
            return value;
        }
        /// <summary>
        /// 읽기 - float
        /// </summary>
        /// <returns>값</returns>
        public float ReadSingle()
        {
            float value = 0;
            int valueInt = ReadInt32();
            if (_error)
                return value;

            return FloatToInt.ToSingle(valueInt);
        }

        /// <summary>
        /// 읽기 - double
        /// </summary>
        /// <returns>값</returns>
        public double ReadDouble()
        {
            double value = 0;
            long valueLong = ReadInt64();
            if (_error)
                return value;

            return FloatToInt.ToDouble(valueLong);
        }

        /// <summary>
        /// 읽기 - double
        /// </summary>
        /// <returns>값</returns>
        public string ReadString()
        {
            int length = ReadInt32();
            byte[] copy = new byte[length];
            for (int i = 0; i < length; ++i)
                copy[i] = ReadByte();
            return System.Text.Encoding.UTF8.GetString(copy);
        }

        public string Trace()
        {
            string s = string.Empty;
            for (int copyBits = 0; copyBits < _buffer.Length * 8; ++copyBits)
            {
                s += ((_buffer[copyBits / 8] >> (7 - copyBits % 8)) & 0x1) == 0 ? "0" : "1";
                if ((copyBits + 1) % 4 == 0 && copyBits != 0)
                {
                    s += " ";
                    if ((copyBits + 1) % 8 == 0)
                    {
                        s += " ";
                    }
                }
            }
            return s;
        }
    } // end Class
}
