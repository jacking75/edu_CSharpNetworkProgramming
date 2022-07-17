using System;
using System.Collections.Generic;
using System.Text;

namespace FreeNet
{
    public interface IMessageResolver
    {
        void Init(int bufferSize);

        void OnReceive(byte[] buffer, int offset, int transffered, Action<Packet> callback);             
    }
}
