namespace OmokAPIServer.Models
{
    public class ClientSession
    {
        public string Nickname { get; set; }
        public ulong PlayerId { get; set; }
        public string LastTime { get; set; }
        public string ExpireTime { get; set; }
        public string AuthToken { get; set; }
    }
}