using System;

namespace MGAsyncNet
{
    public class Packet
    {
        const int BUFFER_SIZE = 10240;

        byte[] Buffer;
        int BufferPos = 0;

       public byte[] GetBuffer() { return Buffer; }
        
        public Packet(int bufferSize = BUFFER_SIZE)
        {
            BufferPos = 0;
            //PacketLen = 0;
            Buffer = new byte[bufferSize];
        }

        //public Packet(int type)
        //{
        //    BufferPos = 0;
        //    //PacketLen = 0;
        //    Buffer = new byte[BUFFER_SIZE];
        //    WriteInt32((int)type);
        //}

        public Packet(Packet p)
        {
            BufferPos = 0;
            //PacketLen = p.PacketLen;
            Buffer = new Byte[BUFFER_SIZE];

            Array.Copy(p.Buffer, Buffer, BUFFER_SIZE);
        }

        public void Copy(Packet p)
        {
            BufferPos = 0;
            //PacketLen = p.PacketLen;
            Buffer = new Byte[BUFFER_SIZE];

            Array.Copy(p.Buffer, Buffer, BUFFER_SIZE);
        }

        public Packet(byte[] buffer)
        {
            Buffer = buffer;
            BufferPos = 0;
        }

        public Packet(byte[] buffer, int startpos, int len)
        {
            Buffer = new byte[len];
            Array.Copy(buffer, startpos, Buffer, 0, len);
            BufferPos = len;
            //PacketLen = len;
        }

        public void Reset()
        {
            BufferPos = 0;
            //PacketLen = 0;
        }

        public int Position
        {
            get { return BufferPos; }
            set { BufferPos = value; }
        }

        //public void SetInt(int data, int offset) { BitConverter.GetBytes(data).CopyTo(Buffer, offset); }

        public void WriteDateTime(DateTime data) 
        {
            var len = FastBinaryReadWrite.WriteDateTime(ref Buffer, BufferPos, data);
            BufferPos += len;
            
            //long d = data.ToBinary();
            //Write(BitConverter.GetBytes(d)); 
        }

        public void WriteUInt16(ushort data)
        {
            var len = FastBinaryReadWrite.WriteUInt16(ref Buffer, BufferPos, data);
            BufferPos += len;
            //Write(BitConverter.GetBytes(data));
        }

        public void WriteUInt32(uint data)
        {
            var len = FastBinaryReadWrite.WriteUInt32(ref Buffer, BufferPos, data);
            BufferPos += len;
            //Write(BitConverter.GetBytes(data));
        }

        public void WriteUInt64(UInt64 data)
        {
            var len = FastBinaryReadWrite.WriteUInt64(ref Buffer, BufferPos, data);
            BufferPos += len;
            //Write(BitConverter.GetBytes(data));
        }               

        public void WriteDouble(double data)
        {
            var len = FastBinaryReadWrite.WriteDouble(ref Buffer, BufferPos, data);
            BufferPos += len;
            //Write(BitConverter.GetBytes(data));
        }

        public void WriteFloat(float data)
        {
            var len = FastBinaryReadWrite.WriteSingle(ref Buffer, BufferPos, data);
            BufferPos += len;
            //Write(BitConverter.GetBytes(data));
        }

        public void WriteByte(byte data)
        {
            FastBinaryReadWrite.WriteByte(ref Buffer, BufferPos, data);
            ++BufferPos;
            //Write(BitConverter.GetBytes(data));
        }

        public void WriteBool(bool data)
        {
            FastBinaryReadWrite.WriteBoolean(ref Buffer, BufferPos, data);
            ++BufferPos;
            //Write(BitConverter.GetBytes(data));
        }

        public void WriteInt16(short data)
        {
            var len = FastBinaryReadWrite.WriteInt16(ref Buffer, BufferPos, data);
            BufferPos += len;
            //Write(BitConverter.GetBytes(data));
        }

