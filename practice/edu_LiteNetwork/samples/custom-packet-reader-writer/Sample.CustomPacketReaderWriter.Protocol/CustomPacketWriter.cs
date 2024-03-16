using System.IO;
using System.Text;

namespace Sample.CustomPacketReaderWriter.Protocol
{
    /// <summary>
    /// Provides a custom packet writer implementation.
    /// </summary>
    /// <remarks>
    /// This is a really basic example which allows to write data into a stream.
    /// </remarks>
    public class CustomPacketWriter : MemoryStream
    {
        private readonly BinaryWriter _writer;

        public CustomPacketWriter()
        {
            _writer = new BinaryWriter(this);
        }

        //
        // Here you can specify the different methods to write your values.
        // You can even write custom methods to add objects and serialize them as JSON for example.
        // It's up to you and your imagination.
        //

        public void WriteInt32(int value) => _writer.Write(value);

        public void WriteUInt32(uint value) => _writer.Write(value);

        public void WriteInt16(short value) => _writer.Write(value);

        public void WriteUInt16(ushort value) => _writer.Write(value);

        public void WriteSingle(float value) => _writer.Write(value);

        public void WriteDouble(double value) => _writer.Write(value);

        public void WriteString(string value)
        {
            _writer.Write(value?.Length ?? 0);
            
            if (!string.IsNullOrEmpty(value))
            {
                _writer.Write(Encoding.UTF8.GetBytes(value));
            }
        }
    }
}