namespace OmokAPIServer.Models.Param
{
    public class UpdateUserParam
    {
        public string Nickname { get; set; }
        public ulong PlayerId { get; set; }
        public string Did { get; set; }
        public string ClientIp { get; set; }
    }
}