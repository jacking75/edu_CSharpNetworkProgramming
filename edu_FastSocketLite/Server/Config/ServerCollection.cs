using System.Configuration;

namespace FastSocketLite.Server.Config
{
    /// <summary>
    /// 서버 컬렉션
    /// </summary>
    [ConfigurationCollection(typeof(Server), AddItemName = "server")]
    public class ServerCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new Server();
        }
        
        protected override object GetElementKey(ConfigurationElement element)
        {
            var server = element as Server;
            return server.Name;
        }

        /// <summary>
        /// 지정된 위치에서 객체를 가져온다.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public Server this[int i]
        {
            get { return BaseGet(i) as Server; }
        }
    }
}