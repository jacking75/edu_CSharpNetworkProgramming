namespace OmokAPIServer.Models.Msg
{
    public class ResLogin
    {
        public uint ResultCode { get; set; }
        public string SessionLastTime { get; set; }
        public string SessionExpireTime { get; set; }
        public string AuthToken { get; set; }
    }
}