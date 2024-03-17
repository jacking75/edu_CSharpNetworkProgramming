using Dapper.Contrib.Extensions;
namespace OmokAPIServer.Models.Table
{
    [Table("t_user")]
    public class User
    {
        public static string SelectQueryOne = "select * from t_user where nickname = @nickname ";
        
        [ExplicitKey]
        public string nickname { get; set; }
        public ulong player_id { get; set; }
        public string did { get; set; }
        public string client_ip { get; set; }
        
    }
}