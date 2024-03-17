using OmokAPIServer.Models.Redis;

namespace OmokAPIServer.Models.Msg
{
    public class ResCheckMatching
    {
        public uint ResultCode { get; set; }
        public string SessionLastTime { get; set; }
        public string SessionExpireTime { get; set; }
        public string IP { get; set; }
        public int Port { get; set; }
        public int RoomNumber { get; set; }
        public int Index { get; set; }
        public string Token { get; set; }
    }
}