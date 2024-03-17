namespace OmokAPIServer.Models.Msg
{
    public class ReqUpdateSession
    {
        public string Nickname { get; set; }
        public ulong PlayerId { get; set; }
        public string AuthToken { get; set; }
    }
}