using System.IO;
using System.Text;

namespace Sample.CustomPacketReaderWriter.Protocol
{
    /// <summary>
    /// Provides a custom packet reader implementation.
    /// </summary>
    /// <remarks>
    /// This is a really basic example which allows to read a input buffer.
    /// </remarks>
    public class CustomPacketReader : MemoryStream
    {
        private readonly BinaryReader _reader;

        public CustomPacketReader(byte[] packetBuffer)
            : base(packetBuffer)
        {
            _reader = new BinaryReader(this);
        }

        public int ReadInt32() => _reader.ReadInt32();

        public uint ReadUInt32() => _reader.ReadUInt32();

        public short ReadInt16() => _reader.ReadInt16();

        public ushort ReadUInt16() => _reader.ReadUInt16();

        public float ReadSingle() => _reader.ReadSingle();

        public double ReadDouble() => _reader.ReadDouble();

        public string ReadString()
        {
            int stringLength = _reader.ReadInt32();

            if (stringLength > 0)
            {
                byte[] content = _reader.ReadBytes(stringLength);
                
                return Encoding.UTF8.GetString(content);
            }

            return string.Empty;
        }
    }
}