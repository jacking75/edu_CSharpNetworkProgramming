using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCommon
{
    public class BinaryUtil
    {
        public static byte[] Clone(byte[] source , int offset, int length)
        {
            var buffer = new byte[length];
            Buffer.BlockCopy(source, offset, buffer, 0, length);
            return buffer;
        }

       
    }
}
