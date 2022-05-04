using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace FastSocketLite.Server
{
    /// <summary>
    /// Socket server manager.
    /// </summary>
    public class SocketServerManager
    {
        /// <summary>
        /// key:server name.
        /// </summary>
        static private readonly Dictionary<string, SocketBase.IHost> _dicHosts = new Dictionary<string, SocketBase.IHost>();
        

        /// <summary>
        /// 初始化Socket Server
        /// </summary>
        static public void Init()
        {
            Init("socketServer");
        }
        /// <summary>
        /// Socket Server 초기화
        /// </summary>
        /// <param name="sectionName"></param>
        static public void Init(string sectionName)
        {
            if (string.IsNullOrEmpty(sectionName))
            {
                throw new ArgumentNullException("sectionName");
            }

            Init(ConfigurationManager.GetSection(sectionName) as Config.SocketServerConfig);
        }
        
        static void Init(Config.SocketServerConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            if (config.Servers == null)
            {
                return;
            }

            foreach (Config.Server serverConfig in config.Servers)
            {
                //inti protocol
                var objProtocol = GetProtocol(serverConfig.Protocol);
                if (objProtocol == null)
                {
                    throw new InvalidOperationException("protocol");
                }

                //init custom service
                var tService = Type.GetType(serverConfig.ServiceType, false);
                if (tService == null)
                {
                    throw new InvalidOperationException("serviceType");
                }

                var objService = Activator.CreateInstance(tService);
                if (objService == null)
                {
                    throw new InvalidOperationException("serviceType");
                }

                //init host.
                _dicHosts.Add(serverConfig.Name, Activator.CreateInstance(
                    typeof(SocketServer<>).MakeGenericType(
                    objProtocol.GetType().GetInterface(typeof(Protocol.IProtocol<>).Name).GetGenericArguments()),
                        serverConfig.Port,
                        objService,
                        objProtocol,
                        serverConfig.SocketBufferSize,
                        serverConfig.MessageBufferSize,
                        serverConfig.MaxMessageSize,
                        serverConfig.MaxConnections) as SocketBase.IHost);
            }
        }
        
        static public object GetProtocol(string protocol)
        {
            switch (protocol)
            {
                case Protocol.ProtocolNames.Thrift: return new Protocol.ThriftProtocol();
                case Protocol.ProtocolNames.CommandLine: return new Protocol.CommandLineProtocol();
            }
            return Activator.CreateInstance(Type.GetType(protocol, false));
        }

        
        static public void Start()
        {
            _dicHosts.ToList().ForEach(c => c.Value.Start());
        }
        
        static public void Stop()
        {
            _dicHosts.ToList().ForEach(c => c.Value.Stop());
        }
        
        static public bool TryGetHost(string name, out SocketBase.IHost host)
        {
            return _dicHosts.TryGetValue(name, out host);
        }
        
    }
}