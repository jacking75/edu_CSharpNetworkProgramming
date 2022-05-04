using System.Configuration;

namespace FastSocketLite.Server.Config
{
    /// <summary>
    /// socket server config.
    /// </summary>
    public class SocketServerConfig : ConfigurationSection
    {
        [ConfigurationProperty("servers", IsRequired = true)]
        public ServerCollection Servers
        {
            get { return this["servers"] as ServerCollection; }
        }
    }
}