namespace OmokAPIServer.Models.Msg
{
    public class ReqMatching
    {
        public string Nickname { get; set; }
        public string AuthToken { get; set; }
        public bool IsMatching { get; set; }
    }
}