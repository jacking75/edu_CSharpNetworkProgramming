using System.Configuration;

namespace FastSocketLite.Server.Config
{
    /// <summary>
    /// server
    /// </summary>
    public class Server : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get { return (string)this["name"]; }
        }

        [ConfigurationProperty("port", IsRequired = true)]
        public int Port
        {
            get { return (int)this["port"]; }
        }

        /// <summary>
        /// Socket Buffer Size.  default 8192 bytes
        /// </summary>
        [ConfigurationProperty("socketBufferSize", IsRequired = false, DefaultValue = 8192)]
        public int SocketBufferSize
        {
            get { return (int)this["socketBufferSize"]; }
        }

        /// <summary>
        /// Message Buffer Size. default 1024 bytes
        /// </summary>
        [ConfigurationProperty("messageBufferSize", IsRequired = false, DefaultValue = 8192)]
        public int MessageBufferSize
        {
            get { return (int)this["messageBufferSize"]; }
        }

        /// <summary>
        /// max message size. default 4MB
        /// </summary>
        [ConfigurationProperty("maxMessageSize", IsRequired = false, DefaultValue = 1024 * 1024 * 4)]
        public int MaxMessageSize
        {
            get { return (int)this["maxMessageSize"]; }
        }

        /// <summary>
        /// 최대 접속 가능 수.  default 20,000
        /// </summary>
        [ConfigurationProperty("maxConnections", IsRequired = false, DefaultValue = 20000)]
        public int MaxConnections
        {
            get { return (int)this["maxConnections"]; }
        }

        /// <summary>
        /// ServiceType
        /// </summary>
        [ConfigurationProperty("serviceType", IsRequired = true)]
        public string ServiceType
        {
            get { return (string)this["serviceType"]; }
        }

        /// <summary>
        /// 프로토콜.  default 명령줄 프로토콜
        /// </summary>
        [ConfigurationProperty("protocol", IsRequired = false, DefaultValue = "commandLine")]
        public string Protocol
        {
            get { return (string)this["protocol"]; }
        }
    }
}