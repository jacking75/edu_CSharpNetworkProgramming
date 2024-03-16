using laster40Net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace laster40Net_SimpleChatServer
{
    public class ChatClient
    {
        public long ID { get; set; }
        public string Account { get; set; }

        private MemoryStream _stream = new MemoryStream();
        private Object _sycStream = new Object();
       // private ProtoBuf.Meta.TypeModel _model = ProtoBuf.Meta.RuntimeTypeModel.Default;
        private TcpService Service { get; set; }
        public ChatClient(TcpService service)
        {
            this.Service = service;
        }

        public void SendObject(int id, Object obj)
        {
            lock (_sycStream)
            {
                _stream.Seek(0, SeekOrigin.Begin);

                //ProtoBuf.Serializer.SerializeWithLengthPrefix<int>(_stream, (int)id, ProtoBuf.PrefixStyle.Fixed32);
                //_model.Serialize(_stream, obj);
                //Service.SendToSession(ID, _stream.GetBuffer(), 0, (int)_stream.Position, false);
            }
        }
    }

    public class ChatServer
    {
        private ConcurrentDictionary<long, ChatClient> _clients = new ConcurrentDictionary<long, ChatClient>();
        //private ProtoBuf.Meta.TypeModel _model = ProtoBuf.Meta.RuntimeTypeModel.Default;
        private TcpService Service { get; set; }
        public ChatServer(TcpService service)
        {
            this.Service = service;
        }
        void SendMessage(long sessionId, int id, Object obj)
        {
            ChatClient client = null;
            _clients.TryGetValue(sessionId, out client);

            if (client != null)
            {
                client.SendObject(id, obj);
            }
        }

        public void HandleNewClient(long session)
        {
            ChatClient client = new ChatClient(Service);
            client.ID = session;
            _clients.TryAdd(session, client);
        }

        public void HandleRemoveClient(long session)
        {
            ChatClient client = null;
            _clients.TryRemove(session, out client);
        }

        public void HandleMessage(long sessionId, byte[] buffer, int offset, int length)
        {
            using (MemoryStream stream = new MemoryStream(buffer, offset, length))
            {
                try
                {
                    //SimpleChat.MESSAGE_ID id = (SimpleChat.MESSAGE_ID)ProtoBuf.Serializer.DeserializeWithLengthPrefix<int>(stream, ProtoBuf.PrefixStyle.Fixed32);
                    //switch (id)
                    //{
                    //    case SimpleChat.MESSAGE_ID.CMSG_HELLO:
                    //        HandleMessage(sessionId, (SimpleChat.CMsgHello)_model.Deserialize(stream, null, typeof(SimpleChat.CMsgHello)));
                    //        break;
                    //    case SimpleChat.MESSAGE_ID.CMSG_CHAT:
                    //        HandleMessage(sessionId, (SimpleChat.CMsgChat)_model.Deserialize(stream, null, typeof(SimpleChat.CMsgChat)));
                    //        break;
                    //    case SimpleChat.MESSAGE_ID.CMSG_BYE:
                    //        HandleMessage(sessionId, (SimpleChat.CMsgBye)_model.Deserialize(stream, null, typeof(SimpleChat.CMsgBye)));
                    //        break;
                    //}
                }
                catch (Exception)
                {
                }
            }
        }

        //private void HandleMessage(long sessionId, SimpleChat.CMsgHello req)
        //{
        //    ChatClient client = null;
        //    _clients.TryGetValue(sessionId, out client);
        //    SimpleChat.SMsgHello ack = new SimpleChat.SMsgHello();
        //    if (client != null)
        //    {
        //        client.Account = req.account;
        //        ack.returnValue = SimpleChat.SMsgHello.RET.OK;
        //    }
        //    else
        //    {
        //        ack.returnValue = SimpleChat.SMsgHello.RET.FAILED;
        //    }

        //    SendMessage(sessionId, SimpleChat.MESSAGE_ID.SMSG_HELLO, ack);
        //}

        //private void HandleMessage(long sessionId, SimpleChat.CMsgChat req)
        //{
        //    ChatClient client = null;
        //    _clients.TryGetValue(sessionId, out client);
        //    SimpleChat.SMsgChat ack = new SimpleChat.SMsgChat();

        //    if (client != null)
        //    {
        //        ack.sender = client.Account;
        //        ack.chatMsg = req.chatMsg;

        //        Parallel.ForEach(_clients, (KeyValuePair<long, ChatClient> s) => s.Value.SendObject(SimpleChat.MESSAGE_ID.SMSG_CHAT, ack));
        //    }
        //}

        //private void HandleMessage(long sessionId, SimpleChat.CMsgBye req)
        //{
        //    SimpleChat.SMsgBye ack = new SimpleChat.SMsgBye();
        //    SendMessage(sessionId, SimpleChat.MESSAGE_ID.SMSG_BYE, ack);
        //}


    } // end Class
}
