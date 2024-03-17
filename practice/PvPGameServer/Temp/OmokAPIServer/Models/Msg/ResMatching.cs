namespace OmokAPIServer.Models.Msg
{
    public class ResMatching
    {
        public uint ResultCode { get; set; }
        public string SessionLastTime { get; set; }
        public string SessionExpireTime { get; set; }
    }
}