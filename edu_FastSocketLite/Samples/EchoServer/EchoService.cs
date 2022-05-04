using System;
using System.Collections.Generic;
using System.Text;
using FastSocketLite.Server;
using FastSocketLite.Server.Messaging;
using FastSocketLite.SocketBase;

namespace EchoServer
{
    public class EchoService : AbsSocketService<CommandLineMessage>
    {
        public override void OnConnected(IConnection connection)
        {
            base.OnConnected(connection);
            Console.WriteLine(connection.RemoteEndPoint.ToString() + " " + connection.ConnectionID.ToString() + " connected");
            connection.BeginReceive();
        }

        public override void OnReceived(IConnection connection, CommandLineMessage message)
        {
            base.OnReceived(connection, message);
            switch (message.CmdName)
            {
                case "echo":
                    message.Reply(connection, "echo_reply " + message.Parameters[0]);
                    break;
                case "init":
                    Console.WriteLine("connection:" + connection.ConnectionID.ToString() + " init");
                    message.Reply(connection, "init_reply ok");
                    break;
                default:
                    message.Reply(connection, "error unknow command ");
                    break;
            }
        }

        public override void OnDisconnected(IConnection connection, Exception ex)
        {
            base.OnDisconnected(connection, ex);
            Console.WriteLine(connection.RemoteEndPoint.ToString() + " disconnected");
        }

        public override void OnException(IConnection connection, Exception ex)
        {
            base.OnException(connection, ex);
            Console.WriteLine(ex.ToString());
        }
    }
}
