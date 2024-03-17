namespace OmokAPIServer.Models.Msg
{
    public class ReqLogin
    {
        public string Nickname { get; set; }
        public ulong PlayerId { get; set; }
        public string Did { get; set; }
    }
}