        public void WriteInt64(long data)
        {
            var len = FastBinaryReadWrite.WriteInt64(ref Buffer, BufferPos, data);
            BufferPos += len;
            //Write(BitConverter.GetBytes(data));
        }

        public void WriteInt32(int data)
        {
            var len = FastBinaryReadWrite.WriteInt32(ref Buffer, BufferPos, data);
            BufferPos += len;
            //Write(BitConverter.GetBytes(data));
        }

        public void WriteString(string data)
        {
            byte[] ConvData = System.Text.Encoding.Unicode.GetBytes(data);
            WriteInt32(ConvData.Length);
            Write(ConvData);
        }

        private void Write(byte[] data)
        {
            data.CopyTo(Buffer, BufferPos);
            BufferPos += data.Length;
            //PacketLen = BufferPos;
        }

        public void WriteBytes(byte[] buffer, int len)
        {
            Array.Copy(buffer, 0, Buffer, BufferPos, len);
            BufferPos += len;
            //PacketLen = BufferPos;
        }

        public uint ReadUInt32()
        {
            var read = FastBinaryReadWrite.ReadUInt32(ref Buffer, BufferPos); //BitConverter.ToUInt32(Buffer, BufferPos);
            BufferPos += sizeof(uint);
            return read;
        }

        public ushort ReadUInt16()
        {
            var read = FastBinaryReadWrite.ReadUInt16(ref Buffer, BufferPos); //BitConverter.ToUInt16(Buffer, BufferPos);
            BufferPos += sizeof(ushort);
            return read;
        }

        public double ReadDouble()
        {
            var read = FastBinaryReadWrite.ReadDouble(ref Buffer, BufferPos); //BitConverter.ToDouble(Buffer, BufferPos);
            BufferPos += sizeof(double);
            return read;
        }

        public float ReadFloat()
        {
            var read = FastBinaryReadWrite.ReadSingle(ref Buffer, BufferPos); //BitConverter.ToSingle(Buffer, BufferPos);
            BufferPos += sizeof(float);
            return read;
        }

        public byte ReadByte()
        {
            var read = FastBinaryReadWrite.ReadByte(ref Buffer, BufferPos);//BitConverter.ToChar(Buffer, BufferPos);
            BufferPos += sizeof(char);
            return read;
        }

        public bool ReadBool()
        {
            bool read = FastBinaryReadWrite.ReadBoolean(ref Buffer, BufferPos);//BitConverter.ToBoolean(Buffer, BufferPos);
            BufferPos += sizeof(bool);
            return read;
        }

        public short ReadInt16()
        {
            short read = FastBinaryReadWrite.ReadInt16(ref Buffer, BufferPos); //BitConverter.ToInt16(Buffer, BufferPos);
            BufferPos += sizeof(short);
            return read;
        }

        public long ReadInt64()
        {
            long read = FastBinaryReadWrite.ReadInt64(ref Buffer, BufferPos);//BitConverter.ToInt64(Buffer, BufferPos);
            BufferPos += sizeof(long);
            return read;
        }

        public int ReadInt32()
        {
            int read = FastBinaryReadWrite.ReadInt32(ref Buffer, BufferPos);//  BitConverter.ToInt32(Buffer, BufferPos);
            BufferPos += sizeof(int);
            return read;
        }

        public string ReadString()
        {
            int Count = ReadInt32();

            string s = System.Text.Encoding.Unicode.GetString(Buffer, BufferPos, Count);
            BufferPos += Count;
            return s;
        }

        public string ReadString(int Count)
        {
            string s = System.Text.Encoding.Unicode.GetString(Buffer, BufferPos, Count);
            BufferPos += Count;
            string ts = s.TrimEnd('\0');
            return ts;
        }

        public void AddPacket(Packet p)
        {
            Array.Copy(p.Buffer, 0, Buffer, BufferPos, p.BufferPos);
            BufferPos += p.BufferPos;
            //PacketLen += p.PacketLen;
        }

    }      

}